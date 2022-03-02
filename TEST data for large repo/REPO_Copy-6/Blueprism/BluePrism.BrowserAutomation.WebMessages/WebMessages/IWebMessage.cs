namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public interface IWebMessage
    {
        Guid ConversationId { get; set; }
        MessageType MessageType { get; set; }
    }
}
