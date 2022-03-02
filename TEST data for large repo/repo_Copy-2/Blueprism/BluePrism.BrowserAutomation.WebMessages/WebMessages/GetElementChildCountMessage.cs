namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementChildCountMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementChildCount;
        }

        public GetElementChildCountMessage()
        {
        }

        public GetElementChildCountMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementChildCountResponse : WebMessage<int>
    {
        public int Count => Data;
    }
}
