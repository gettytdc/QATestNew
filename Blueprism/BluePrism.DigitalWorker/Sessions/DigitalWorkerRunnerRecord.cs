using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;
using System;

namespace BluePrism.DigitalWorker.Sessions
{
    public class DigitalWorkerRunnerRecord : RunnerRecord, IDigitalWorkerRunnerRecord
    {
        public DigitalWorkerRunnerRecord(
            SessionContext context,
            RunningSessionMonitor monitor,
            ISessionStatusPublisher publisher,
            ISessionCleanup sessionCleanup,
            Func<Guid, IProcessLogContext, clsLoggingEngine> loggingEngineFactory) :
                base(context.Process.ProcessXml,
                    context.Process.ProcessId,
                    monitor,
                    new SessionInformation(context.SessionId.ToString()),
                    publisher)
        {
            Context = context;
            mRunMode = context.Process.EffectiveRunMode;
            _sessionCleanup = sessionCleanup ?? throw new ArgumentNullException(nameof(sessionCleanup));
            _loggingEngineFactory = loggingEngineFactory ?? throw new ArgumentNullException(nameof(loggingEngineFactory));
        }

        public readonly SessionContext Context;
        private readonly ISessionCleanup _sessionCleanup;

        private readonly Func<Guid, IProcessLogContext, clsLoggingEngine> _loggingEngineFactory;

        public override clsLoggingEngine Log => mLog ?? (mLog = CreateLoggingEngine());

        private CompoundLoggingEngine CreateLoggingEngine()
        {
            var loggingContext = new SimpleLogContext(Process);
            var engine = new CompoundLoggingEngine();
            engine.Add(_loggingEngineFactory(Context.SessionId, loggingContext));
            return engine;
        }

        protected override SessionIdentifier SessionIdentifier
            => new DigitalWorkerSessionIdentifier(Context.SessionId);

        public bool StopRequested { get; set; }

        public string StartedByUsername => Context.StartedByUsername;

        protected override void OnSessionEnded()
        {
            _sessionCleanup.OnSessionEnded(SessionIdentifier);
        }
    }
}
