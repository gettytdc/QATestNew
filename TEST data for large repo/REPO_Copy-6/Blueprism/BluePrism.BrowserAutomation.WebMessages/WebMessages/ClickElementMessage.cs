namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class ClickElementMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.ClickElement;
        }

        public ClickElementMessage()
        {
        }

        public ClickElementMessage(Guid elementId)
        {
            Data = elementId;
        }
    }
}
