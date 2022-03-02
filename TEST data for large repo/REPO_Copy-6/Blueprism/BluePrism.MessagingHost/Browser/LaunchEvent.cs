using System.Collections.Generic;

namespace BluePrism.MessagingHost.Browser
{
    public class LaunchEvent
    {
        public string BpClientId { get; set; }

        public string TrackingId { get; set; }
        public Dictionary<string, string> LaunchedUrlDictionary { get; } = new Dictionary<string, string>();
        
        public LaunchEvent(string bpClientId, string trackingId)
        {
            BpClientId = bpClientId;
            TrackingId = trackingId;
        }
    }
}
