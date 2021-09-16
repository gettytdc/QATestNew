namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetDescendantsMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetDescendants;
        }

        public GetDescendantsMessage()
        {
        }

        public GetDescendantsMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetDescendantsResponse : BaseElementsResponse
    {
    }
}
