namespace BluePrism.BrowserAutomation.WebMessages
{
    public class GetElementsByClassMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementsByClass;
        }

        public GetElementsByClassMessage()
        {
        }

        public GetElementsByClassMessage(string className)
        {
            Data = className;
        }
    }

    public class GetElementsByClassResponse : BaseElementsResponse
    {
    }
}
