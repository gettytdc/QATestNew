using BluePrism.NativeMessaging.Interfaces;
using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
    public class BPPageConnectedResponseMessage : IBPMessage
    {
        [JsonProperty("data")]
        public const string Data = "attached";
        [JsonProperty("bpClientId")]
        public string BpClientId { get;}
        [JsonProperty("title")]
        public const string Title = null;
        [JsonProperty("trackingId")]
        public string TrackingId { get;}
        [JsonProperty("extensionVersion")]
        public string ExtensionVersion { get;}
        [JsonProperty("conversationId")]
        public const string ConversationId = null;

        public BPPageConnectedResponseMessage(string bpClientId, string trackingId, string extensionVersion)
        {
            BpClientId = bpClientId;
            TrackingId = trackingId;
            ExtensionVersion = extensionVersion;
        }
    }
}
