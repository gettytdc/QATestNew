using System;

namespace BluePrism.DigitalWorker.Messages.Events.Internal
{
    internal class DigitalWorkerStoppedMessage : DigitalWorkerStopped
    {
        public string Name { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}