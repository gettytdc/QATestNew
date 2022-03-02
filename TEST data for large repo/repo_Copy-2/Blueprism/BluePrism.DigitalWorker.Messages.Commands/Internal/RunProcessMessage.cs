using System;

namespace BluePrism.DigitalWorker.Messages.Commands.Internal
{
    internal class RunProcessMessage : RunProcess
    {
        public Guid SessionId { get; set; }
        public Guid ProcessId { get; set; }
        public string Username { get; set; }
    }
}