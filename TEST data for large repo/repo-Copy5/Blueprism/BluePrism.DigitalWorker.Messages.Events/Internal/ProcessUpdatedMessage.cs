using System;

namespace BluePrism.DigitalWorker.Messages.Events.Internal
{
    internal abstract class ProcessUpdatedMessage : ProcessUpdated
    {
        public Guid SessionId { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}