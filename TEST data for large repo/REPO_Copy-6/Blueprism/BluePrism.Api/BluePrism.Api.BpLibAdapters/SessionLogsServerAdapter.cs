namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using Domain;
    using Domain.Errors;
    using Func;
    using Mappers;
    using Server.Domain.Models;
    using static ServerResultTask;
    using static Func.ResultHelper;
    using SessionLogsParameters = Domain.SessionLogsParameters;

    public class SessionLogsServerAdapter : ISessionLogsServerAdapter
    {
        private readonly IServer _server;

        public SessionLogsServerAdapter(IServer server)
        {
            _server = server;
        }

        public Task<Result<ItemsPage<SessionLogItem>>> GetLogs(int sessionNumber, SessionLogsParameters sessionLogsParameters)
            =>
                RunOnServer(() => _server
                    .GetLogs(sessionNumber, sessionLogsParameters.ToBluePrismObject())
                    .Select(x => x.ToDomainObject()).ToArray()
                    .Map(x => x.ToItemsPage(sessionLogsParameters)))
                .Catch<PermissionException>(ex => ResultHelper<ItemsPage<SessionLogItem>>
                    .Fail(new PermissionError(ex.Message)));

        public Task<Result<SessionLogItemParameters>> GetLogParameters(Guid sessionId, long logId) =>
            RunOnServer(() => _server.GetSessionNumber(sessionId))
                .Catch<PermissionException>(ex => ResultHelper<int>
                    .Fail(new PermissionError(ex.Message)))
                .Execute()
                .Then(x => CheckSessionExists(x))
                .Then(sessionNumber => GetSessionLogParameters(logId, sessionNumber));

        private Task<Result<SessionLogItemParameters>> GetSessionLogParameters(long logId, int sessionNumber) =>
            RunOnServer(() => _server.GetSessionAttributeXml(sessionNumber, logId))
                .Catch<PermissionException>(ex => ResultHelper<string>.Fail(new PermissionError(ex.Message)))
                .OnNull(() => ResultHelper<string>.Fail(new LogNotFoundError()))
                .Execute()
                .Then(x => Succeed(x.ToSessionLogItemParameters()));

        private static Result<int> CheckSessionExists(int x) =>
            x >= 0 ? Succeed(x) : ResultHelper<int>.Fail<SessionNotFoundError>();
    }
}
