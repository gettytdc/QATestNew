namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementScrollBoundsMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementScrollBounds;
        }

        public GetElementScrollBoundsMessage()
        {
        }

        public GetElementScrollBoundsMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementScrollBoundsResponse : BaseBoundsResponse
    {
    }
}
