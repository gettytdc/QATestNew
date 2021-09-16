using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.BrowserAutomation.WebMessages;
using BluePrism.BrowserAutomation.Events;
using BluePrism.BrowserAutomation.NamedPipe;
using BluePrism.NamedPipes;
using BluePrism.Core.Extensions;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using BluePrism.BrowserAutomation.WebMessages.Events;
using NLog;

namespace BluePrism.BrowserAutomation.NativeMessaging
{
    public class NativeMessagingWebPageProvider : IWebPageProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly INamedPipeWrapper _namedPipeWrapper;
        private readonly Func<Guid, Action<WebMessageWrapper>, IWebPage> _webPageFactory;
        private readonly ConcurrentDictionary<Guid, IWebPage> _activeWebPages = new ConcurrentDictionary<Guid, IWebPage>();
        private readonly ConcurrentDictionary<string, List<Guid>> _trackedWebPages = new ConcurrentDictionary<string, List<Guid>>();
        private readonly Action<WebMessageWrapper> _sendMessage;
        private readonly IWebPage _connectionWebPage;

        private IntPtr _procHandle = IntPtr.Zero;

        public event  TrackingIdDetachedDelegate OnTrackingIdDetached;

        public NativeMessagingWebPageProvider(
            Func<Guid, Action<WebMessageWrapper>, IWebPage> webPageFactory,
            INamedPipeWrapper webSocketServerWrapper)
        {
            _webPageFactory = webPageFactory;

            _namedPipeWrapper = webSocketServerWrapper;
            _sendMessage = message => _namedPipeWrapper.SendMessage(message);

            _connectionWebPage = _webPageFactory(Guid.NewGuid(), _sendMessage);
            _connectionWebPage.ElementHover += OnElementHover;

            _namedPipeWrapper.MessageReceived += OnServerMessageReceived;
            _namedPipeWrapper.PipeDisposed += PipeDisposed;
            Log.Info("Web Factory created");
        }

        //Constructor used for unit testing purposes
        public NativeMessagingWebPageProvider(ConcurrentDictionary<Guid, IWebPage> activeWebPages, ConcurrentDictionary<string, List<Guid>> trackedPages)
        {
            _activeWebPages = activeWebPages;
            _trackedWebPages = trackedPages;
        }

        private static class ServerMessages
        {
            public const string Attached = "Attached";
            public const string Detached = "Detached";
            public const string Launch = "Launch";
            public const string PageDisconnected = "Page disconnected";
            public const string SessionStarted = "Session Started";
        }

        private void PipeDisposed(object sender, PipeDisposedDelegateEventArgs args)
        {
            Log.Info("Pipe Disposed, clearing active webpages");
            CloseActivePages();
        }

        private void OnServerMessageReceived(object sender, WebMessageReceivedDelegateEventArgs args)
        {
            switch (args.Message.Data)
            {
                case ServerMessages.Launch:
                case ServerMessages.Attached:
                    Log.Info($"Attached/Launch message received pages count: {args.Message.Pages.Count}");
                    CreateAndTrackPages(args);
                    CheckExtensionVersion(args.Message.ExtensionVersion);
                    break;
                case ServerMessages.Detached:
                    Log.Info("OnServerMessageReceived Detached");
                    foreach (var pageId in args.Message.Pages)
                    {
                        DisconnectPage(pageId);
                    }

                    if (!_activeWebPages.Any() && !_trackedWebPages.Any() && _procHandle != IntPtr.Zero)
                    {
                        SetEvent(_procHandle);
                        _procHandle = IntPtr.Zero;
                    }
                    break;
                case ServerMessages.PageDisconnected:
                    Log.Info("OnServerMessageReceived PageDisconnected");
                    DisconnectPage(args.Message.PageId);
                    break;
                case ServerMessages.SessionStarted:
                    Log.Info("OnServerMessageReceived Session Started");
                    CheckExtensionVersion(args.Message.ExtensionVersion);
                    break;
                default:
                    // We check that we have a real pageId as null messages from timed out SendMessageAndWaitForResponse can cause pages with empty GUIDs...
                    if (args.Message.PageId != Guid.Empty)
                    {
                        GetOrCreatePage(args.Message.PageId).ReceiveMessage(args.Message);
                    }
                    break;
            }
        }

        private void CheckExtensionVersion(string extensionVersion) => OnWebPageCreated?.Invoke(this,
                VersionExtensions.TryParseVersionString(extensionVersion, out var test)
                    ? new WebPageCreatedEventArgs(test)
                    : new WebPageCreatedEventArgs(new Version(0, 0, 0, 0)));

        private void OnElementHover(object sender, ElementHoverArgs args) =>
            ElementUnderCursor = args.Element;

