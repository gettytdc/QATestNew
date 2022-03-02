namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class HoverElementMessage : WebMessage<Guid>
    {
        public override MessageType MessageType => MessageType.HoverElement;

        public HoverElementMessage()
        {
        }

        public HoverElementMessage(Guid elementId)
        {
            Data = elementId;
        }
    }
}
