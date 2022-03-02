namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class SetElementAttributeMessage : WebMessage<SetElementAttributeMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.SetElementAttribute;
        }

        public SetElementAttributeMessage()
        {
        }

        public SetElementAttributeMessage(Guid elementId, string attributeName, string value)
        {
            Data = new SetElementAttributeMessageBody
            {
                ElementId = elementId,
                AttributeName = attributeName,
                Value = value
            };
        }
    }

    public class SetElementAttributeMessageBody
    {
        public Guid ElementId { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
    }
}
