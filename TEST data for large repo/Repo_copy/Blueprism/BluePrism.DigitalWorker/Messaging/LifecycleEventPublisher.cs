using System;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Messages.Events.Factory;
using NLog;

namespace BluePrism.DigitalWorker.Messaging
{
    public class LifecycleEventPublisher : ILifecycleEventPublisher, IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly DigitalWorkerContext _context;
        private readonly IMessageBusWrapper _bus;
        private readonly ISystemTimer _heartbeatTimer;
        private readonly ISystemClock _clock;
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(60);

        public LifecycleEventPublisher(DigitalWorkerContext context, IMessageBusWrapper bus, ISystemTimer timer, ISystemClock clock)
        {
            _context = context;
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _heartbeatTimer = timer;
            _clock = clock;
        }

        private string Name => _context.StartUpOptions.Name.FullName;
        
        public async Task Start()
        {
            _heartbeatTimer.Start(PublishHeartbeat, HeartbeatInterval, HeartbeatInterval);
            await _bus.Publish(DigitalWorkerEvents.DigitalWorkerStarted(Name, _clock.Now)).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            _heartbeatTimer.Stop();
            await _bus.Publish(DigitalWorkerEvents.DigitalWorkerStopped(Name, _clock.Now)).ConfigureAwait(false);
        }

        private void PublishHeartbeat()
        {
            try
            {
                _bus.Publish(DigitalWorkerEvents.DigitalWorkerHeartbeat(Name, _clock.Now),
                    context => context.TimeToLive = HeartbeatInterval).Await();
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Error publishing heartbeat event");
            }
        }

        public void Dispose()
        {
            _heartbeatTimer.Dispose();
        }
    }
}
