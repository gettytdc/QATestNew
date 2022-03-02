using System;

namespace BluePrism.DigitalWorker.Messages.Events
{
    public interface DigitalWorkerStopped
    {
        string Name { get; }
        DateTimeOffset Date { get; }
    }
}
