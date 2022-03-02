namespace BluePrism.BrowserAutomation.WebMessages
{
    public class InjectJavascriptMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.InjectJavascript;
        }

        public InjectJavascriptMessage()
        {
        }

        public InjectJavascriptMessage(string code)
        {
            Data = code;
        }
    }
}
