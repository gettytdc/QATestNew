using System;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore;
using BluePrism.Server.Domain.Models;
using BPC.IMS.Messages.Events;
using MassTransit;
using NLog;

namespace BluePrism.AuthenticationServerSynchronization.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreated>
    {
        private readonly IServer _server;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public UserCreatedConsumer(IServer server)
        {
            _server = server;
        }


        public Task Consume(ConsumeContext<UserCreated> context)
        {
            var username = context.Message.Username;
            var authServerId = context.Message.Id;
            Log.Info($"Message to create user {username} with ID {authServerId} and message date of {context.Message.Date}");

            if (string.IsNullOrEmpty(username))
            {
                Log.Error("Invalid UserCreated message. Username is null");
                throw new ArgumentException("Message.Username is null", nameof(context));
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
                var name = _server.CreateNewAuthenticationServerUserWithUniqueName(username, authServerId);
                Log.Info($"Created new user with name {name}, auth server id: {authServerId}");
            }
            catch(AuthenticationServerUserIdAlreadyInUseException)
            {
                Log.Info($"Authentication server user with ID {authServerId} already exists in the BPE database.");
                throw;
            }
            catch(Exception ex)
            {
                Log.Error(ex, $"An error occured when creating user {username} with ID {authServerId}.");
                throw;
            }

            return Task.CompletedTask;
        }

    }
}
