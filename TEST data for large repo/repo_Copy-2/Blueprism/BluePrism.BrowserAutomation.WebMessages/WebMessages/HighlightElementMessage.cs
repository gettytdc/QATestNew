namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class HighlightElementMessage : WebMessage<HighlightElementMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.HighlightElement;
        }

        public HighlightElementMessage()
        {
        }

        public HighlightElementMessage(Guid elementId, string color)
        {
            Data = new HighlightElementMessageBody
            {
                ElementId = elementId,
                Color = color
            };
        }
    }

    public class HighlightElementMessageBody
    {
        public Guid ElementId { get; set; }
        public string Color { get; set; }
    }
}
