namespace BluePrism.BrowserAutomation.WebMessages
{
    public class AttachMessage : WebMessage<AttachMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.Attach;
        }

        public AttachMessage()
        {
        }

        public AttachMessage(BrowserType browserType, string tabName, string trackingId) =>
            Data = new AttachMessageBody
            {
                BrowserType = browserType,
                TabName = tabName,
                TrackingId = trackingId
            };
    }

    public class AttachMessageBody
    {
        public BrowserType BrowserType { get; set; }
        public string TabName { get; set; }
        public string TrackingId { get; set; }
        public int Attempt { get; set; } = 1;
    }

    public class AttachResponse : WebMessage<string>
    {
        public string Name => Data;
    }
}
