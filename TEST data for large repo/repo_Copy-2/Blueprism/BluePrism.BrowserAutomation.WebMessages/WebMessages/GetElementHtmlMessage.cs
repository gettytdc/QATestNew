namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementHtmlMessage : WebMessage<Guid>
    {
        public override MessageType MessageType => MessageType.GetElementHtml;

        public GetElementHtmlMessage()
        {
        }

        public GetElementHtmlMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementHtmlResponse : WebMessage<string>
    {
        public string Html => Data;
    }
}
