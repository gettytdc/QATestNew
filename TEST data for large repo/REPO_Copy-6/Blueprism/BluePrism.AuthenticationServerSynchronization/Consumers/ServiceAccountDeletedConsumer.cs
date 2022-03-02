using System;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore;
using BPC.IMS.Messages.Events;
using MassTransit;
using NLog;

namespace BluePrism.AuthenticationServerSynchronization.Consumers
{
    public class ServiceAccountDeletedConsumer : IConsumer<ServiceAccountDeleted>
    {
        private readonly IServer _server;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ServiceAccountDeletedConsumer(IServer server)
        {
            _server = server;
        }

        public Task Consume(ConsumeContext<ServiceAccountDeleted> context)
        {
            var clientId = context.Message.Id;
            var synchronizationDate = context.Message.Date;

            Log.Info($"Message to delete service account with client ID {clientId} and message date of {synchronizationDate}");

            if (string.IsNullOrEmpty(clientId))
            {
                Log.Error("Invalid ServiceAccountDeleted message. Id is null");
                throw new ArgumentException("Message.Id", nameof(context));
            }

            try
            {
                if (!_server.IsAuthenticationServerIntegrationEnabled())
                {
                    return Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured when getting the signon settings.");
                throw;
            }

            try
            {
                _server.DeleteServiceAccount(clientId, synchronizationDate);

                Log.Info($"Deleted service account with client id: {clientId}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured when deleting service account with client ID {clientId}.");
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
