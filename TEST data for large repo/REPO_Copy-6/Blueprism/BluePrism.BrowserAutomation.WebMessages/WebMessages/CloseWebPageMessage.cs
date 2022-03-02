namespace BluePrism.BrowserAutomation.WebMessages
{
    public class CloseWebPageMessage : WebMessage<CloseMessageBody>
    {
        public override MessageType MessageType => MessageType.ClosePage;

        public CloseWebPageMessage(string trackingId) =>
            Data = new CloseMessageBody
            {
                TrackingId = trackingId
            };

        public CloseWebPageMessage() =>
            Data = new CloseMessageBody
            {
                TrackingId = string.Empty
            };
    }

    public class CloseMessageBody
    {
        public string TrackingId { get; set; }
    }

}
