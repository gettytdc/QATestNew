namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class SelectElementMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.SelectElement;
        }

        public SelectElementMessage()
        {
        }

        public SelectElementMessage(Guid elementId)
        {
            Data = elementId;
        }
    }
}
