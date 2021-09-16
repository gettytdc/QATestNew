namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementPathMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementPath;
        }

        public GetElementPathMessage()
        {
        }

        public GetElementPathMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementPathResponse : WebMessage<string>
    {
        public string Path => Data;
    }
}
