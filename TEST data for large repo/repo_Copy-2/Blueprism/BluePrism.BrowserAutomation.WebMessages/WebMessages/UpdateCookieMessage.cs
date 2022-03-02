namespace BluePrism.BrowserAutomation.WebMessages
{
    public class UpdateCookieMessage : WebMessage<string>
    {
        public override MessageType MessageType
        {
            get => MessageType.UpdateCookie;
        }

        public UpdateCookieMessage()
        {
        }

        public UpdateCookieMessage(string cookie)
        {
            Data = cookie;
        }
    }
}
