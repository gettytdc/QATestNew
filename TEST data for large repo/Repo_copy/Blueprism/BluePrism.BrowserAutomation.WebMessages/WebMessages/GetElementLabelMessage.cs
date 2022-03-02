namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementLabelMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementLabel;
        }

        public GetElementLabelMessage()
        {
        }

        public GetElementLabelMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementLabelResponse : WebMessage<string>
    {
        public string Label => Data;
    }
}
