namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;
    public class SetElementSelectedTextRange : WebMessage<SetElementSelectedTextBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.SetElementTextRange;
        }

        public SetElementSelectedTextRange()
        {
        }

        public SetElementSelectedTextRange(Guid elementId, int startIndex, int length)
        {
            Data = new SetElementSelectedTextBody
            {
                ElementId = elementId,
                StartIndex = startIndex,
                Length = length
            };
        }
    }

    public class SetElementSelectedTextBody
    {
        public Guid ElementId { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
    }
}
