using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class DebugDataEmittedData : ProcessStages.LogData.DebugDataEmittedData
    {
        public DebugDataEmittedData(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
