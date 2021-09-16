using System;
using System.Collections.Generic;
using BluePrism.NativeMessaging.Interfaces;
using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
    public class CloseWebPageResponseMessage : IResponseMessage
    {
        [JsonProperty("name")]
        public const string Name = "sendMessage";
        [JsonProperty("pages")]
        public string Pages { get; }
        [JsonProperty("message")]
        public const string Message = "ClosePage";

        public CloseWebPageResponseMessage(List<Guid> pages)
        { 
            Pages = JsonConvert.SerializeObject(pages);
        }
    }
}
