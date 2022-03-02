using System;

namespace BluePrism.DigitalWorker.Messages.Commands
{
    public interface StopProcess
    {
        Guid SessionId { get; }

        string Username { get; }
    }
}
