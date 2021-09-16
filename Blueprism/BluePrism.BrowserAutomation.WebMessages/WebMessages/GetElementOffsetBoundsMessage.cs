namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementOffsetBoundsMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementOffsetBounds;
        }

        public GetElementOffsetBoundsMessage()
        {
        }

        public GetElementOffsetBoundsMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementOffsetBoundsResponse : BaseBoundsResponse
    {
    }
}
