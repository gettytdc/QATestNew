namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface ISessionLogsServerAdapter : IServerAdapter
    {
        Task<Result<ItemsPage<SessionLogItem>>> GetLogs(int sessionNumber, SessionLogsParameters sessionLogsParameters);
        Task<Result<SessionLogItemParameters>> GetLogParameters(Guid sessionId, long logId);
    }
}
