using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.AutomateProcessCore;
using NLog;

namespace BluePrism.DigitalWorker.Sessions.Coordination
{
    /// <summary>
    /// Coordinates running of sessions based on their compatible run modes, holding sessions
    /// until they are able to be run. This works in conjunction with the separate endpoints
    /// that are used to limit the number of RunProcess command messages. Commands to start
    /// processes with a background run mode are received on a "concurrent" queue, which is configured
    /// to run concurrent consumers. Commands to start foreground / exclusive processes are receive
    /// on a "consecutive" queue, which is limited to a single concurrent consumer.
    /// </summary>
    /// <remarks>
    /// Changes to state and starting of sessions take place in response to events that are
    /// managed via an Observable. The subscription handler will be executed in serial order
    /// as events are pushed to the observable, allowing us to use predictable logic and avoid
    /// race conditions in this context of sessions running concurrently.
    /// </remarks>
    public sealed class SessionCoordinator : ISessionCoordinator, IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IExclusiveProcessLock _exclusiveProcessLock;
        private readonly ISessionRunner _sessionRunner;
        private readonly Subject<CoordinationEvent> _events = new Subject<CoordinationEvent>();
        private readonly List<SessionInfo> _runningSessions = new List<SessionInfo>();
        private readonly List<SessionInfo> _pendingSessions = new List<SessionInfo>();

        public SessionCoordinator(IExclusiveProcessLock exclusiveProcessLock, ISessionRunner sessionRunner)
        {
            _exclusiveProcessLock = exclusiveProcessLock;
            _sessionRunner = sessionRunner;
            _events.Synchronize().Subscribe(HandleEvent);
        }

        public async Task RunProcess(SessionContext context, CancellationToken waitCancellationToken)
        {
            Logger.Debug("Preparing process: {0}", context);
            var session = new SessionInfo(context, waitCancellationToken);
            AddEvent(session, CoordinationEventType.Requested);
            waitCancellationToken.Register(() => AddEvent(session, CoordinationEventType.WaitTimeout));
            await session.Task;
        }

        private void AddEvent(SessionInfo session, CoordinationEventType eventType)
        {
            _events.OnNext(new CoordinationEvent(session, eventType));
        }
        
        private void HandleEvent(CoordinationEvent @event)
        {
            Logger.Debug("Handling event {0}: {1}", @event.EventType, @event.Session.Context);
            switch (@event.EventType)
            {
                case CoordinationEventType.Requested:
                    OnRequested(@event.Session);
                    break;
                case CoordinationEventType.WaitTimeout:
                    OnWaitTimeout(@event.Session);
                    break;
                case CoordinationEventType.Pending:
                    OnPending(@event.Session);
                    break;
                case CoordinationEventType.Finished:
                    OnFinished(@event.Session);
                    break;
            }
        }

        private void OnRequested(SessionInfo session)
        {
            _pendingSessions.Add(session);
            if (session.Context.Process.EffectiveRunMode == BusinessObjectRunMode.Exclusive)
            {
                Logger.Debug("Starting exclusive process lock");
                _exclusiveProcessLock.Lock();
            }
            AddEvent(session, CoordinationEventType.Pending);
        }

        private void OnWaitTimeout(SessionInfo session)
        {
            bool wasPending = _pendingSessions.Remove(session);
            if (wasPending)
            {
                ReleaseExclusiveProcessLock();
                var exception = new OperationCanceledException(session.WaitCancellationToken);
                session.SetException(exception);
            }
        }

        private void OnPending(SessionInfo session)
        {
            StartProcess(session);
        }

        private void OnFinished(SessionInfo session)
        {
            _runningSessions.Remove(session);
            ReleaseExclusiveProcessLock();
            StartPendingProcesses();
            // Completes the task returned by RunProcess
            session.SetComplete();
        }

        private void StartPendingProcesses()
        {
            foreach (var pending in _pendingSessions.ToList())
            {
                if (!StartProcess(pending)) break;
            }
        }

        private bool StartProcess(SessionInfo session)
        {
            if (!CanStartProcess(session))
            {
                Logger.Debug("Not ready to start session: {0}", session.Context);
                return false;
            }
            Logger.Debug("Starting session: {0}", session.Context);
            _runningSessions.Add(session);
            _pendingSessions.Remove(session);
            Task.Run(async () =>
            {
                await _sessionRunner.RunAsync(session.Context);
                Logger.Debug("Session finished: {0}", session.Context);
                AddEvent(session, CoordinationEventType.Finished);
            });
            return true;
        }

        private bool CanStartProcess(SessionInfo session)
        {
            var runMode = session.Context.Process.EffectiveRunMode;
            switch (runMode)
            {
                case BusinessObjectRunMode.Exclusive:
                    return !_runningSessions.Any();
                case BusinessObjectRunMode.Foreground:
                    return _runningSessions.All(x =>
                        x.Context.Process.EffectiveRunMode == BusinessObjectRunMode.Background);
                case BusinessObjectRunMode.Background:
                    return _runningSessions.All(x => 
                        x.Context.Process.EffectiveRunMode != BusinessObjectRunMode.Exclusive);
                default:
                    Logger.Warn("Unexpected runmode: {0}", runMode);
                    break;
            }
            return false;
        }

        private void ReleaseExclusiveProcessLock()
        {
            bool canRelease = _runningSessions
                .Concat(_pendingSessions)
                .All(x => x.Context.Process.EffectiveRunMode != BusinessObjectRunMode.Exclusive);
            if (canRelease)
            {
                Logger.Debug("Releasing exclusive process lock");
                _exclusiveProcessLock.Unlock();
            }
        }

        public void Dispose()
        {
            _events?.Dispose();
        }

        private class CoordinationEvent
        {
            public CoordinationEvent(SessionInfo session, CoordinationEventType eventType)
            {
                Session = session;
                EventType = eventType;
            }

            public SessionInfo Session { get; }

            public CoordinationEventType EventType { get; }
        }

        private class SessionInfo
        {
            private readonly TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            public SessionInfo(SessionContext context, CancellationToken waitCancellationToken)
            {
                Context = context;
                WaitCancellationToken = waitCancellationToken;
            }

            public SessionContext Context { get; }

            public CancellationToken WaitCancellationToken { get; }

            public Task Task => _completionSource.Task;

            public void SetComplete() => _completionSource.SetResult(true);

            public void SetException(OperationCanceledException exception) => _completionSource.SetException(exception);
        }

        private enum CoordinationEventType
        {
            Requested,
            WaitTimeout,
            Pending,
            Finished
        }
    }
}