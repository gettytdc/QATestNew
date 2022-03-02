namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface ISessionServerAdapter : IServerAdapter
    {
        Task<Result<ItemsPage<Session>>> GetSessions(SessionParameters sessionParameters);

        Task<Result<int>> GetSessionNumber(Guid sessionId);

        Task<Result<Session>> GetActualSessionById(Guid sessionId);
    }
}
