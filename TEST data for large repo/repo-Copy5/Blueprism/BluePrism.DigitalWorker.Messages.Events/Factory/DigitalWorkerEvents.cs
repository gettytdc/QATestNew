using System;
using BluePrism.DigitalWorker.Messages.Events.Internal;
using BluePrism.DigitalWorker.Messages.Events.LogEntryData;

namespace BluePrism.DigitalWorker.Messages.Events.Factory
{
    /// <summary>
    /// Contains factory methods to create messages within .NET applications
    /// </summary>
    public static class DigitalWorkerEvents
    {
        public static DigitalWorkerStarted DigitalWorkerStarted(string name, DateTimeOffset date)
        {
            return new DigitalWorkerStartedMessage
            {
                Name = name,
                Date = date
            };
        }

        public static DigitalWorkerStopped DigitalWorkerStopped(string name, DateTimeOffset date)
        {
            return new DigitalWorkerStoppedMessage
            {
                Name = name,
                Date = date
            };
        }

        public static DigitalWorkerHeartbeat DigitalWorkerHeartbeat(string name, DateTimeOffset date)
        {
            return new DigitalWorkerHeartbeatMessage
            {
                Name = name,
                Date = date
            };
        }

        public static ProcessStarted ProcessStarted(Guid sessionId, DateTimeOffset date)
        {
            return new ProcessStartedMessage
            {
                SessionId = sessionId,
                Date = date
            };
        }

        public static ProcessStageLogged ProcessStageLogged(Guid sessionId, int entryNumber, DateTimeOffset date, Guid? stageId, string stageName, ProcessStageLogEntryType entryType, ProcessLogEntryData data)
        {
            return new ProcessStageLoggedMessage
            {

                SessionId = sessionId,
                EntryNumber = entryNumber,
                Date = date,
                StageId = stageId,
                StageName = stageName,
                Data = data,
                EntryType = entryType
            };
        }

        public static ProcessTerminated ProcessTerminated(Guid sessionId, DateTimeOffset date, TerminationReason terminationReason, string exceptionType, string exceptionMessage)
        {
            return new ProcessTerminatedMessage
            {
                SessionId = sessionId,
                Date = date,
                TerminationReason = terminationReason,
                ExceptionType = exceptionType,
                ExceptionMessage = exceptionMessage
            };
        }

        public static ProcessStopped ProcessStopped(Guid sessionId, DateTimeOffset date)
        {
            return new ProcessStoppedMessage
            {
                SessionId = sessionId,
                Date = date
            };
        }

        public static ProcessCompleted ProcessCompleted(Guid sessionId, DateTimeOffset date)
        {
            return new ProcessCompletedMessage
            {
                SessionId = sessionId,
                Date = date
            };
        }

        public static ProcessNotStarted ProcessNotStarted(Guid sessionId, DateTimeOffset date)
        {
            return new ProcessNotStartedMessage
            {
                SessionId = sessionId,
                Date = date
            };
        }

        public static ProcessPreStartFailed ProcessPreStartFailed(Guid sessionId, DateTimeOffset date)
        {
            return new ProcessPreStartFailedMessage
            {
                SessionId = sessionId,
                Date = date
            };
        }
    }
}
