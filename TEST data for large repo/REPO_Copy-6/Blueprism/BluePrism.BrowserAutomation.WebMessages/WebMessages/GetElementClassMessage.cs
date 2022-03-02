namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementClassMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementClass;
        }

        public GetElementClassMessage()
        {
        }

        public GetElementClassMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementClassResponse : WebMessage<string>
    {
        public string Class => Data;
    }
}
