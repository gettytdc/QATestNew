namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class WebMessage : IWebMessage
    {
        public Guid ConversationId { get; set; } = Guid.NewGuid();
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual MessageType MessageType { get; set; }
    }

    public class WebMessage<TMessageBody> : WebMessage
    {
        public TMessageBody Data { get; set; }
    }
}
