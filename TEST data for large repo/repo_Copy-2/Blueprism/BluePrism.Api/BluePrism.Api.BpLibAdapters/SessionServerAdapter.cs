namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Mappers;
    using Domain;
    using Domain.Errors;
    using AutomateAppCore;
    using BluePrism.Server.Domain.Models;
    using Func;

    using static ServerResultTask;
    using static Func.ResultHelper;

    public class SessionServerAdapter : ISessionServerAdapter
    {
        private readonly IServer _server;

        public SessionServerAdapter(IServer server) =>
            _server = server;

        public Task<Result<ItemsPage<Session>>> GetSessions(SessionParameters sessionParameters) =>
            RunOnServer(() => _server.GetActualSessionsFilteredAndOrdered(sessionParameters.ToBluePrismObject())
                    .Select(x => x.ToDomainObject()).ToArray()
                    .Map(x => x.ToItemsPage(sessionParameters)))
                .Catch<PermissionException>(ex => ResultHelper<ItemsPage<Session>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<int>> GetSessionNumber(Guid sessionId) =>
            RunOnServer(() => _server.GetSessionNumber(sessionId))
                .Execute()
                .Then(x => x >= 0 ? Succeed(x) : ResultHelper<int>.Fail<SessionNotFoundError>());

        public Task<Result<Session>> GetActualSessionById(Guid sessionId) =>
            RunOnServer(() => _server.GetActualSessionById(sessionId)?.ToDomainObject())
                .Catch<PermissionException>(ex => ResultHelper<Session>.Fail(new PermissionError(ex.Message)))
                .OnNull(() => ResultHelper<Session>.Fail(new SessionNotFoundError()));

    }
}
