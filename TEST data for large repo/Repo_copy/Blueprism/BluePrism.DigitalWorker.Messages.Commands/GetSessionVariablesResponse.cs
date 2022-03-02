using System;

namespace BluePrism.DigitalWorker.Messages.Commands
{
    public interface GetSessionVariablesResponse
    {
        bool SessionRunning { get; set; }

        SessionVariable[] Variables { get; set; }
    }
}
