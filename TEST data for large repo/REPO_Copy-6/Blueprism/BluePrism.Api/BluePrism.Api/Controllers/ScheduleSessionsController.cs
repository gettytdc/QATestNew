namespace BluePrism.Api.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Func;
    using Func.AspNet;
    using Models;
    using Services;

    [RoutePrefix("schedules/{scheduleId}/sessions")]
    public class ScheduleSessionsController : ResultControllerBase
    {
        private readonly ISchedulesService _schedulesService;

        public ScheduleSessionsController(ISchedulesService schedulesService)
        {
            _schedulesService = schedulesService;
        }

        [HttpPost, Route("")]
        [OnSuccess(HttpStatusCode.Accepted)]
        public Task<Result<ScheduleOneOffScheduleRunResponseModel>> ScheduleOneOffScheduleRun(int scheduleId, ScheduleOneOffScheduleRunModel model) =>
            ValidateModel()
                .Then(() => _schedulesService.ScheduleOneOffScheduleRun(scheduleId, model.StartTime))
                .ThenMap(x => new ScheduleOneOffScheduleRunResponseModel {ScheduledTime = x});
    }
}
