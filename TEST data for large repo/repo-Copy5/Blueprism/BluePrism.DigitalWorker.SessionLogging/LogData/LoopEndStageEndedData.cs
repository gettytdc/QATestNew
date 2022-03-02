using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class LoopEndStageEndedData : ProcessStages.LogData.LoopEndStageEndedData
    {
        public LoopEndStageEndedData(string iteration)
        {
            Iteration = iteration;
        }

        public string Iteration { get; }
    }
}
