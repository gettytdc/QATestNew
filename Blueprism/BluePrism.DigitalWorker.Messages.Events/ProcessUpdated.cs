using System;

namespace BluePrism.DigitalWorker.Messages.Events
{
    public interface ProcessUpdated
    {
        Guid SessionId { get; }

        DateTimeOffset Date { get; }
    }
}