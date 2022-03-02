namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    public class GetElementSelectedTextMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementSelectedText;
        }

        public GetElementSelectedTextMessage()
        { }

        public GetElementSelectedTextMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementSelectedTextResponse : WebMessage<string>
    {
        public string Text => Data;
    }
}
