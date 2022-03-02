namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementIdMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementId;
        }

        public GetElementIdMessage()
        {
        }

        public GetElementIdMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementIdResponse : WebMessage<string>
    {
        public string Id => Data;
    }
}
