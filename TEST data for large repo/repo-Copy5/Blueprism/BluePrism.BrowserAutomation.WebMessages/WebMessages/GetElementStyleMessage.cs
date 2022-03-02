namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementStyleMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementStyle;
        }

        public GetElementStyleMessage()
        {
        }

        public GetElementStyleMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementStyleResponse : WebMessage<string>
    {
        public string Style => Data;
    }
}
