using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class AlertStageEndedData : ProcessStages.LogData.AlertStageEndedData
    {
        public AlertStageEndedData(string alert)
        {
            Alert = alert;
        }

        public string Alert { get; }
    }
}
