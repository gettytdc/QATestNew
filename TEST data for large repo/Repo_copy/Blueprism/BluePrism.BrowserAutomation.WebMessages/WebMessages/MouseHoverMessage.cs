namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class MouseHoverMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.MouseHover;
        }

        public MouseHoverMessage()
        {
        }

        public MouseHoverMessage(Guid elementId)
        {
            Data = elementId;
        }

        public Guid ElementId => Data;
    }
}
