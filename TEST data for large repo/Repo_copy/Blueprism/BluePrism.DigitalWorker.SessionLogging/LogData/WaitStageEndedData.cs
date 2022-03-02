using System;
using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class WaitStageEndedData : ProcessStages.LogData.WaitStageEndedData
    {
        public WaitStageEndedData(string choiceName, int choiceNumber)
        {
            ChoiceName = choiceName;
            ChoiceNumber = choiceNumber;
        }

        public string ChoiceName { get; }
        public int ChoiceNumber { get; }
    }
}
