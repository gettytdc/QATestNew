using System;

namespace BluePrism.DigitalWorker.Messages.Events
{
    public interface DigitalWorkerHeartbeat
    {
        string Name { get; }
        DateTimeOffset Date { get; }
    }
}
