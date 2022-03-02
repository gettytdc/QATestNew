using System;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore;
using BluePrism.Server.Domain.Models;
using BPC.IMS.Messages.Events;
using MassTransit;
using NLog;

namespace BluePrism.AuthenticationServerSynchronization.Consumers
{
    public class UserUnretiredConsumer : IConsumer<UserUnretired>
    {
        private readonly IServer _server;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public UserUnretiredConsumer(IServer server)
        {
            _server = server;
        }

        public Task Consume(ConsumeContext<UserUnretired> context)
        {
            var authenticationServerUserId = context.Message.Id;
            var synchronizationDate = context.Message.Date;
            Log.Info($"Message to unretire authentication server user with id {authenticationServerUserId} and message date of {synchronizationDate}");

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
                _server.UnretireAuthenticationServerUser(authenticationServerUserId, synchronizationDate);
                Log.Info($"Unretired user with authentication server user id: {authenticationServerUserId}");
            }
            catch (SynchronizationOutOfSequenceException)
            {
                Log.Info($"The authentication user with id: {authenticationServerUserId} has already been synchronized.");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured when unretiring user with authentication server user id: {authenticationServerUserId}");
                throw;
            }

            return Task.CompletedTask;
        }
    }
}
