namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementAccessKeyMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementAccessKey;
        }

        public GetElementAccessKeyMessage()
        {
        }

        public GetElementAccessKeyMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementAccessKeyResponse : WebMessage<string>
    {
        public string AccessKey => Data;
    }
}
