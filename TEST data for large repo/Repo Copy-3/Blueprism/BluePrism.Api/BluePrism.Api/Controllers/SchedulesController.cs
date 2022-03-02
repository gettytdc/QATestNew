namespace BluePrism.Api.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Func;
    using Func.AspNet;
    using Mappers;
    using Models;
    using Services;
    using static Func.ResultHelper;

    [RoutePrefix("schedules")]
    public class SchedulesController : ResultControllerBase
    {
        private readonly ISchedulesService _schedulesService;

        public SchedulesController(ISchedulesService schedulesService)
        {
            _schedulesService = schedulesService;
        }

        [HttpGet, Route("")]
        public async Task<Result<ItemsPageModel<ScheduleModel>>> Get([FromUri] ScheduleParameters scheduleParameters) =>
            await ValidateModel()
                .Then(() => Succeed((scheduleParameters ?? new ScheduleParameters()).ToDomainObject()))
                .Then(x => _schedulesService.GetSchedules(x))
                .Then(x => Succeed(x.ToModelItemsPage(y => y.ToModelObject())));

        [HttpGet, Route("{scheduleId}")]
        public async Task<Result<ScheduleModel>> Get([FromUri] int scheduleId) =>
            await _schedulesService.GetSchedule(scheduleId)
                .Then(x => Succeed(x.ToModelObject()));

        [HttpPut, Route("{scheduleId}")]
        [OnSuccess(HttpStatusCode.NoContent)]
        public async Task<Result> UpdateSchedule([FromUri] int scheduleId, [FromBody] UpdateScheduleModel scheduleChanges) =>
            await ValidateModel()
                .Then(() => _schedulesService.ModifySchedule(scheduleId, scheduleChanges.ToDomain()));        

        [HttpGet, Route("logs")]
        public async Task<Result<ItemsPageModel<ScheduleLogModel>>> GetScheduleLogs([FromUri] ScheduleLogParameters scheduleLogParameters) =>
            await GetScheduleLogsAndConvertToModel(scheduleLogParameters);

        [HttpGet, Route("{scheduleId}/logs")]
        public async Task<Result<ItemsPageModel<ScheduleLogModel>>> GetScheduleLogsByScheduleId(int scheduleId, [FromUri] ScheduleLogParameters scheduleLogParameters) =>
            await GetScheduleLogsAndConvertToModel(scheduleLogParameters, scheduleId);

        [HttpGet, Route("tasks/{taskId:int}/sessions")]
        public async Task<Result<IEnumerable<ScheduledSessionModel>>> GetScheduledSessions(int taskId) =>
            await _schedulesService.GetScheduledSessions(taskId)
                .Then(x => Succeed(x.Select(s => s.ToModelObject())));

        private Task<Result<ItemsPageModel<ScheduleLogModel>>> GetScheduleLogsAndConvertToModel(ScheduleLogParameters scheduleLogParameters, int? scheduleId = null) =>
            ValidateModel()
                .Then(() => Succeed((scheduleLogParameters ?? new ScheduleLogParameters()).ToDomainObject(scheduleId)))
                .Then(x => _schedulesService.GetScheduleLogs(x))
                .Then(x => Succeed(x.ToModelItemsPage(y => y.ToModelObject())));

        [HttpGet, Route("{scheduleId}/tasks")]
        public async Task<Result<IEnumerable<ScheduledTaskModel>>> GetScheduledTasks(int scheduleId) =>
            await _schedulesService.GetScheduledTasks(scheduleId)
                .Then(x => Succeed(x.Select(m => m.ToModelObject())));
    }
}
