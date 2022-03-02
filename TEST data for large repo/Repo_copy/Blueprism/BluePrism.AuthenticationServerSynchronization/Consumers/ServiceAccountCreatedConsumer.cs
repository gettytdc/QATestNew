using BluePrism.AutomateAppCore;
using BluePrism.Server.Domain.Models;
using MassTransit;
using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using BPC.IMS.Messages.Events;
using ImsServer.Core.Enums;

namespace BluePrism.AuthenticationServerSynchronization.Consumers
{
    public class ServiceAccountCreatedConsumer : IConsumer<ServiceAccountCreated>
    {
        private readonly IServer _server;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ServiceAccountCreatedConsumer(IServer server)
        {
            _server = server;
        }

        public Task Consume(ConsumeContext<ServiceAccountCreated> context)
        {
            var clientId = context.Message.Id;
            var clientName = context.Message.Name;

            Log.Info($"Message to create service account {clientName} with client ID {clientId} and message date of {context.Message.Date}");

            if (string.IsNullOrEmpty(clientName))
            {
                Log.Error("Invalid ServiceAccountCreated message. ClientName is null");
                throw new ArgumentException("Message.Name", nameof(context));
            }

            if (string.IsNullOrEmpty(clientId))
            {
                Log.Error("Invalid ServiceAccountCreated message. Id is null");
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
                var hasBluePrismApiScope = context.Message.AllowedPermissions.Any(x => x == ServiceAccountPermission.BluePrismApi);
                var name = _server.CreateNewServiceAccount(clientName, clientId, hasBluePrismApiScope);
                Log.Info($"Created new service account with client name {name}, client id: {clientId}");
            }
            catch (AuthenticationServerClientIdAlreadyInUseException)
            {
                Log.Info($"Authentication server service account with client ID {clientId} already exists in the BPE database.");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured when creating service account {clientName} with client ID {clientId}.");
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
