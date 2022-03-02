
namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class SubmitElementMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.SubmitElement;
        }

        public SubmitElementMessage()
        {
        }

        public SubmitElementMessage(Guid elementId)
        {
            Data = elementId;
        }
    }
}
