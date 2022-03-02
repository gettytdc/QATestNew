using System;

namespace BluePrism.DigitalWorker.Messages.Events.Internal
{
    internal class DigitalWorkerStartedMessage : DigitalWorkerStarted
    {
        public string Name { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}