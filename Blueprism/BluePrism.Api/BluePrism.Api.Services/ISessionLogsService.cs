namespace BluePrism.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface ISessionLogsService
    {
        Task<Result<ItemsPage<SessionLogItem>>> GetSessionLogs(Guid sessionId, SessionLogsParameters sessionLogsParameters);
        Task<Result<SessionLogItemParameters>> GetLogParameters(Guid sessionId, long logId);
    }
}
