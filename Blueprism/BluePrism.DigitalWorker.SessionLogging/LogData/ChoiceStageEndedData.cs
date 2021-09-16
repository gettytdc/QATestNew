using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class ChoiceStageEndedData : ProcessStages.LogData.ChoiceStageEndedData
    {
        public ChoiceStageEndedData(string choiceTaken, int choiceNumber) 
        {
            ChoiceTaken = choiceTaken;
            ChoiceNumber = choiceNumber;
        }

        public string ChoiceTaken { get; }
        public int ChoiceNumber { get; }
    }
}
