namespace BluePrism.BrowserAutomation.WebMessages
{
    public class RemoveHighlightingMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.RemoveHighlighting;
        }
    }
}
