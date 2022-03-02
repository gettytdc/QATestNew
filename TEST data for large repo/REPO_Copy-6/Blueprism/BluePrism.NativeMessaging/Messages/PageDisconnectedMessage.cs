using System;
using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
    public class PageDisconnectedMessage
    {                
        [JsonProperty("PageId")]
        public Guid PageId { get; set; }
        
        [JsonProperty("Data")]
        public string MessageType { get; set; }
    }
}
