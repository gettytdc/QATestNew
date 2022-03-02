namespace BluePrism.BrowserAutomation.WebMessages
{
    public class GetElementByIdMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementById;
        }

        public GetElementByIdMessage()
        {
        }

        public GetElementByIdMessage(string id)
        {
            Data = id;
        }
    }

    public class GetElementByIdResponse : BaseElementResponse
    {
    }
}
