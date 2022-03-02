using System;
using System.Collections.Generic;
using BluePrism.NativeMessaging.Browser;
using BluePrism.NativeMessaging.Implementations;
using Newtonsoft.Json.Linq;

namespace BluePrism.NativeMessaging.Interfaces
{
    public interface INativeMessagingHost
    {
        List<Session> Sessions { get; }
        Session GetSessionForPage(string pageId);
        Session GetSessionForTab(int tabId);
        List<Guid> GetPagesForClient(string clientId);
        List<Guid> GetPagesForTrackingId(string trackingId);
        List<Guid> GetUnassociatedPages();
        event EventHandler LostConnection;
        event MessageReceivedHandler MessageReceived;
        void Listen();
        JObject Read();
        bool SendMessage(JObject data);
        void ProcessReceivedMessage(JObject data);
        void UnassociatePages(List<Guid> pages);
    }
}
