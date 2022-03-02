namespace BluePrism.Api.BpLibAdapters
{
    using System.Linq;
    using AutomateAppCore;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain.Dashboard;
    using Domain.Errors;
    using Func;
    using Mappers;
    using Mappers.Dashboard;
    using Server.Domain.Models;
    using static ServerResultTask;

    public class DashboardsServerAdapter : IDashboardsServerAdapter
    {
        private readonly IServer _server;

        public DashboardsServerAdapter(IServer server)
        {
            _server = server;
        }

        public Task<Result<IEnumerable<WorkQueueComposition>>>
            GetWorkQueueComposition(IEnumerable<Guid> workQueueIds) =>
            RunOnServer(() => _server.GetWorkQueueCompositions(workQueueIds)
                    .Select(x => x.ToDomainObject()))
                .Catch<PermissionException>(ex => ResultHelper<IEnumerable<WorkQueueComposition>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<IEnumerable<ResourceUtilization>>> GetResourceUtilization(
            Domain.ResourceUtilizationParameters resourceUtilizationParameters) =>
            RunOnServer(() => _server.GetResourceUtilization(resourceUtilizationParameters.ToBluePrismObject())
                    .Select(x => x.ToDomainObject()))
                .Catch<PermissionException>(ex => ResultHelper<IEnumerable<ResourceUtilization>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<IEnumerable<ResourcesSummaryUtilization>>> GetResourcesSummaryUtilization(
            Domain.ResourcesSummaryUtilizationParameters resourcesSummaryUtilizationParameters) =>
                RunOnServer(() => _server.GetResourcesSummaryUtilization(resourcesSummaryUtilizationParameters.ToBluePrismObject())
                    .Select(x => x.ToDomainObject()))
                .Catch<PermissionException>(ex => ResultHelper<IEnumerable<ResourcesSummaryUtilization>>.Fail(new PermissionError(ex.Message)));
    }
}
