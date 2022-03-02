using System;
using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
    public class PageConnectedMessage
    {
        [JsonProperty("sessionId")]
        public int SessionId { get; set; }

        [JsonProperty("tabID")]
        public int TabId { get; set; }

        [JsonProperty("parentTabID")]
        public int ParentTabId { get; set; }
        
        [JsonProperty("pageId")]
        public Guid PageId { get; set; }

        [JsonProperty("tabTitle")]
        public string TabTitle { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("extensionVersion")]
        public string ExtensionVersion { get; set; }

        [JsonProperty("data")]
        public string MessageType { get; set; }

    }
}
