namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementIsVisibleMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementIsVisible;
        }

        public GetElementIsVisibleMessage()
        {
        }

        public GetElementIsVisibleMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementIsVisibleResponse : WebMessage<bool>
    {
        public bool IsVisible => Data;
    }
}
