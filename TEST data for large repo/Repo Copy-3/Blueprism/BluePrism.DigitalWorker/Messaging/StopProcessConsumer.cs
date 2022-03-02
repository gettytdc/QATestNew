using BluePrism.DigitalWorker.Sessions;
using MassTransit;
using System;
using System.Threading.Tasks;
using BluePrism.DigitalWorker.Messages.Commands;

namespace BluePrism.DigitalWorker.Messaging
{
    public class StopProcessConsumer : IConsumer<StopProcess>
    {
        private readonly IRunningSessionRegistry  _runningSessionRegistry;
        
        public StopProcessConsumer(IRunningSessionRegistry runningSessionRegistry)
        {
            _runningSessionRegistry = runningSessionRegistry ?? throw new ArgumentNullException(nameof(runningSessionRegistry));
        }

        public async Task Consume(ConsumeContext<StopProcess> context)
        {
            ValidateParameters(context);

            StopProcess(context.Message.SessionId, context.Message.Username);

            await Task.CompletedTask;
        }

        private static void ValidateParameters(ConsumeContext<StopProcess> context)
        {
            if (context.Message.SessionId == Guid.Empty)
                throw new ArgumentException("No valid Session Id provided.");

            if (string.IsNullOrEmpty(context.Message.Username))
                throw new ArgumentException("No valid Username provided.");
        }

        private void StopProcess(Guid sessionId, string userName)
        {
            var runner = _runningSessionRegistry.Get(sessionId);

            if (runner == null)
                return;

            runner.StopProcess(userName, string.Empty);
        }
    }
}
