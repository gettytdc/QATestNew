namespace BluePrism.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Domain;
    using Domain.Errors;
    using Func;
    using Logging;

    public class SessionsService : ISessionsService
    {
        private readonly IAdapterAuthenticatedMethodRunner<ISessionServerAdapter> _sessionMethodRunner;
        private readonly ILogger<SessionsService> _logger;

        public SessionsService(IAdapterAuthenticatedMethodRunner<ISessionServerAdapter> sessionMethodRunner, ILogger<SessionsService> logger)
        {
            _sessionMethodRunner = sessionMethodRunner;
            _logger = logger;
        }

        public Task<Result<ItemsPage<Session>>> GetSessions(SessionParameters sessionParameters) =>
            _sessionMethodRunner.ExecuteForUser(x => x.GetSessions(sessionParameters));

        public Task<Result<Session>> GetSessionById(Guid sessionId) =>
            _sessionMethodRunner.ExecuteForUser(x => x.GetActualSessionById(sessionId))
                .OnSuccess(() => _logger.Debug("Successfully retrieved session with ID {0}", sessionId))
                .OnError<SessionNotFoundError, Session>(_ => _logger.Debug("Attempt to retrieve session with ID '{0}', but no matching session was found", sessionId));
    }
}
