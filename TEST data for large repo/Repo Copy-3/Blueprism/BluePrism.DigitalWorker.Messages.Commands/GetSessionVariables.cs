using System;

namespace BluePrism.DigitalWorker.Messages.Commands
{
    public interface GetSessionVariables
    {
        Guid SessionId { get; set; }
    }
}
