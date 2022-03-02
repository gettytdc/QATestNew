using MassTransit;
using System;
using System.Threading.Tasks;
using BluePrism.Cirrus.Common.MassTransit;
using GreenPipes;
using System.Collections.Generic;

namespace BluePrism.DigitalWorker.Messaging
{
    public class MessageBusWrapper : IMessageBusWrapper
    {
        private readonly IBusControl _bus;

        private static TimeSpan DefaultHostTimeout => TimeSpan.FromSeconds(10);

        public MessageBusWrapper(IBusControl bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task Publish<T>(T message) where T : class
        {
            await _bus.PublishWithTimeout(message, DefaultHostTimeout);
        }

        public async Task Publish<T>(T message, Action<PublishContext<T>> context) where T : class
        {
            await _bus.PublishWithTimeout(message, context, DefaultHostTimeout);
        }

        public async Task Send<T>(T message) where T : class
        {
            await _bus.SendWithTimeout(message, DefaultHostTimeout);
        }
        
        public void Start(TimeSpan timeout) 
            => _bus.Start(timeout);

        public void Stop()
            => _bus.Stop();    

        public (string Host, string Username) Configuration
        {
            get
            {
                var configuration = _bus.GetProbeResult();
                var busConfiguration = (IDictionary<string, object>)configuration.Results["bus"];
                var hostConfiguration = (IDictionary<string, object>)busConfiguration["host"];
                return ($"{hostConfiguration["Host"]}:{hostConfiguration["Port"]}", hostConfiguration["Username"].ToString());
            }
        }

    }
}
