using System;
using System.Threading.Tasks;
using MassTransit;

namespace BluePrism.DigitalWorker.Messaging
{
    public interface IMessageBusWrapper
    {
        Task Send<T>(T message) where T : class;
        Task Publish<T>(T message) where T : class;
        Task Publish<T>(T message, Action<PublishContext<T>> context) where T : class;
        void Start(TimeSpan timeout);
        void Stop();
        (string Host, string Username) Configuration { get; }
    }
}
