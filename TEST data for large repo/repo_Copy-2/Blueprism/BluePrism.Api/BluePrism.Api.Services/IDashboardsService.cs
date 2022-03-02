namespace BluePrism.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Dashboard;
    using Func;

    public interface IDashboardsService
    {
        Task<Result<IEnumerable<WorkQueueComposition>>> GetWorkQueueCompositions(IEnumerable<Guid> workQueueIds);
        Task<Result<IEnumerable<ResourceUtilization>>> GetResourceUtilization(ResourceUtilizationParameters resourceUtilizationParameters);
        Task<Result<IEnumerable<ResourcesSummaryUtilization>>> GetResourcesSummaryUtilization(ResourcesSummaryUtilizationParameters resourcesSummaryUtilizationParameters);
    }
}
