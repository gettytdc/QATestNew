using System;

namespace BluePrism.DigitalWorker.Messages.Commands
{
    public interface RequestStopProcess
    {
        Guid SessionId { get; }
    }
}
