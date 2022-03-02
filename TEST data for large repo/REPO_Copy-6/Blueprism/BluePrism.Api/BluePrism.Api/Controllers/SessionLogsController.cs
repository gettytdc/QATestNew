namespace BluePrism.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Func;
    using Mappers;
    using Models;
    using Services;
    using static Func.ResultHelper;
    using SessionLogsParameters = Models.SessionLogsParameters;

    [RoutePrefix("sessions/{sessionId:guid}/logs")]
    public class SessionLogsController : ResultControllerBase
    {
        private readonly ISessionLogsService _sessionLogsService;

        public SessionLogsController(ISessionLogsService sessionLogsService)
        {
            _sessionLogsService = sessionLogsService;
        }

        [HttpGet, Route("")]
        public async Task<Result<ItemsPageModel<SessionLogItemModel>>> GetSessionLogs(Guid sessionId, [FromUri] SessionLogsParameters sessionLogsParameters) =>
            await ValidateModel()
                .Then(() => Succeed((sessionLogsParameters ?? new SessionLogsParameters()).ToDomainObject()))
                .Then(x => _sessionLogsService.GetSessionLogs(sessionId, x))
                .Then(x => Succeed(x.ToModelItemsPage(item => item.ToModel())));

        [HttpGet, Route("{logId:long}/parameters")]
        public async Task<Result<SessionLogParametersModel>> GetSessionLogParameters(Guid sessionId, long logId) =>
            await _sessionLogsService.GetLogParameters(sessionId, logId)
                .Then(x=> Succeed(x.ToModel()));
    }
}
