using System;
using BluePrism.NativeMessaging.Interfaces;
using Newtonsoft.Json;

namespace BluePrism.NativeMessaging.Messages
{
    public class AttachResponseMessage : IResponseMessage
    {
        [JsonProperty("name")]
        public const string Name = "Attach";
        [JsonProperty("title")]
        public string Title { get; }
        [JsonProperty("trackingId")]
        public string TrackingId { get; }
        [JsonProperty("bpClientId")]
        public string BpClientId { get; }
        [JsonProperty("conversationId")]
        public Guid ConversationId { get; }
        [JsonProperty("message")]
        public const string Message = "Attach";
        [JsonProperty("attempt")]
        public int Attempt { get; }

        public AttachResponseMessage(string title,string trackingId,string bpClientId,Guid conversationId,int attempt)
        {
            Title = title;
            TrackingId = trackingId;
            BpClientId = bpClientId;
            ConversationId = conversationId;
            Attempt = attempt;
        }
    }
}
