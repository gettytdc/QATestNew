using System;
using System.Threading.Tasks;
using MassTransit;
using NLog;

namespace BluePrism.AuthenticationServerSynchronization
{
    public class MessageBus
    {
        private readonly IBusControl _bus;
        private static readonly TimeSpan MessageBusStartTimeout = TimeSpan.FromDays(1);
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public MessageBus(IBusControl bus)
        {
            _bus = bus;
        }

        public async Task Start()
        {
            if (Status == MessageBusStatus.ReadyToStart)
            {
                try
                {
                    Log.Info("Starting message bus");
                    Status = MessageBusStatus.Starting;
                    await _bus.StartAsync(MessageBusStartTimeout);
                    Status = MessageBusStatus.Started;
                    Log.Info("Message bus started");
                }
                catch (Exception ex)
                {
                    Status = MessageBusStatus.StartFailed;
                    Log.Error(ex, "Failed to start message bus");
                }
            }
        }

        public void Stop()
        {
            if (Status == MessageBusStatus.Starting || Status == MessageBusStatus.Started)
            {
                _bus.Stop();
            }
            Status = MessageBusStatus.Stopped;
        }

        public MessageBusStatus Status { get; private set; } = MessageBusStatus.ReadyToStart;

        public enum MessageBusStatus { ReadyToStart, Starting, Started, StartFailed, Stopped }
    }
}
