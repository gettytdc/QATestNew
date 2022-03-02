using System;

namespace BluePrism.DigitalWorker.Messages.Events
{
    public interface DigitalWorkerStarted
    {
        string Name { get; }
        DateTimeOffset Date { get; }
    }
}
