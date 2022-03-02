using System;

namespace BluePrism.BrowserAutomation.Events
{
    public delegate void TrackingIdDetachedDelegate(object sender, TrackingIdDetachedEventArgs args);
    public class TrackingIdDetachedEventArgs :EventArgs
    {
        public string TrackingId { get; }
        public TrackingIdDetachedEventArgs(string trackingId) => TrackingId = trackingId;
    }
}
