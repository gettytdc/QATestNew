namespace BluePrism.BrowserAutomation.WebMessages
{
    public class SetAddressMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.SetAddress;
        }

        public SetAddressMessage()
        {
        }

        public SetAddressMessage(string address)
        {
            Data = address;
        }
    }
}
