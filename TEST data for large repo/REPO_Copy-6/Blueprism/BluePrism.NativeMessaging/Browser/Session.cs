using System;
using System.Collections.Generic;

namespace BluePrism.NativeMessaging.Browser
{
    public class Session
    {
        public Session(int sessionId, string bpClientId)
        {
            SessionId = sessionId;
            Tabs = new List<Tab>();
            BpClientId = bpClientId;
            TrackingId = Guid.Empty.ToString();
        }
        public int SessionId { get; set; }

        public List<Tab> Tabs { get; set; }

        public string BpClientId { get; set; }

        public string TrackingId { get; set; }
        
    }
}
