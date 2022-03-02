namespace BluePrism.BrowserAutomation.WebMessages
{
    public class GetElementByPathMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementByPath;
        }

        public GetElementByPathMessage()
        {
        }

        public GetElementByPathMessage(string selector)
        {
            Data = selector;
        }
    }

    public class GetElementByPathResponse : BaseElementResponse
    {
    }
}
