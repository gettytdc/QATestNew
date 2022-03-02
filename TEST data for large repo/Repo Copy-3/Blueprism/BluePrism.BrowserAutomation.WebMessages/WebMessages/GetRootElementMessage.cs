namespace BluePrism.BrowserAutomation.WebMessages
{
    public class GetRootElementMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetRootElement;
        }
    }

    public class GetRootElementResponse : BaseElementResponse
    {
    }
}
