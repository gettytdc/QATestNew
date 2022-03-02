namespace BluePrism.BrowserAutomation.WebMessages
{
    public class LaunchMessage : WebMessage<LaunchMessageBody>
    {
        public override MessageType MessageType
        {
            get => MessageType.Launch;
        }

        public LaunchMessage()
        {
        }

        public LaunchMessage(BrowserType browserType, string urls, string trackingId)
        {
            Data = new LaunchMessageBody
            {
                BrowserType = browserType,
                Urls = urls,
                TrackingId = trackingId
            };
        }
    }

    public class LaunchMessageBody
    {
        public BrowserType BrowserType { get; set; }
        public string Urls { get; set; }

        public string TrackingId { get; set; }
    }

    public class LaunchResponse : WebMessage<string>
    {
        public string Name => Data;
    }
}
