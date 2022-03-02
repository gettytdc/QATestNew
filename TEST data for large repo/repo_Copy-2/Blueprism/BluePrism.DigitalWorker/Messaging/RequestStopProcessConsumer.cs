using BluePrism.DigitalWorker.Sessions;
using MassTransit;
using System;
using System.Threading.Tasks;
using BluePrism.DigitalWorker.Messages.Commands;

namespace BluePrism.DigitalWorker.Messaging
{
    public class RequestStopProcessConsumer : IConsumer<RequestStopProcess>
    {
        private readonly IRunningSessionRegistry  _runningSessionRegistry;

        public RequestStopProcessConsumer(IRunningSessionRegistry  runningSessionRegistry)
        {
            _runningSessionRegistry = runningSessionRegistry ?? throw new ArgumentNullException(nameof(runningSessionRegistry));
        }

        public async Task Consume(ConsumeContext<RequestStopProcess> context)
        {
            ValidateParameters(context);

            RequestStopProcess(context.Message.SessionId);

            await Task.CompletedTask;
        }

        private static void ValidateParameters(ConsumeContext<RequestStopProcess> context)
        {
            if (context.Message.SessionId == Guid.Empty)
                throw new ArgumentException("No valid Session Id provided.");
        }

        private void RequestStopProcess(Guid sessionId)
        {
            var runner = _runningSessionRegistry.Get(sessionId);

            if (runner == null)
                return;

            runner.StopRequested = true;
        }
    }
}
