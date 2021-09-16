using BluePrism.AutomateAppCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BluePrism.DigitalWorker.Sessions
{
    public class SessionRunner : ISessionRunner
    {
        private readonly Func<SessionContext, IDigitalWorkerRunnerRecord> _runnerRecordFactory;
        private readonly IRunningSessionRegistry  _runningSessionRegistry;

        public SessionRunner(Func<SessionContext, IDigitalWorkerRunnerRecord> runnerRecordFactory, IRunningSessionRegistry runningSessionRegistry)
        {
            _runnerRecordFactory = runnerRecordFactory ?? throw new ArgumentNullException(nameof(runnerRecordFactory));
            _runningSessionRegistry = runningSessionRegistry ?? throw new ArgumentNullException(nameof(runningSessionRegistry));
        }

        public async Task RunAsync(SessionContext context)
        {
            var runner = _runnerRecordFactory(context);

            var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Run the process explicitly on a new thread to ensure that it starts on it's own separate thread
            // as opposed to leveraging the thread pool. This is also required to set the apartment state of
            // the thread as the process running may call COM objects which don't support threading and must
            // be called from the same thread (as opposed to using Tasks)
            var thread = new Thread(() =>
            {
                try
                {
                    _runningSessionRegistry.Register(context.SessionId, runner);

                    runner.RunnerMethod();
                }
                finally
                {
                    _runningSessionRegistry.Unregister(context.SessionId);

                    completionSource.SetResult(true);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            await completionSource.Task;
        }
    }

}
