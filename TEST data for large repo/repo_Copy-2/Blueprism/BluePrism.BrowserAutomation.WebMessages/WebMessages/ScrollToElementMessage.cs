namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class ScrollToElementMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.ScrollToElement;
        }

        public ScrollToElementMessage()
        {
        }

        public ScrollToElementMessage(Guid elementId)
        {
            Data = elementId;
        }
    }
}
