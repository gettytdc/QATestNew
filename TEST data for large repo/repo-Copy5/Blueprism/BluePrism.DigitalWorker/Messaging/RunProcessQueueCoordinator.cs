using System;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.DigitalWorker.MessagingClient;
using BluePrism.DigitalWorker.Messaging.Observers;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace BluePrism.DigitalWorker.Messaging
{
    public class RunProcessQueueCoordinator : IRunProcessQueueCoordinator
    {
        private readonly DigitalWorkerContext _context;
        private readonly IBusControl _bus;
        private readonly Func<RunProcessConsumer> _consumerFactory;
        private readonly Func<StopProcessConsumer> _stopProcessConsumer;
        private readonly Func<RequestStopProcessConsumer> _requestStopProcessConsumer;
        private readonly Func<GetSessionVariablesConsumer> _getSessionVariablesConsumer;
        private readonly IExclusiveProcessLockObserver _lockObserver;
        private const int ForegroundConcurrentProcessLimit = 1;
        private string DigitalWorkerName => _context.StartUpOptions.Name.FullName;

        public RunProcessQueueCoordinator(DigitalWorkerContext context, IBusControl bus, Func<RunProcessConsumer> consumerFactory,
            IExclusiveProcessLockObserver lockObserver, Func<StopProcessConsumer> stopProcessConsumer, 
            Func<RequestStopProcessConsumer> requestStopProcessConsumer,
            Func<GetSessionVariablesConsumer> getSessionVariableConsumer
        )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _consumerFactory = consumerFactory ?? throw new ArgumentNullException(nameof(consumerFactory));
            _lockObserver = lockObserver;
            _stopProcessConsumer = stopProcessConsumer ?? throw new ArgumentNullException(nameof(stopProcessConsumer));
            _requestStopProcessConsumer = requestStopProcessConsumer ?? throw new ArgumentNullException(nameof(requestStopProcessConsumer));
            _getSessionVariablesConsumer = getSessionVariableConsumer ?? throw new ArgumentNullException(nameof(getSessionVariableConsumer));
        }

        public void Start(TimeSpan messageBusStartTimeout) => _bus.Start(messageBusStartTimeout);
        
        public async Task BeginReceivingMessages()
        {
            await ConnectConsecutiveRunProcessConsumer();
            await ConnectConcurrentRunProcessConsumer();
            await ConnectControlConsumers();
        }

        private async Task ConnectConsecutiveRunProcessConsumer()
        {
            var handle = _bus.ConnectReceiveEndpoint(DigitalWorkerQueues.Consecutive, options =>
            {
                if (options is IRabbitMqReceiveEndpointConfigurator rabbitConfigurator)
                {
                    // Receive a single message at a time. High throughput systems that consume
                    // hundreds of messages per second can buffer unacknowledged messages for
                    // efficiency. This is not appropriate for low throughput processing of
                    // long-running foreground / exclusive processes running one at a time.
                    // The loss of efficiency in terms of network communication with message
                    // queue will be trivial in the context of a process running for several seconds
                    rabbitConfigurator.PrefetchCount = 1;
                    // TODO - handle transport-specific configuration more elegantly
                    // TODO - look at implications with other transports

                }
                options.Consumer(_consumerFactory, c =>
                {
                    c.UseConcurrentMessageLimit(ForegroundConcurrentProcessLimit);
                });
            });

            await handle.Ready.ConfigureAwait(false);
            handle.ReceiveEndpoint.ConnectReceiveObserver(_lockObserver);
        }

        private async Task ConnectConcurrentRunProcessConsumer()
        {
            var handle = _bus.ConnectReceiveEndpoint(DigitalWorkerQueues.Concurrent,
                                    options => options.Consumer(_consumerFactory));

            await handle.Ready.ConfigureAwait(false);
            handle.ReceiveEndpoint.ConnectReceiveObserver(_lockObserver);
        }

        /// <summary>
        /// Connects to a queue intended for commands that should be executed immediately
        /// </summary>
        private async Task ConnectControlConsumers()
        {
            var handle = _bus.ConnectReceiveEndpoint(DigitalWorkerQueues.Control(DigitalWorkerName),
                options => {
                    options.Consumer(_stopProcessConsumer);
                    options.Consumer(_requestStopProcessConsumer);
                    options.Consumer(_getSessionVariablesConsumer);
                });

            await handle.Ready.ConfigureAwait(false);
        }
    }
}
