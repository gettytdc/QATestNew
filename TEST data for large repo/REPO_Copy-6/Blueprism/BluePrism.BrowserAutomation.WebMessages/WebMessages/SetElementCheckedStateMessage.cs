namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    public class SetElementCheckedStateMessage : WebMessage<SetElementCheckedStateMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.SetCheckedState;
        }

        public SetElementCheckedStateMessage()
        {
        }

        public SetElementCheckedStateMessage(Guid elementId, bool value)
        {
            Data = new SetElementCheckedStateMessageBody
            {
                ElementId = elementId,
                Value = value
            };
        }

    }

    public class SetElementCheckedStateMessageBody
    {
        public Guid ElementId { get; set; }
        public bool Value { get; set; }
    }
}
