namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementNameMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementName;
        }

        public GetElementNameMessage()
        {
        }

        public GetElementNameMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementNameResponse : WebMessage<string>
    {
        public string Name => Data;
    }
}
