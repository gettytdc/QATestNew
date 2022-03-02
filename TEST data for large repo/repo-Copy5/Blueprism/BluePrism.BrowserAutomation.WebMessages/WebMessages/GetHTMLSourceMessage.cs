namespace BluePrism.BrowserAutomation.WebMessages
{
    public class GetHtmlSourceMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetHTMLSource;
        }
    }

    public class GetHtmlSourceResponse : WebMessage<string>
    {
        public string HTMLSource => Data;
    }
}
