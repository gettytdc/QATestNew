namespace BluePrism.BrowserAutomation.WebMessages
{
    public class GetAddressMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetAddress;
        }
    }

    public class GetAddressResponse : WebMessage<string>
    {
        public string Address => Data;
    }
}
