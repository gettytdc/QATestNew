using System;
using BluePrism.NativeMessaging.Interfaces;
using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
    public class SendMessageToConnectedPagesResponseMessage : IResponseMessage
    {
        [JsonProperty("name")]
        public const string Name = "sendMessage";
        [JsonProperty("pageId")]
        public Guid PageId { get; }
        [JsonProperty("message")]
        public string Message { get; }
        public SendMessageToConnectedPagesResponseMessage(Guid pageId, string message)
        {
            PageId = pageId;
            Message = message;
        }
    }
}
