using System;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.DigitalWorker.Sessions.Coordination;
using MassTransit;
using NLog;

namespace BluePrism.DigitalWorker.Messaging.Observers
{
    public class ExclusiveProcessLockObserver : IExclusiveProcessLockObserver
    {
        private static readonly TimeSpan StartTimeoutDuration = TimeSpan.FromSeconds(10);
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        readonly IExclusiveProcessLock _lock;

        public ExclusiveProcessLockObserver(IExclusiveProcessLock @lock)
        {
            _lock = @lock;
        }
        
        public async Task PreReceive(ReceiveContext context)
        {
            if (_lock.State == ExclusiveProcessLockState.Locked)
            {
                try
                {
                    using (var timeoutTokenSource = new CancellationTokenSource(StartTimeoutDuration))
                    {
                        await _lock.Wait(timeoutTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.Debug("PreReceive - Throwing exception to stop consuming message on queue {0}", context.InputAddress);
                    // Throwing an exception within an observer results in the message being returned
                    // to the queue at its original position, allowing it to be received by the next
                    // available consumer (see BrokeredMessageReceiver and RabbitMqBasicConsumer in
                    // MassTransit). The wait step above will prevent messages from being redelivered
                    // to the same digital worker in a tight loop.
                    throw new DigitalWorkerBusyException("Unable to start process as exclusive process is running");
                }
            }
        }
        
        public Task PostReceive(ReceiveContext context)
        {
            return Task.CompletedTask;
        }

        public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
            where T : class
        {
            return Task.CompletedTask;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan elapsed, string consumerType, Exception exception) where T : class
        {
            if (exception is DigitalWorkerBusyException)
            {
                Logger.Debug("ConsumeFault - Rethrowing DigitalWorkerBusyException from consumer");
                // This specific exception indicates that and we want the message to be redelivered.
                // Exceptions thrown within a consumer will result in message being moved to the error
                // This specific exception indicates that the process cannot be run due to another process
                // running. We rethrow it to all the message to be redelivered - see notes above (PreReceive method).
                throw exception;
            }
            return Task.CompletedTask;
        }

        public Task ReceiveFault(ReceiveContext context, Exception exception)
        {
            // called when an exception occurs early in the message processing, such as deserialization, etc.
            return Task.CompletedTask;
        }
    }
}
