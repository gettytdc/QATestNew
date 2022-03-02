using System;
using BluePrism.DigitalWorker.Messages.Events.LogEntryData;
using Newtonsoft.Json;

namespace BluePrism.DigitalWorker.Messages.Events
{
    public interface ProcessStageLogged
    {
        Guid SessionId { get; }
        int EntryNumber { get; } 
        DateTimeOffset Date { get; }
        Guid? StageId { get; }
        string StageName { get; }
        ProcessStageLogEntryType EntryType { get; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        ProcessLogEntryData Data { get; }
    }
}
