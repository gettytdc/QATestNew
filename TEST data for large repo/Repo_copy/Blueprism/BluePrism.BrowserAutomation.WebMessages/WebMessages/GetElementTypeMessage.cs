namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementTypeMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementType;
        }

        public GetElementTypeMessage()
        {
        }

        public GetElementTypeMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementTypeResponse : WebMessage<string>
    {
        public string Type => Data;
    }
}
