using System;
using System.Collections.Concurrent;

namespace BluePrism.DigitalWorker.Sessions
{
    /// <summary>
    /// This class will hold a collection of sessions/runner records so they can
    /// be tracked by a digital worker and retrieved when required
    /// </summary>
    public class RunningSessionRegistry : IRunningSessionRegistry
    {
        private readonly ConcurrentDictionary<Guid, IDigitalWorkerRunnerRecord> _sessions = new ConcurrentDictionary<Guid, IDigitalWorkerRunnerRecord>();

        public void Register(Guid sessionId, IDigitalWorkerRunnerRecord runnerRecord)
        {
            if (!_sessions.TryAdd(sessionId, runnerRecord))
                throw new InvalidOperationException("Session already registered");
        }

        public IDigitalWorkerRunnerRecord Get(Guid sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var runnerRecord))
                return null;

            return runnerRecord;
        }

        public void Unregister(Guid sessionId)
        {
            if(!_sessions.TryRemove(sessionId, out var _))
                throw new ArgumentException("Session not registered");
        }
    }
}
