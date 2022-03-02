using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class DecisionStageEndedData : ProcessStages.LogData.DecisionStageEndedData
    {
        public DecisionStageEndedData(string result) 
        {
            Result = result;
        }

        public string Result { get; }
    }
}
