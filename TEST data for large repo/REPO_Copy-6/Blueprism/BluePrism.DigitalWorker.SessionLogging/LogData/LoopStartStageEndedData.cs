using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class LoopStartStageEndedData : ProcessStages.LogData.LoopStartStageEndedData
    {
        public LoopStartStageEndedData(string count)
        {
            Count = count;
        }

        public string Count { get; }
    }
}
