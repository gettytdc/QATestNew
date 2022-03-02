

namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    public class DoubleClickElementMessage : WebMessage<Guid>
    {

        public override MessageType MessageType
        {
            get => MessageType.DoubleClickElement;
        }

        public DoubleClickElementMessage()
        {
        }

        public DoubleClickElementMessage(Guid elementId)
        {
            Data = elementId;
        }

    }
}
