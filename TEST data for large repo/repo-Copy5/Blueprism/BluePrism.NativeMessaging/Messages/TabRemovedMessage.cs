using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
    public class TabRemovedMessage
    {
        [JsonProperty("TabID")]
        public int TabId { get; set; }

        [JsonProperty("Data")]
        public string MessageType { get; set; }
    }
}
