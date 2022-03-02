using System.Threading.Tasks;

namespace BluePrism.DigitalWorker.Messaging
{
    public interface IRunProcessQueueCoordinator
    {
        Task BeginReceivingMessages();
    }
}
