namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementIsOnScreenMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementIsOnScreen;
        }

        public GetElementIsOnScreenMessage()
        {
        }

        public GetElementIsOnScreenMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementIsOnScreenResponse : WebMessage<bool>
    {
        public bool IsOnScreen => Data;
    }
}
