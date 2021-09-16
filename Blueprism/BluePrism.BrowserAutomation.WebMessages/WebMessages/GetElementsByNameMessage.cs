namespace BluePrism.BrowserAutomation.WebMessages
{
    public class GetElementsByNameMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementsByName;
        }

        public GetElementsByNameMessage()
        {
        }

        public GetElementsByNameMessage(string className)
        {
            Data = className;
        }
    }

    public class GetElementsByNameResponse : BaseElementsResponse
    {
    }
}
