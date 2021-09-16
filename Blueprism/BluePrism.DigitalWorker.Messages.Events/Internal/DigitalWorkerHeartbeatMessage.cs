using System;

namespace BluePrism.DigitalWorker.Messages.Events.Internal
{
    internal class DigitalWorkerHeartbeatMessage : DigitalWorkerHeartbeat
    {
        public string Name { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}