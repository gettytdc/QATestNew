using BluePrism.NativeMessaging.Interfaces;
using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
    public class MessageAcknowledgedMessage : IResponseMessage
    {
        [JsonProperty("name")]
        public const string Name = "Acknowledge";

        public MessageAcknowledgedMessage()
        {
        }
    }
}
