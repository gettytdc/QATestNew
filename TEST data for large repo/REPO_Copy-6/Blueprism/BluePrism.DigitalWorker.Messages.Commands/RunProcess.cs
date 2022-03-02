using System;

namespace BluePrism.DigitalWorker.Messages.Commands
{
    public interface RunProcess
    {
        Guid SessionId { get; }
        Guid ProcessId { get; }
        string Username { get; }
    }
}
