using System;

namespace BluePrism.DigitalWorker.Messages.Commands.Internal
{
    internal class GetSessionVariablesMessage : GetSessionVariables
    {
        public Guid SessionId { get; set; }
    }
}
