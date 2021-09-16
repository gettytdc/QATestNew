namespace BluePrism.DocumentProcessing.Integration.Messaging
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Utility;
    using MassTransit;
    using ServerPlugin.Messages;
    using NLog;

    public class BatchFinishedEventListener : IConsumer<BatchFinishedMessage>, IBatchFinishedEventListener
    {
        private readonly IBusControl _bus;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public delegate void DocumentProcessingCompleteEvent(object sender, DocumentProcessingCompleteEventArgs eventArgs);
        public event DocumentProcessingCompleteEvent DocumentProcessingComplete;

        public BatchFinishedEventListener(
            string queueNameSuffix,
            MessageQueueConfiguration messageQueueConfiguration,
            IBusFactorySelector busFactory)
        {
            _bus = ConfigureBus(busFactory, messageQueueConfiguration, queueNameSuffix);
            _bus.Start();
        }

        public Task Consume(ConsumeContext<BatchFinishedMessage> context) =>
            Task.Run(() =>
                DocumentProcessingComplete?.Invoke(this,
                    new DocumentProcessingCompleteEventArgs
                    {
                        BatchId = context.Message.BatchId.Value,
                        Documents = context.Message.Documents.Select(x => (x.Id.Value, x.TypeId.Value)).ToArray()
                    }));


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        _bus.Stop();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
            }

            _disposed = true;
        }

        ~BatchFinishedEventListener()
        {
            Dispose(false);
        }


        private IBusControl ConfigureBus(IBusFactorySelector busFactory, MessageQueueConfiguration messageQueueConfiguration, string queueNameSuffix) =>
            busFactory.CreateUsingRabbitMq(config =>
            {
                config.Host(new Uri(messageQueueConfiguration.BrokerUrl), x =>
                {
                    x.Username(messageQueueConfiguration.Username);
                    x.Password(messageQueueConfiguration.Password.MakeInsecure());
                });

                config.ReceiveEndpoint(
                    $"{messageQueueConfiguration.QueueName}_{queueNameSuffix}",
                    e => e.Instance(this));
            });
    }
}
