namespace BluePrism.BrowserAutomation.WebMessages
{
    public class GetElementsByTypeMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementsByType;
        }

        public GetElementsByTypeMessage()
        {
        }

        public GetElementsByTypeMessage(string type)
        {
            Data = type;
        }
    }

    public class GetElementsByTypeResponse : BaseElementsResponse
    {
    }
}
