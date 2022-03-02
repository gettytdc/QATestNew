namespace BluePrism.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface ISessionsService
    {
        Task<Result<ItemsPage<Session>>> GetSessions(SessionParameters sessionParameters);
        Task<Result<Session>> GetSessionById(Guid sessionId);
    }
}
