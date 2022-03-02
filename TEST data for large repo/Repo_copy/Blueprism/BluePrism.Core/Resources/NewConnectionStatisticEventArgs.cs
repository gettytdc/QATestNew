using System;

namespace BluePrism.Core.Resources
{
    public class NewConnectionStatisticEventArgs
        : EventArgs
    {
        public Guid ResourceId { get; }
        public bool Success { get; }
        public long Ping { get; }
        public DateTime Time { get; }

        public NewConnectionStatisticEventArgs(Guid resourceId, bool success, long ping, DateTime time)
        {
            ResourceId = resourceId;
            Success = success;
            Ping = ping;
            Time = time;
        }
    }
}
