namespace BluePrism.DigitalWorker.Messages.Events.LogEntryData
{
    public class CalculationLogEntryData : ProcessLogEntryData
    {
        public string Result { get; set; }
        public int DataType { get; set; }
    }
}
