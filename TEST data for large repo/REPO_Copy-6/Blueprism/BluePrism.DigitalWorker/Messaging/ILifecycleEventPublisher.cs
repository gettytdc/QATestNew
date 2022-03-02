using System.Threading.Tasks;

namespace BluePrism.DigitalWorker.Messaging
{
    public interface ILifecycleEventPublisher
    {
        Task Start();
        Task Stop();
    }
}