using BluePrism.DigitalWorker.Messages.Events.ProcessStages;
using BluePrism.DigitalWorker.Messages.Events.ProcessStages.LogData;
using Newtonsoft.Json;
using System;

namespace BluePrism.DigitalWorker.SessionLogging
{
    public class SessionLog
    {
        public Guid SessionId { get; }
        public int EntryNumber { get; }
        public DateTimeOffset At { get; }
        public SessionLogData Data { get; }
        public Guid StageId { get; }
        public string StageName { get; }

        public SessionLog(Guid sessionId, int entryNumber, DateTimeOffset at, Guid stageId, string stageName, Messages.Events.ProcessStages.LogData.SessionLogData data)
        {
            SessionId = sessionId;
            EntryNumber = entryNumber;
            At = at;
            StageId = stageId;
            StageName = stageName;
            Data = data;
        }

        public ProcessStageActioned CreateMessage()
            => new ProcessStageActionedMessage( SessionId, EntryNumber, At, StageId, StageName, JsonConvert.SerializeObject(Data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));

        public static SessionLog Deserialize(Guid sessionId, int entryNumber, DateTimeOffset at, Guid stageId, string stageName, string data)
            => new SessionLog(sessionId,
                  entryNumber,
                  at,
                  stageId,
                  stageName,
                  JsonConvert.DeserializeObject<SessionLogData>(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));

        private class ProcessStageActionedMessage : ProcessStageActioned
        {
            public ProcessStageActionedMessage(Guid sessionId, int entryNumber, DateTimeOffset at, Guid stageId, string stageName, string data)
            {
                SessionId = sessionId;
                EntryNumber = entryNumber;
                At = at;
                StageId = stageId;
                StageName = stageName;
                Data = data;
            }

            public Guid SessionId { get; }
            public int EntryNumber { get; }
            public DateTimeOffset At { get; }
            public string Data { get; }
            public Guid StageId { get; }
            public string StageName { get; }
        }
    }
}
