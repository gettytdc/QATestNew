using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class SkillStageStartedData : ProcessStages.LogData.SkillStageStartedData
    {
        public SkillStageStartedData(string objectName, string actionName)
        {
            ObjectName = objectName;
            ActionName = actionName;
        }

        public string ObjectName { get; }
        public string ActionName { get; }
    }
}
