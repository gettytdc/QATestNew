using System;

namespace BluePrism.DigitalWorker.Messages.Commands.Internal
{
    internal class RequestStopProcessMessage : RequestStopProcess
    {
        public Guid SessionId { get; set; }
    }
}