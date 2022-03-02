using BluePrism.DigitalWorker.Sessions;
using MassTransit;
using System;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.DigitalWorker.Messages.Commands;
using BluePrism.DigitalWorker.Messages.Events.Factory;
using BluePrism.DigitalWorker.Sessions.Coordination;
using NLog;
using StartProcessStatus = BluePrism.Cirrus.Sessions.SessionService.Messages.Commands.StartProcessStatus;

namespace BluePrism.DigitalWorker.Messaging
{
    public class RunProcessConsumer : IConsumer<RunProcess>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly ISessionCoordinator _sessionCoordinator;
        private readonly IProcessInfoLoader _processInfoLoader;
        private static readonly TimeSpan StartTimeoutDuration = TimeSpan.FromSeconds(10);
        private readonly string _digitalWorkerName;
        private readonly ISessionServiceClient _sessionServiceClient;

        public RunProcessConsumer(ISessionCoordinator sessionCoordinator, IProcessInfoLoader processInfoLoader,
            ISessionServiceClient sessionServiceClient, DigitalWorkerContext context)
        {
            _sessionCoordinator = sessionCoordinator ?? throw new ArgumentNullException(nameof(sessionCoordinator));
            _processInfoLoader = processInfoLoader ?? throw new ArgumentNullException(nameof(processInfoLoader));
            _sessionServiceClient = sessionServiceClient ?? throw new ArgumentNullException(nameof(sessionServiceClient));
            _digitalWorkerName = context.StartUpOptions.Name.FullName;
        }

        public async Task Consume(ConsumeContext<RunProcess> context)
        {
            CheckParameters(context);
            StartProcessStatus sessionStatus;
                        
            try
            {
                sessionStatus = await _sessionServiceClient.RequestStartProcess(context.Message.SessionId, _digitalWorkerName);
            }
            catch (RequestTimeoutException timeoutException)
            {
                Logger.Error(timeoutException, "Timed out requesting to start process");
                await context.Publish(DigitalWorkerEvents.ProcessPreStartFailed(context.Message.SessionId, DateTimeOffset.Now));
                return;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Error requesting session status");
                throw;
            }

            switch (sessionStatus)
            {
                case StartProcessStatus.ReadyToStart:
                    var process = _processInfoLoader.GetProcess(context.Message.ProcessId);

                    var sessionContext = new SessionContext(context.Message.SessionId, process, context.Message.Username);

                    try
                    {
                        using (var startTimeoutTokenSource = new CancellationTokenSource(StartTimeoutDuration))
                        {
                            await _sessionCoordinator.RunProcess(sessionContext, startTimeoutTokenSource.Token);
                        }
                    }
                    catch (OperationCanceledException exception)
                    {
                        await context.Publish(DigitalWorkerEvents.ProcessNotStarted(context.Message.SessionId, DateTimeOffset.Now));
                        Logger.Info(exception, "Timed out waiting for process to start");
                        throw new DigitalWorkerBusyException("Timed out waiting for process to start", exception);
                    }
                    break;

                case StartProcessStatus.AlreadyStarted:
                    Logger.Info("This session has already been started");
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected session status");
            }
        }

        private static void CheckParameters(ConsumeContext<RunProcess> context)
        {
            var processId = context.Message.ProcessId;

            if (processId == Guid.Empty)
            {
                throw new ArgumentException("No valid ProcessId provided.");
            }
        }
    }
}
