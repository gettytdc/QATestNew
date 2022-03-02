using BluePrism.AutomateAppCore.Resources;
using System;
using System.Threading.Tasks;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Messaging;
using BluePrism.DigitalWorker.Notifications;
using MassTransit;

namespace BluePrism.DigitalWorker
{
    public class DigitalWorker : ResourceRunnerBase
    {
        private static readonly TimeSpan MessageBusStartTimeout = TimeSpan.FromDays(1);

        private IMessageBusWrapper _bus;
        private readonly DigitalWorkerContext _context;
        private readonly ISessionServiceClient _sessionServiceClient;
        private readonly IRunProcessQueueCoordinator _queueCoordinator;
        private readonly INotificationHandler _notificationHandler;
        private MessageBusStatus _busStatus = MessageBusStatus.ReadyToStart;
        private RegisteredStatus _registrationStatus = RegisteredStatus.None;
        private readonly ILifecycleEventPublisher _lifecycleEventPublisher;
        private readonly ITaskDelay _taskDelay;
        

        public DigitalWorker(
            IMessageBusWrapper bus,
            DigitalWorkerContext context, 
            IResourcePCView view,
            ISessionServiceClient sessionServiceClient,
            IRunProcessQueueCoordinator queueCoordinator,
            INotificationHandler notificationHandler,
            ILifecycleEventPublisher lifecycleEventPublisher,
            ITaskDelay taskDelay) : base(view)
        {
            _bus = bus;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _sessionServiceClient = sessionServiceClient;
            _queueCoordinator = queueCoordinator ?? throw new ArgumentNullException(nameof(queueCoordinator));
            _notificationHandler = notificationHandler ?? throw new ArgumentNullException(nameof(notificationHandler));
            _lifecycleEventPublisher = lifecycleEventPublisher ?? throw new ArgumentNullException(nameof(lifecycleEventPublisher));
            _taskDelay = taskDelay;
            _notificationHandler.Notify += (s, e) => DisplayNotification(e.Notification);
        }

        public override bool SessionsRunning() => false;
        
        protected override bool Start()
        {
            DisplayNotification(ResourceNotificationLevel.Comment, $"Starting digital worker with name '{_context.StartUpOptions.Name.FullName}'...");
            
            StartBus();
            if (_busStatus == MessageBusStatus.StartFailed)
            {
                DisplayNotification(ResourceNotificationLevel.Warning, $"Message queue not available.");
                return false;
            }

            if (!Register())
            {
                return false;
            }                

            if (!BeginReceivingProcesses())
            {
                return false;
            }                

            return true;
        }

        private bool Register()
        {           
            while(_registrationStatus == RegisteredStatus.None)
            {
                try
                {
                    DisplayNotification(ResourceNotificationLevel.Comment, "Registering...");

                    _registrationStatus = _sessionServiceClient.RegisterDigitalWorker(_context.StartUpOptions.Name.FullName).Await();
                    
                    if (_registrationStatus == RegisteredStatus.AlreadyOnline)
                    {
                        DisplayNotification(ResourceNotificationLevel.Error,
                            $"Error starting digital worker: {_context.StartUpOptions.Name.FullName} is already online.");
                        return false;
                    }

                    DisplayNotification(ResourceNotificationLevel.Comment,
                        $"{_context.StartUpOptions.Name.FullName} registered");
                    _lifecycleEventPublisher.Start().Await();

                    return true;
                }
                catch (RequestTimeoutException)
                {
                    DisplayNotification(ResourceNotificationLevel.Error, "Timed out waiting for registration response");
                }
                
                catch (Exception ex)
                {
                    DisplayNotification(ResourceNotificationLevel.Error, $"Error starting digital worker: {ex}.");
                    DisplayNotification(ResourceNotificationLevel.Comment, "Waiting to retry registration...");
                    _taskDelay.Delay(TimeSpan.FromSeconds(5)).Await();
                }
            }

            return false;
            
        }

        private bool BeginReceivingProcesses()
        {
            DisplayNotification(ResourceNotificationLevel.Comment, $"Preparing to receive process messages");
            try
            {
                _queueCoordinator.BeginReceivingMessages().Await();
            }
            catch (Exception ex)
            {
                DisplayNotification(ResourceNotificationLevel.Error,
                    $"Error connecting message consumer: {ex.Message}.");
                return false;
            }

            DisplayNotification(ResourceNotificationLevel.Comment,
                $"Receiving processes");
            return true;
        }

        private void StartBus()
        {
            if (_busStatus == MessageBusStatus.ReadyToStart)
            {
                DisplayNotification(ResourceNotificationLevel.Comment, $"Starting message bus");
                var (host, username) = _bus.Configuration;
                DisplayNotification(ResourceNotificationLevel.Comment, $"Connecting to message broker {host} as {username}");
                try
                {
                    _bus.Start(MessageBusStartTimeout);
                    DisplayNotification(ResourceNotificationLevel.Comment, $"Message bus started");
                    _busStatus = MessageBusStatus.Started;
                }
                catch (Exception ex)
                {
                    DisplayNotification(ResourceNotificationLevel.Error,
                        $"Error starting message bus: {ex.Message}. The Digital Worker will need to be shutdown and started when the queue server is available.");
                    _busStatus = MessageBusStatus.StartFailed;
                }
            }
        }

        protected override void Stop(bool shuttingDown)
        {
            if (shuttingDown)
            {
                // _bus used throughout application lifetime and is not stopped when restarting
                if (_bus != null)
                {
                    if (_registrationStatus == RegisteredStatus.Registered)
                    {
                        Task.Run(async () => await _lifecycleEventPublisher.Stop()).Await();
                        DisplayNotification(ResourceNotificationLevel.Comment, $"{_context.StartUpOptions.Name.FullName} stopped");
                    }

                    _registrationStatus = RegisteredStatus.None;
                    DisplayNotification(ResourceNotificationLevel.Comment, "Stopping message bus");
                    _bus.Stop();
                }
                _bus = null;
            }
        }
        
        protected override bool AllowRestart() => false;

        protected override bool AllowShutDown() => true;
        
        protected override string GetEventLogSource() => $"Digital Worker - {_context.StartUpOptions.Name.FullName}";

        private enum MessageBusStatus { ReadyToStart, Started, StartFailed }
    }
}