        public IWebElement ElementUnderCursor { get; private set; }

        public event WebPageCreatedDelegate OnWebPageCreated;

        private void CreateAndTrackPages(WebMessageReceivedDelegateEventArgs args)
        {
            foreach (var pageId in args.Message.Pages)
            {
                if (!_activeWebPages.ContainsKey(pageId))
                {
                    CreatePage(pageId);                    
                }

                if (!string.IsNullOrEmpty(args.Message.TrackingId) && args.Message.TrackingId != Guid.Empty.ToString() &&
                    !_trackedWebPages.Values.Any(x => x.Contains(pageId)))
                {
                    if (_trackedWebPages.Keys.All(x => x != args.Message.TrackingId))
                    {
                        _trackedWebPages[args.Message.TrackingId] = new List<Guid> { pageId };
                    }
                    else
                    {
                        _trackedWebPages[args.Message.TrackingId].Add(pageId);
                    }
                }
            }        
        }

        private IWebPage GetOrCreatePage(Guid pageId)
        {
            if (_activeWebPages.ContainsKey(pageId))
            {
                return _activeWebPages[pageId];
            }
            return CreatePage(pageId);
        }

        private IWebPage CreatePage(Guid pageId)
        {            
            var newPage = _webPageFactory(pageId, _sendMessage);
            newPage.ElementHover += OnElementHover;
            Log.Info($"Added page {pageId} to collection");
            return _activeWebPages[pageId] = newPage;
        }

        public void CloseActivePages()
        {
            Log.Info("Closing all pages");
            _activeWebPages.Clear();
            _trackedWebPages.Clear();
        }

        public void DisconnectPage(Guid pageId)
        {
            Log.Info($"Disconnecting page {pageId}");
            _activeWebPages.TryRemove(pageId, out var deletedPage);
            if (deletedPage != null)
            {
                var trackingId = _trackedWebPages.FirstOrDefault(x => x.Value.Contains(pageId)).Key;
                if (trackingId != null)
                {
                    _trackedWebPages[trackingId].Remove(pageId);
                    if (!_trackedWebPages[trackingId].Any())
                    {
                        _trackedWebPages.TryRemove(trackingId, out var deletedTrack);
                    }
                }
            }
            if (ElementUnderCursor?.Page.Id == pageId)
            {
                ElementUnderCursor = null;
            }
        }

        public IReadOnlyCollection<IWebPage> GetActiveWebPages(string trackingId = "")
        {
            if (string.IsNullOrEmpty(trackingId))
            {
                Log.Info($"Returning {_activeWebPages.Count} pages, no tracking ID");
                return _activeWebPages.Values.ToList();
            }

            _trackedWebPages.TryGetValue(trackingId, out var listOfTrackedPages);

            var mappedActivePages = new List<IWebPage>();

            //Add the pages covered by the tracking ID to the collection
            mappedActivePages.AddRange(_activeWebPages.Where(x => _trackedWebPages.Where(tr => tr.Key == trackingId).SelectMany(t => t.Value).Any(tx => tx == x.Key)).Select(x => x.Value));
                        
            //Add the untracked pages to the collection
            mappedActivePages.AddRange(_activeWebPages.Where(x => !_trackedWebPages.SelectMany(t => t.Value).Any(tx => tx == x.Key)).Select(x => x.Value));

            Log.Info($"Returning {mappedActivePages.Count} pages, tracking ID {trackingId}");
            return mappedActivePages;
        }

        public void SetProcHandle(IntPtr procHandle) => _procHandle = procHandle;

        public void DetachTrackingId(string trackingId)
        {
            if (_trackedWebPages.TryRemove(trackingId, out var removedPage))
            {
                Log.Info($"DetachTrackingId pages found to remove {removedPage.Count}, tracking ID {trackingId}");
                OnTrackingIdDetached?.Invoke(this, new TrackingIdDetachedEventArgs(trackingId));
                //remove the pages associated with the tracking ID from the active pages collection
                foreach (var page in removedPage)
                {
                    Log.Info($"DetachTrackingId removing page {page}");
                    _activeWebPages.TryRemove(page, out _);
                }
            }
        }
        public void DetachAllTrackedPages()
        {
            var keys = _trackedWebPages.Keys;
            foreach (var trackingId in keys)
            {
                DetachTrackingId(trackingId);                
            }
        }

        public bool IsTracking() => _trackedWebPages.Any();

        public bool IsTracking(string trackingId)
        {
            return _trackedWebPages.Any(x => x.Key == trackingId);
        }

        public IWebPage ActiveMessagingHost() => _connectionWebPage;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _namedPipeWrapper.Dispose();
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetEvent(IntPtr hEvent);
    }
}
