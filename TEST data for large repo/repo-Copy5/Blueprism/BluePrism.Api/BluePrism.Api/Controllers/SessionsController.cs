namespace BluePrism.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Mappers;
    using Services;
    using Func;
    using Models;
    using static Func.ResultHelper;

    [RoutePrefix("sessions")]
    public class SessionsController : ResultControllerBase
    {
        private readonly ISessionsService _sessionsService;

        public SessionsController(ISessionsService sessionsService)
        {
            _sessionsService = sessionsService;
        }

        [HttpGet, Route("")]
        public async Task<Result<ItemsPageModel<SessionModel>>> Get([FromUri] SessionParameters sessionParameters) =>
            await
                ValidateModel()
                    .Then(() => Succeed((sessionParameters ?? new SessionParameters()).ToDomainObject()))
                    .Then(x => _sessionsService.GetSessions(x))
                    .Then(item => Succeed(item.ToModelItemsPage(x => x.ToModelObject())));

        [HttpGet, Route("{sessionId:guid}")]
        public async Task<Result<SessionModel>> GetSessionInfo(Guid sessionId) =>
            await _sessionsService.GetSessionById(sessionId)
                .Then(x => Succeed(x.ToModelObject()));
    }
}
