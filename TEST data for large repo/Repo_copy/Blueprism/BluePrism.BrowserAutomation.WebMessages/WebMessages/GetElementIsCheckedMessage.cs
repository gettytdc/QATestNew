namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    public class GetElementIsCheckedMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementIsChecked;
        }

        public GetElementIsCheckedMessage()
        {
        }

        public GetElementIsCheckedMessage(Guid elementId)
        {
            Data = elementId;
        }

    }

    public class GetElementIsCheckedResponse : WebMessage<bool>
    {
        public bool IsChecked => Data;
    }
}
