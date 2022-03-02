namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementAttributeMessage : WebMessage<GetElementAttributeMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementAttribute;
        }


        public GetElementAttributeMessage()
        {
        }

        public GetElementAttributeMessage(Guid elementId, string attributeName)
        {
            Data = new GetElementAttributeMessageBody
            {
                ElementId = elementId,
                AttributeName = attributeName
            };
        }
    }

    public class GetElementAttributeMessageBody
    {
        public Guid ElementId { get; set; }
        public string AttributeName { get; set; }
    }

    public class GetElementAttributeResponse : WebMessage<string>
    {
        public string Value => Data;
    }
}
