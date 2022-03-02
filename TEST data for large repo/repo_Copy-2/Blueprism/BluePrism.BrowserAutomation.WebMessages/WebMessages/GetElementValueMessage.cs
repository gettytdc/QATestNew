namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementValueMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementValue;
        }

        public GetElementValueMessage()
        {
        }

        public GetElementValueMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementValueResponse : WebMessage<string>
    {
        public string Value => Data;
    }
}
