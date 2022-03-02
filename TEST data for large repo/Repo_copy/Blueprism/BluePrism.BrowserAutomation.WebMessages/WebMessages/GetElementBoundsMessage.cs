namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementBoundsMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementBounds;
        }

        public GetElementBoundsMessage()
        {
        }

        public GetElementBoundsMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementBoundsResponse : BaseBoundsResponse
    {
    }
}
