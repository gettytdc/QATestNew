namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementScreenLocationAndBoundsMessage : WebMessage<Guid>
    {
        public override MessageType MessageType => MessageType.GetElementScreenLocationAndBounds;

        public GetElementScreenLocationAndBoundsMessage()
        {
        }

        public GetElementScreenLocationAndBoundsMessage(Guid elementId) => Data = elementId;
    }

    public class GetElementScreenLocationAndBoundsResponse : BaseBoundsResponse
    {
    }
}
