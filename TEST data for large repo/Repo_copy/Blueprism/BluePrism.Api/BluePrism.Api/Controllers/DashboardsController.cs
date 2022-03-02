namespace BluePrism.Api.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Errors;
    using Func;
    using Mappers.Dashboard;
    using Models.Dashboard;
    using Services;
    using Models;
    using Mappers;
    using static Func.ResultHelper;

    [RoutePrefix("dashboards")]
    public class DashboardsController : ResultControllerBase
    {
        private readonly IDashboardsService _dashboardsService;

        public DashboardsController(IDashboardsService dashboardsService)
        {
            _dashboardsService = dashboardsService;
        }

        [HttpGet, Route("workQueueCompositions")]
        public async Task<Result<IEnumerable<WorkQueueCompositionModel>>> GetWorkQueueCompositions([FromUri] WorkQueueCompositionParameters workQueueCompositionParameters)
        {
            if (workQueueCompositionParameters == null)
                return ResultHelper<IEnumerable<WorkQueueCompositionModel>>.Fail(new EmptyCollectionError($"{nameof(workQueueCompositionParameters)} parameter is required"));

            return await ValidateModel()
                .Then(() => _dashboardsService.GetWorkQueueCompositions(workQueueCompositionParameters.WorkQueueIds))
                .Then(x => Succeed(x.ToModel()));
        }

        [HttpGet, Route("resourceUtilization")]
        public async Task<Result<IEnumerable<ResourceUtilization>>> GetResourceUtilization([FromUri] ResourceUtilizationParameters resourceUtilizationParameters)
        {
            if (resourceUtilizationParameters == null)
                return ResultHelper<IEnumerable<ResourceUtilization>>.Fail(new EmptyCollectionError($"{nameof(resourceUtilizationParameters)} parameter is required"));

            return await ValidateModel()
                .Then(() => _dashboardsService.GetResourceUtilization(resourceUtilizationParameters.ToDomainObject()))
                .Then(x => Succeed(x.ToModel()));
        }

        [HttpGet, Route("resourcesSummaryUtilization")]
        public async Task<Result<IEnumerable<ResourcesSummaryUtilization>>> GetResourceUtilizationByDateRange([FromUri] ResourcesSummaryUtilizationParameters resourcesSummaryUtilizationParameters)
        {
            if (resourcesSummaryUtilizationParameters == null)
                return ResultHelper<IEnumerable<ResourcesSummaryUtilization>>.Fail(new EmptyCollectionError($"{nameof(resourcesSummaryUtilizationParameters)} parameter is required"));

            return await ValidateModel()
                .Then(() => _dashboardsService.GetResourcesSummaryUtilization(resourcesSummaryUtilizationParameters.ToDomainObject()))
                .Then(x => Succeed(x.ToModel()));
        }
    }
}
