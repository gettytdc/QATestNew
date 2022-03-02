namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementIsEditableMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementIsEditable;
        }

        public GetElementIsEditableMessage()
        {
        }

        public GetElementIsEditableMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementIsEditableResponse : WebMessage<bool>
    {
        public bool IsEditable => Data;
    }
}
