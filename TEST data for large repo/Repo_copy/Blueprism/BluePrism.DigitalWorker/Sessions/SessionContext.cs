using System;

namespace BluePrism.DigitalWorker.Sessions
{
    public class SessionContext
    {
        public SessionContext(Guid sessionId, ProcessInfo process, string startedByUsername)
        {
            SessionId = sessionId;
            Process = process;
            StartedByUsername = startedByUsername;
        }

        public SessionContext(Guid sessionId)
        {
            SessionId = sessionId;
        }

        public Guid SessionId { get; }

        public ProcessInfo Process { get; }

        public string StartedByUsername { get; }

        public override string ToString()
        {
            return $"{nameof(SessionId)}: {SessionId}, {nameof(Process)}: {Process}";
        }
    }
}