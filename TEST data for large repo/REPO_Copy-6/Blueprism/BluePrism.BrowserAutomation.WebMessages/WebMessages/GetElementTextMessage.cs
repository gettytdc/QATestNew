namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementTextMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementText;
        }

        public GetElementTextMessage()
        {
        }

        public GetElementTextMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementTextResponse : WebMessage<string>
    {
        public string Text => Data;
    }
}
