using System.Threading;
using System.Threading.Tasks;
using BluePrism.Core.Extensions;
using GreenPipes.Internals.Extensions;
using NLog;

namespace BluePrism.DigitalWorker.Sessions.Coordination
{
    public class ExclusiveProcessLock : IExclusiveProcessLock
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private TaskCompletionSource<bool> _completionSource;

        public ExclusiveProcessLock()
        {
            _completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Unlock();
        }

        public ExclusiveProcessLockState State 
            => _completionSource.Task.IsCompleted 
                ? ExclusiveProcessLockState.Unlocked 
                : ExclusiveProcessLockState.Locked;

        public void Lock()
        {
            if (State != ExclusiveProcessLockState.Locked)
            {
                Logger.Debug("Transitioning to locked");
                _completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        public void Unlock()
        {
            if (State != ExclusiveProcessLockState.Unlocked)
            {
                Logger.Debug("Transitioning to unlocked");
                _completionSource.SetResult(true);
            }
        }

        public async Task Wait(CancellationToken cancellationToken)
        {
            if (State == ExclusiveProcessLockState.Locked)
                await _completionSource.Task.OrCanceled(cancellationToken);
        }
    }
}
