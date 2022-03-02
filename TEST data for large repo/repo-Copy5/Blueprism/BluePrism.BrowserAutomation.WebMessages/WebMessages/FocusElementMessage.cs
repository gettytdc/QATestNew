namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class FocusElementMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.FocusElement;
        }

        public FocusElementMessage()
        {
        }

        public FocusElementMessage(Guid elementId)
        {
            Data = elementId;
        }
    }
}
