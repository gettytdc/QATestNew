namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class SetElementValueMessage : WebMessage<SetElementValueMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.SetElementValue;
        }

        public SetElementValueMessage()
        {
        }

        public SetElementValueMessage(Guid elementId, string value)
        {
            Data = new SetElementValueMessageBody
            {
                ElementId = elementId,
                Value = value
            };
        }
    }

    public class SetElementValueMessageBody
    {
        public Guid ElementId { get; set; }
        public string Value { get; set; }
    }
}
