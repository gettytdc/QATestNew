using System;

namespace BluePrism.DigitalWorker.Messages.Commands.Internal
{
    internal class StopProcessMessage : StopProcess
    {
        public Guid SessionId { get; set; }
        public string Username { get; set; }
    }
}