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
    public class ServiceAccountUpdatedConsumer : IConsumer<ServiceAccountUpdated>
    {
        private readonly IServer _server;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ServiceAccountUpdatedConsumer(IServer server)
        {
            _server = server;
        }

        public Task Consume(ConsumeContext<ServiceAccountUpdated> context)
        {
            var id = context.Message.Id;
            var name = context.Message.Name;

            Log.Info($"Message to update service account {name} with client ID {id} and message date of {context.Message.Date}");

            if (string.IsNullOrEmpty(name))
            {
                Log.Error("Invalid ServiceAccountUpdated message. Name is null");
                throw new ArgumentException("Message.Name", nameof(context));
            }

            if (string.IsNullOrEmpty(id))
            {
                Log.Error("Invalid ServiceAccountUpdated message. Id is null");
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
                _server.UpdateServiceAccount(id, name, hasBluePrismApiScope, context.Message.Date);
                Log.Info($"Updated service account with client id: {id}");
            }
            catch (SynchronizationOutOfSequenceException)
            {
                Log.Info($"The authentication service account with client id: {id} has already been synchronized.");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured when updating service account {name} with client ID {id}.");
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
