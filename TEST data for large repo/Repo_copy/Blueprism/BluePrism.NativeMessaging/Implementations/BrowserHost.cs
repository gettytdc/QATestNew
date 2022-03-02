using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BluePrism.NativeMessaging.Browser;
using BluePrism.NativeMessaging.EventArgs;
using BluePrism.NativeMessaging.Interfaces;
using BluePrism.NativeMessaging.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace BluePrism.NativeMessaging.Implementations
{
    public class BrowserHost : INativeMessagingHost
    {
        public event EventHandler LostConnection;
        public event MessageReceivedHandler MessageReceived;
        public List<Session> Sessions { get; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly object _sendLock = new object();

        /// <summary>
        /// Creates the ChromeHost Object
        /// </summary>
        public BrowserHost() => Sessions = new List<Session>();

        /// <summary>
        /// Starts listening for input.
        /// </summary>
        public void Listen()
        {
            JObject data;
            while ((data = Read()) != null)
            {   
                SendAcknowledge();
                ProcessReceivedMessage(data);
            }
            Log.Info("Listen exited");
            LostConnection?.Invoke(this, System.EventArgs.Empty);
        }

        public Session GetSessionForPage(string pageId)
        {
            if (Guid.TryParse(pageId, out var pageGuid))
            {
                var session = from s in Sessions from tab in s.Tabs where tab.Pages.Contains(pageGuid) select s;
                return session.FirstOrDefault();
            }

            throw new InvalidOperationException($"Invalid Page ID: {pageId}");
        }

        public Session GetSessionForTab(int tabId) => (from s in Sessions from tab in s.Tabs where tab.Id == tabId select s).FirstOrDefault();

        public void UnassociatePages(List<Guid> pages)
        {
            foreach (var page in pages)
            {
                var associatedSessions = from s in Sessions from tab in s.Tabs where tab.Pages.Contains(page) select s;
                associatedSessions.ToList().ForEach(s => UnassociateSession(s));
            }
        }

        private void UnassociateSession(Session session)
        {
            session.BpClientId = Guid.Empty.ToString();
            session.TrackingId = string.Empty;
        }

        public List<Guid> GetPagesForClient(string clientId)
        {
            var mySessions = Sessions.Where(x => x.BpClientId == clientId);
            var pages = new List<Guid>();
            foreach (var s in mySessions)
            {
                foreach (var t in s.Tabs)
                {
                    pages.AddRange(t.Pages);
                }
            }
            return pages;
        }

        public List<Guid> GetUnassociatedPages()
        {
            var mySessions = Sessions.Where(x => x.BpClientId == Guid.Empty.ToString());
            var pages = new List<Guid>();
            foreach (var s in mySessions)
            {
                foreach (var t in s.Tabs)
                {
                    pages.AddRange(t.Pages);
                }
            }
            return pages;
        }
        public List<Guid> GetPagesForTrackingId(string trackingId)
        {
            var mySessions = Sessions.Where(x => x.TrackingId == trackingId);
            var pages = new List<Guid>();
            foreach (var s in mySessions)
            {
                foreach (var t in s.Tabs)
                {
                    pages.AddRange(t.Pages);
                }
            }

            return pages;
        }


        public virtual JObject Read()
        {
            using (var stdin = Console.OpenStandardInput())
            {
                var lengthBytes = new byte[4];
                stdin.Read(lengthBytes, 0, 4);

                var buffer = new char[BitConverter.ToInt32(lengthBytes, 0)];

                using (var reader = new StreamReader(stdin))
                {
                    reader.Read(buffer, 0, buffer.Length);
                }

                try
                {
                    return JsonConvert.DeserializeObject<JObject>(new string(buffer));
                }
                catch (Exception ex)
                {
                    Log.Error($"ChromeHost.Read Exception: {ex}, message: {new string(buffer)}");
                    return new JObject();
                }
            }
        }

        private void SendAcknowledge()
        {
            // Send the acknowledged response to the browser
            var response = new ResponseConfirmation(new MessageAcknowledgedMessage());
            _ = SendMessage(response.GetJObject());
        }
        /// <summary>
        /// Sends a message to Chrome, note that the message might not be able to reach Chrome if the stdIn / stdOut aren't properly configured (i.e. Process needs to be started by Chrome)
        /// </summary>
        /// <param name="data"></param>
        public virtual bool SendMessage(JObject data)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(data.ToString(Formatting.None));
            lock (_sendLock)
            {
                using (var stdout = Console.OpenStandardOutput())
                {
                    stdout.WriteByte((byte)((bytes.Length >> 0) & 0xFF));
                    stdout.WriteByte((byte)((bytes.Length >> 8) & 0xFF));
                    stdout.WriteByte((byte)((bytes.Length >> 16) & 0xFF));
                    stdout.WriteByte((byte)((bytes.Length >> 24) & 0xFF));
                    stdout.Write(bytes, 0, bytes.Length);
                    stdout.Flush();
                }
            }
            return true;
        }

        public void ProcessReceivedMessage(JObject data) => MessageReceived?.Invoke(this, new MessageReceivedEventArgs { Data = data });
    }

    public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
}
