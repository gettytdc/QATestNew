using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using BluePrism.BPCoreLib;
using BluePrism.BrowserAutomation.WebMessages;
using BluePrism.NativeMessaging.EventArgs;
using BluePrism.NativeMessaging.Implementations;
using BluePrism.NativeMessaging.Interfaces;
using BluePrism.NativeMessaging.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace BluePrism.MessagingHost.Browser
{
    public class Host
    {
        public INativeMessagingHost MessagingHost { get; set; }

        public event MessageReceivedHandler MessageReceived;
        public event MessageReceivedHandler Attached;
        public event MessageReceivedHandler Detached;
        public bool AllPagesDisconnected => !MessagingHost.Sessions.Any();

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly List<LaunchEvent> _launchEvents = new List<LaunchEvent>();

        public Host(INativeMessagingHost messagingHost)
        {
            MessagingHost = messagingHost;
            MessagingHost.MessageReceived += HostMessageReceived;
        }

        private void HostMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            var messageType = string.Empty;
            try
            {
                messageType = args.Data["data"]?["data"]?.ToString();
            }
            catch
            {
                // fall through
            }

            ProcessHostMessage(args, messageType);
        }

        private void ProcessHostMessage(MessageReceivedEventArgs args, string messageType)
        {
            switch (messageType)
            {
                case "Page connected":
                    ConnectPage(args.Data);
                    break;
                case "Page disconnected":
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs { Data = args.Data });
                    DisconnectPage(args.Data);
                    break;
                case "Tab removed":
                    RemoveTab(args.Data);
                    break;
                case "attached":
                    AttachPagesToBluePrism(args.Data);
                    break;
                default: //probably "sendMessage":
                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs { Data = args.Data });
                    break;
            }
        }

        private void ConnectPage(JObject data)
        {
            var messageData = data["data"];
            if (messageData == null)
            {
                return;
            }

            var pageConnectedMessage = JsonConvert.DeserializeObject<PageConnectedMessage>(messageData.ToString());
            Log.Info($"Connect Page recieved for page: {pageConnectedMessage.PageId}");

            // Send the acknowledged response to the browser
            var response = new ResponseConfirmation(new PageConnectedResponseMessage(pageConnectedMessage.PageId));
            MessagingHost.SendMessage(response.GetJObject());

            var existingSession = MessagingHost.Sessions.FirstOrDefault(session => session.SessionId == pageConnectedMessage.SessionId);
            if (existingSession == null)
            {
                Log.Info($"Session does not exist for page: {pageConnectedMessage.PageId}");
                CreateSession(pageConnectedMessage);
            }
            else
            {
                Log.Info($"Session exists for page: {pageConnectedMessage.PageId}, Session: {existingSession.SessionId}");
                var attachData = new BPMessageWrapper(new BPPageConnectedResponseMessage(existingSession.BpClientId, existingSession.TrackingId,pageConnectedMessage.ExtensionVersion));
                Attached?.Invoke(this, new MessageReceivedEventArgs { Data = attachData.GetJObject(), Pages = new List<Guid> { pageConnectedMessage.PageId } });

                var existingTab = existingSession.Tabs.FirstOrDefault(tabs => tabs.Id == pageConnectedMessage.TabId);
                if (existingTab == null)
                {
                    Log.Info($"Tab does not exist for page: {pageConnectedMessage.PageId}, creating tab");
                    var tab = new NativeMessaging.Browser.Tab(pageConnectedMessage.TabId, pageConnectedMessage.TabTitle);
                    tab.Pages.Add(pageConnectedMessage.PageId);
                    Log.Info($"Page: {pageConnectedMessage.PageId} add to tab {tab.Id}");
                    existingSession.Tabs.Add(tab);
                    Log.Info($"Tab: {tab.Id} add to Session: {existingSession.SessionId}");

                }
                else
                {
                    Log.Info($"Tab exists for page: {pageConnectedMessage.PageId}");
                    if (!existingTab.Pages.Contains(pageConnectedMessage.PageId))
                    {                        
                        existingTab.Pages.Add(pageConnectedMessage.PageId);
                        Log.Info($"Page: {pageConnectedMessage.PageId} add to tab {existingTab.Id}");
                        existingTab.Title = pageConnectedMessage.TabTitle;
                    }
                }
            }
        }


        private void CreateSession(PageConnectedMessage pageConnectedMessage)
        {
            Log.Info($"Creating session for {pageConnectedMessage.PageId}");
            var url = GetUrlPath(pageConnectedMessage.Url);

            var parentSession = MessagingHost.GetSessionForTab(pageConnectedMessage.ParentTabId);

            var session = new NativeMessaging.Browser.Session(pageConnectedMessage.SessionId, parentSession?.BpClientId ?? Guid.Empty.ToString());

            var tab = new NativeMessaging.Browser.Tab(pageConnectedMessage.TabId, pageConnectedMessage.TabTitle);
            tab.Pages.Add(pageConnectedMessage.PageId);

            session.Tabs.Add(tab);
            session.TrackingId = parentSession?.TrackingId ?? Guid.Empty.ToString();

            MessagingHost.Sessions.Add(session);

            var launchEvent = _launchEvents.FirstOrDefault(l => l.LaunchedUrlDictionary.ContainsKey(url) || l.LaunchedUrlDictionary.Keys.Any(u => url.Contains(u)));
            if (launchEvent != null)
            {
                Log.Info(
                    $"Launch event found - BpClientId {launchEvent.BpClientId}, trackingId {launchEvent.TrackingId}, url {url}");

                var urlToRemove = launchEvent.LaunchedUrlDictionary.ContainsKey(url)
                    ? url
                    : launchEvent.LaunchedUrlDictionary.Keys.First(u => url.Contains(u));

                session.BpClientId = launchEvent.LaunchedUrlDictionary[urlToRemove];
                session.TrackingId = launchEvent.TrackingId;
                launchEvent.LaunchedUrlDictionary.Remove(urlToRemove);
                if (launchEvent.LaunchedUrlDictionary.Count == 0)
                {
                    _launchEvents.Remove(launchEvent);
                }
            }

            var attachData = new BPMessageWrapper(new BPPageConnectedResponseMessage(session.BpClientId, session.TrackingId, pageConnectedMessage.ExtensionVersion));
            Attached?.Invoke(this, new MessageReceivedEventArgs { Data = attachData.GetJObject(), Pages = new List<Guid> { pageConnectedMessage.PageId } });
        }
              
        private void DisconnectPage(JObject data)
        {
            var messageData = data["data"];
            if (messageData == null)
            {
                return;
            }
            var pageDisconnectedMessage = JsonConvert.DeserializeObject<PageConnectedMessage>(messageData.ToString());

            foreach (var session in MessagingHost.Sessions)
            {
                foreach (var tab in session.Tabs)
                {
                    if (tab.Pages.Contains(pageDisconnectedMessage.PageId))
                    {
                        tab.Pages.Remove(pageDisconnectedMessage.PageId);
                        break;
                    }
                }
            }
        }

        private void RemoveTab(JObject data)
        {
            try
            {
                var messageData = data["data"]?.ToString();
                if (string.IsNullOrWhiteSpace(messageData))
                {
                    return;
                }

                var tabRemovedMessage = JsonConvert.DeserializeObject<TabRemovedMessage>(messageData);

                var session =
                    MessagingHost.Sessions.FirstOrDefault(s => s.Tabs.Any(t => t.Id == tabRemovedMessage.TabId));

                if (session != null)
                {
                    var bpClientId = session.BpClientId;
                    var tab = session.Tabs.FirstOrDefault(t => t.Id == tabRemovedMessage.TabId);
                    if (tab != null)
                    {
                        var pages = new List<Guid>();
                        pages.AddRange(tab.Pages);
                        session.Tabs.Remove(tab);
                        if (session.Tabs.Count == 0)
                        {
                            MessagingHost.Sessions.Remove(session);
                        }
                        Detached?.Invoke(this,
                            new MessageReceivedEventArgs
                            {
                                Data = JObject.Parse($"{{ BpClientId: \"{bpClientId}\"}}"),
                                Pages = pages
                            });
                    }
                }
            }
            catch (Exception e) { Log.Error(e); }
        }
        private void AttachPagesToBluePrism(JObject data)
        {
            // The Attach in the background page just returns the entire collection of pages
            // we only attach the pages we want, the original message forwards the all and name params back to us
            // but we should only associate unassociated pages
            var messageData = data["data"];
            if (messageData == null || string.IsNullOrWhiteSpace(messageData["bpClientId"]?.ToString()))
            {
                return;
            }

           
            var bpClientId = messageData["bpClientId"]?.ToString();
            var trackingId = messageData["trackingId"]?.ToString();
            var title = messageData["title"]?.ToString();
           
            int.TryParse(messageData["attempt"]?.ToString(), out var attempt);

            Guid.TryParse(messageData["conversationId"]?.ToString(), out var conversationId);

            if (!string.IsNullOrEmpty(bpClientId) && messageData["pages"]?.ToString() != "[]")
            {
                AttachPages(data, messageData, title, bpClientId, trackingId);
            }
            else if (!string.IsNullOrEmpty(bpClientId) && attempt < 5)
            {
                //lets try resending the attach message to see if we get pages this time
                var attachMessage = new AttachMessage(BrowserType.Chrome, title, trackingId)
                {
                    ConversationId = conversationId,
                    Data = {Attempt = attempt + 1}
                };
                Thread.Sleep(2000);
                AttachToBrowser(attachMessage, bpClientId);
            }
        }

        private void AttachPages(JObject data, JToken messageData, string title, string bpClientId, string trackingId)
        {
            var pages = messageData["pages"];

            if (pages == null)
            {
                return;
            }

            var pagesList = new List<Guid>();
            var pagesToAttach = new List<Guid>();

            pagesList.AddRange(pages.Select(resultString => Guid.Parse(resultString.ToString())));

            foreach (var page in pagesList)
            {
                var session = MessagingHost.GetSessionForPage(page.ToString());
                if (session != null && (session.BpClientId == Guid.Empty.ToString() || session.BpClientId == bpClientId) &&
                    (string.IsNullOrWhiteSpace(title) || FindMatchingTitle(title, session)))
                {
                    session.BpClientId = bpClientId;
                    session.TrackingId = trackingId ?? string.Empty;
                    pagesToAttach.Add(page);
                }
            }

            if (pagesToAttach.Any())
            {
                Attached?.Invoke(this, new MessageReceivedEventArgs {Data = data, Pages = pagesToAttach});
            }
        }

        private bool FindMatchingTitle(string title, NativeMessaging.Browser.Session session) => session.Tabs.Any(t => Regex.IsMatch(t.Title, WildCardToRegular(title), RegexOptions.None, RegexTimeout.DefaultRegexTimeout));

        private string WildCardToRegular(string value) => "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";

        public bool AttachToBrowser(AttachMessage command, string bpClientId)
        {
            if (!string.IsNullOrEmpty(bpClientId))
            {
                var response = new ResponseConfirmation(new AttachResponseMessage(command.Data.TabName, command.Data.TrackingId, bpClientId, command.ConversationId, command.Data.Attempt));
                return MessagingHost.SendMessage(response.GetJObject());
            }

            return false;
        }

        public bool LaunchBrowser(LaunchMessage command, string bpClientId)
        {
            var urls = command.Data.Urls.Split(new[] { ' ' }).ToList();
            var launchEvent = new LaunchEvent(bpClientId, command.Data.TrackingId);
            foreach (var url in urls)
            {
                launchEvent.LaunchedUrlDictionary.Add(GetUrlPath(url), bpClientId);
            }
            _launchEvents.Add(launchEvent);

            var response = new ResponseConfirmation(new LaunchResponseMessage(command.Data.Urls));
            return MessagingHost.SendMessage(response.GetJObject());
        }

        public void DetachBrowser(DetachMessage command, string bpClientId)
        {
            var pages = string.IsNullOrEmpty(command.Data.TrackingId) ?
                MessagingHost.GetPagesForClient(bpClientId) :
                MessagingHost.GetPagesForTrackingId(command.Data.TrackingId);
            MessagingHost.UnassociatePages(pages);
        }

        public void CloseWebPages(CloseWebPageMessage closeWebPageMessage, string bluePrismClientId)
        {
            var myPages = string.IsNullOrEmpty(closeWebPageMessage.Data.TrackingId) ?
                MessagingHost.GetPagesForClient(bluePrismClientId) :
                MessagingHost.GetPagesForTrackingId(closeWebPageMessage.Data.TrackingId);

            var response = new ResponseConfirmation(new CloseWebPageResponseMessage(myPages));
            MessagingHost.SendMessage(response.GetJObject());
        }

        private string GetUrlPath(string url)
        {
            try
            {
                var uri = new Uri(url);
                return $"{uri.Host}{uri.LocalPath}";
            }
            catch
            {
                //Fall through
            }

            return string.Empty;

        }
        public bool SendMessageToConnectedPages(JObject messageObject, string bpClientId)
        {
            var message = messageObject["message"]?.ToString();
            //Get the message that we want to forward
            if (message != null)
            {
                var webMessage = JsonConvert.DeserializeObject<WebMessageWrapper>(message);

                if (webMessage == null)
                {
                    Log.Error("Empty WebMessage");
                    return false;
                }
                var response = new ResponseConfirmation(new SendMessageToConnectedPagesResponseMessage(webMessage.PageId,webMessage.Data));

                return MessagingHost.SendMessage(response.GetJObject());
            }
            return false;
        }
    }
}
