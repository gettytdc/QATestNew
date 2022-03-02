namespace BluePrism.BrowserAutomation.WebMessages
{
    using System;

    public class GetElementClientBoundsMessage : WebMessage<Guid>
    {
        public override MessageType MessageType
        {
            get => MessageType.GetElementClientBounds;
        }

        public GetElementClientBoundsMessage()
        {
        }

        public GetElementClientBoundsMessage(Guid elementId)
        {
            Data = elementId;
        }
    }

    public class GetElementClientBoundsResponse : BaseBoundsResponse
    {
    }
}
