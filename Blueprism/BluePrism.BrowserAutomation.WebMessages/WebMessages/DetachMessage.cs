namespace BluePrism.BrowserAutomation.WebMessages
{
    public class DetachMessage : WebMessage<DetachMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.Detach;
        }

        public DetachMessage()
        {
        }

        public DetachMessage(string trackingId) =>
            Data = new DetachMessageBody
            {
                TrackingId = trackingId
            };
    }

    public class DetachMessageBody
    {
        public string TrackingId { get; set; }
    }

    public class DetachResponse : WebMessage<string>
    {
        public string Name => Data;
    }
}
