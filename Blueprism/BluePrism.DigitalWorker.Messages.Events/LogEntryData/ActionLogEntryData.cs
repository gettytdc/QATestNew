namespace BluePrism.DigitalWorker.Messages.Events.LogEntryData
{
    public class ActionLogEntryData : ProcessLogEntryData
    {
        public string ObjectName { get; set; }
        public string ActionName { get; set; }
    }
}