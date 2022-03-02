using System;

namespace BluePrism.DigitalWorker.Sessions
{
    public interface IRunningSessionRegistry 
    {
        void Register(Guid sessionId, IDigitalWorkerRunnerRecord runnerRecord);

        IDigitalWorkerRunnerRecord Get(Guid sessionId);

        void Unregister(Guid sessionId);
    }
}
