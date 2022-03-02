namespace BluePrism.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Domain;
    using Domain.Dashboard;
    using Domain.Errors;
    using Func;
    using Logging;

    public class DashboardsService : IDashboardsService
    {
        private readonly IAdapterAuthenticatedMethodRunner<IDashboardsServerAdapter> _dashboardServerMethodRunner;
        private readonly ILogger<WorkQueuesService> _logger;

        public DashboardsService(IAdapterAuthenticatedMethodRunner<IDashboardsServerAdapter> dashboardServerMethodRunner, ILogger<WorkQueuesService> logger)
        {
            _dashboardServerMethodRunner = dashboardServerMethodRunner;
            _logger = logger;
        }

        public Task<Result<IEnumerable<WorkQueueComposition>>> GetWorkQueueCompositions(IEnumerable<Guid> workQueueIds) =>
            _dashboardServerMethodRunner.ExecuteForUser(s => s.GetWorkQueueComposition(workQueueIds))
                .OnSuccess(() => _logger.Debug("Successfully retrieved work queue compositions"))
                .OnError((PermissionError _) => _logger.Info("Attempted to retrieve work queue compositions without permission"));

        public Task<Result<IEnumerable<ResourceUtilization>>> GetResourceUtilization(ResourceUtilizationParameters resourceUtilizationParameters) =>
            _dashboardServerMethodRunner.ExecuteForUser(s => s.GetResourceUtilization(resourceUtilizationParameters))
                .OnSuccess(() => _logger.Debug("Successfully retrieved resource utilization"))
                .OnError((PermissionError _) => _logger.Info("Attempted to retrieve resource utilization without permission"));

        public Task<Result<IEnumerable<ResourcesSummaryUtilization>>> GetResourcesSummaryUtilization(
            ResourcesSummaryUtilizationParameters resourcesSummaryUtilizationParameters) =>
            _dashboardServerMethodRunner.ExecuteForUser(s => s.GetResourcesSummaryUtilization(resourcesSummaryUtilizationParameters))
                .OnSuccess(() => _logger.Debug("Successfully retrieved resources summary utilization"))
                .OnError((PermissionError _) => _logger.Info("Attempted to retrieve resources summary utilization without permission"));
    }
}
