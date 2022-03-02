namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Dashboard;
    using Func;

    public interface IDashboardsServerAdapter : IServerAdapter
    {
        Task<Result<IEnumerable<WorkQueueComposition>>> GetWorkQueueComposition(IEnumerable<Guid> workQueueIds);
        Task<Result<IEnumerable<ResourceUtilization>>> GetResourceUtilization(ResourceUtilizationParameters resourceUtilizationParameters);
        Task<Result<IEnumerable<ResourcesSummaryUtilization>>> GetResourcesSummaryUtilization(ResourcesSummaryUtilizationParameters resourcesSummaryUtilizationParameters);
    }
}
