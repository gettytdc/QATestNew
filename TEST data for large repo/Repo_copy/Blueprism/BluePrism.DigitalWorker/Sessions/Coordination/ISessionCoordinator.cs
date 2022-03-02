using System.Threading;
using System.Threading.Tasks;

namespace BluePrism.DigitalWorker.Sessions.Coordination
{
    public interface ISessionCoordinator
    {
        Task RunProcess(SessionContext context, CancellationToken waitCancellationToken);
    }
}