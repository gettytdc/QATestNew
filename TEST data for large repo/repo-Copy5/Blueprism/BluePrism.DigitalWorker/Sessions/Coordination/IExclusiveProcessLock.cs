using System.Threading;
using System.Threading.Tasks;

namespace BluePrism.DigitalWorker.Sessions.Coordination
{
    public interface IExclusiveProcessLock 
    {
        ExclusiveProcessLockState State { get; }

        void Lock();

        void Unlock();
        
        
        Task Wait(CancellationToken cancellationToken);
    }
}
