namespace BluePrism.Api.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Domain;
    using Domain.Errors;
    using Func;
    using Logging;

    using static Func.ResultHelper;

    public class SessionLogsService : ISessionLogsService
    {
        private readonly IAdapterAuthenticatedMethodRunner<ISessionServerAdapter> _sessionMethodRunner;
        private readonly IAdapterAuthenticatedMethodRunner<ISessionLogsServerAdapter> _sessionLogsMethodRunner;
        private readonly ILogger<SessionLogsService> _logger;
        private readonly SessionLogConfiguration _sessionLogConfiguration;

        public SessionLogsService(
            IAdapterAuthenticatedMethodRunner<ISessionServerAdapter> sessionMethodRunner,
            IAdapterAuthenticatedMethodRunner<ISessionLogsServerAdapter> sessionLogsMethodRunner,
            ILogger<SessionLogsService> logger,
            SessionLogConfiguration sessionLogConfiguration)
        {
            _sessionMethodRunner = sessionMethodRunner;
            _sessionLogsMethodRunner = sessionLogsMethodRunner;
            _logger = logger;
            _sessionLogConfiguration = sessionLogConfiguration;
        }

        public Task<Result<ItemsPage<SessionLogItem>>> GetSessionLogs(Guid sessionId, SessionLogsParameters sessionLogsParameters) =>
            GetSessionNumberForSessionId(sessionId)
                .Then(number => GetSessionLogsForSessionNumber(number, sessionLogsParameters))
                .Then(page => TruncateItemResults(page))
                .OnSuccess(() => _logger.Debug("Successfully retrieved logs for session {0}", sessionId))
                .OnError((SessionNotFoundError _) => _logger.Info("Attempted to get logs for session {0} but the session was not found", sessionId));

        public Task<Result<SessionLogItemParameters>> GetLogParameters(Guid sessionId, long logId) =>
            _sessionLogsMethodRunner.ExecuteForUser(server => server.GetLogParameters(sessionId, logId))
                .OnSuccess(() => _logger.Debug("Sucessfully retrieved log parameters for session: {0}, log: {1}", sessionId, logId))
                .OnError((SessionNotFoundError _) => _logger.Info("Attempted to get log parameters for session: {0}, log: {1} but the session was not found", sessionId, logId))
                .OnError((LogNotFoundError _) => _logger.Info("Attempted to get log parameters for session: {0}, log: {1} but the log was not found", sessionId, logId))
                .OnError((PermissionError _) => _logger.Info("Attempted to get log parameters for session: {0}, log: {1} but no permission", sessionId, logId));

        private Task<Result<int>> GetSessionNumberForSessionId(Guid sessionId) =>
            _sessionMethodRunner.ExecuteForUser(server => server.GetSessionNumber(sessionId));

        private Task<Result<ItemsPage<SessionLogItem>>> GetSessionLogsForSessionNumber(int sessionNumber, SessionLogsParameters sessionLogsParameters) =>
            _sessionLogsMethodRunner.ExecuteForUser(server => server.GetLogs(sessionNumber, sessionLogsParameters));

        private Result<ItemsPage<SessionLogItem>> TruncateItemResults(ItemsPage<SessionLogItem> page) =>
            Succeed(new ItemsPage<SessionLogItem>
            {
                Items = page.Items?.Select(x =>
                        x.Result.Length > _sessionLogConfiguration.MaxResultStringLength
                            ? new SessionLogItem
                            {
                                LogId = x.LogId,
                                StageName = x.StageName,
                                StageType = x.StageType,
                                ResourceStartTime = x.ResourceStartTime,
                                Result = $"{x.Result.Substring(0, _sessionLogConfiguration.MaxResultStringLength)}...",
                            }
                            : x
                    ),
                PagingToken = page.PagingToken
            });

    }
}
