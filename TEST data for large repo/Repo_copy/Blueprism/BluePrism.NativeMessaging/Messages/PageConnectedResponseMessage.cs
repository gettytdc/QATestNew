using System;
using BluePrism.NativeMessaging.Interfaces;
using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
   public class PageConnectedResponseMessage : IResponseMessage
    {
        [JsonProperty("pageId")]
        public Guid PageId { get;}
        [JsonProperty("message")]
        public const string Message = "acknowledged";

        public PageConnectedResponseMessage(Guid pageId)
        {
            PageId = pageId;
        }
    }
}
