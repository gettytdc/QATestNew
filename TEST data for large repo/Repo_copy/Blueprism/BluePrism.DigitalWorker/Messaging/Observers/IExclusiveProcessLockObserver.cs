using MassTransit;

namespace BluePrism.DigitalWorker.Messaging.Observers
{
    public interface IExclusiveProcessLockObserver : IReceiveObserver
    {
    }
}