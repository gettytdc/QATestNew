namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementTabIndexMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementTabIndex;
        }

        public GetElementTabIndexMessage()
        {
        }

        public GetElementTabIndexMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementTabIndexResponse : WebMessage<int>
    {
        public int TabIndex => Data;
    }
}
