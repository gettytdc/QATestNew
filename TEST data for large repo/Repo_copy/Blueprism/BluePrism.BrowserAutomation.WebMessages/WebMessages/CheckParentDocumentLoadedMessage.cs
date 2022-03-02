namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    public class CheckParentDocumentLoadedMessage : WebMessage<Guid>
    {
        public override MessageType MessageType =>
            MessageType.CheckParentDocumentLoaded;

        public CheckParentDocumentLoadedMessage() { }

        public CheckParentDocumentLoadedMessage(Guid elementId) =>
            Data = elementId;
    }

    public class CheckParentDocumentLoadedResponse : WebMessage<bool>
    {
        public bool IsLoaded => Data;
    }
}
