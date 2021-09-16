using ProcessStages = BluePrism.DigitalWorker.Messages.Events.ProcessStages;

namespace BluePrism.DigitalWorker.SessionLogging.LogData
{
    public class CalculationStageEndedData : ProcessStages.LogData.CalculationStageEndedData
    {
        public CalculationStageEndedData(string result, int dataType)
        {
            Result = result;
            DataType = dataType;
        }

        public string Result { get; }
        public int DataType { get; }
    }
}
