using BluePrism.AutomateAppCore;

namespace BluePrism.DigitalWorker.Sessions
{
    public interface IDigitalWorkerRunnerRecord : IRunnerRecord
    {
        bool StopRequested { get; set; }

        string StartedByUsername { get; }
    }
}