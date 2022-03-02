using System;
using BluePrism.DigitalWorker.Messages.Events.LogEntryData;

namespace BluePrism.DigitalWorker.Messages.Events.Internal
{
    internal class ProcessStageLoggedMessage : ProcessStageLogged
    {
        public Guid SessionId { get; set; }
        public int EntryNumber { get; set; }
        public DateTimeOffset Date { get; set; }
        public ProcessLogEntryData Data { get; set; }
        public Guid? StageId { get; set; }
        public string StageName { get; set; }
        public ProcessStageLogEntryType EntryType { get; set; }
    }
}