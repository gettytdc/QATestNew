namespace BluePrism.BrowserAutomation.WebMessages
{
    public class InvokeJavascriptMessage : WebMessage<InvokeJavascriptMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.InvokeJavascript;
        }

        public InvokeJavascriptMessage()
        {
        }

        public InvokeJavascriptMessage(string functionName, string parameters)
        {
            Data = new InvokeJavascriptMessageBody
            {
                FunctionName = functionName,
                Parameters = parameters
            };
        }
    }

    public class InvokeJavascriptMessageBody
    {
        public string FunctionName { get; set; }
        public string Parameters { get; set; }
    }
}
