namespace BluePrism.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Domain;
    using Extensions;
    using Func;
    using Models;
    using Func.AspNet;
    using Mappers;
    using Microsoft.AspNetCore.JsonPatch;
    using Services;

    using static Func.ResultHelper;

    using WorkQueueParameters = Models.WorkQueueParameters;
    using WorkQueueItemParameters = Models.WorkQueueItemParameters;

    [RoutePrefix("workqueues")]
    public class WorkQueuesController : ResultControllerBase
    {
        private readonly IWorkQueuesService _workQueuesService;

        public WorkQueuesController(IWorkQueuesService workQueuesService)
        {
            _workQueuesService = workQueuesService;
        }

        [HttpGet, Route("")]
        public async Task<Result<ItemsPageModel<WorkQueueModel>>> GetWorkQueues([FromUri] WorkQueueParameters workQueueParameters) =>
             await
                ValidateModel()
                .Then(() => Succeed((workQueueParameters ?? new WorkQueueParameters()).ToDomainObject()))
                .Then(x => _workQueuesService.GetWorkQueues(x))
                .Then(x => Succeed(x.ToModelItemsPage(item => item.ToModel())));

        [HttpPost, Route("")]
        [OnSuccess(HttpStatusCode.Created)]
        public async Task<Result<CreateWorkQueueResponseModel>> CreateWorkQueue([FromBody] CreateWorkQueueRequestModel requestModel) =>
            await
                ValidateModel()
                .Then(() => _workQueuesService.CreateWorkQueue(requestModel.ToDomainObject()))
                .Then(x => Succeed(new CreateWorkQueueResponseModel(x)));

        [HttpDelete, Route("{workQueueId}")]
        [OnSuccess(HttpStatusCode.NoContent)]
        public async Task<Result> DeleteWorkQueue(Guid workQueueId) =>
            await _workQueuesService.DeleteWorkQueue(workQueueId);

        [HttpGet, Route("items/{workQueueItemId}")]
        public async Task<Result<WorkQueueItemModel>> GetWorkQueueItem(Guid workQueueItemId) =>
            await _workQueuesService.GetWorkQueueItem(workQueueItemId)
                .Then(x => Succeed(x.ToModel()));

        [HttpPatch, Route("{workQueueId:guid}")]
        [OnSuccess(HttpStatusCode.NoContent)]
        public async Task<Result> UpdateWorkQueue([FromUri] Guid workQueueId, [FromBody] JsonPatchDocument<UpdateWorkQueueModel> patchModel) =>
            await ValidatePatchDocumentModel(patchModel)
                .Then(() =>
                {
                    var workQueuePatchDocument = patchModel.Map().To<WorkQueue>();
                    return _workQueuesService.UpdateWorkQueue(workQueueId, workQueue => Succeed(workQueue.Tee(x => workQueuePatchDocument.ApplyTo(x))));
                });

        [HttpGet, Route("{workQueueId:guid}/items")]
        public async Task<Result<ItemsPageModel<WorkQueueItemNoDataXmlModel>>> WorkQueueGetQueueItems(Guid workQueueId, [FromUri] WorkQueueItemParameters workQueueItemParameters) =>
                await
                    ValidateModel()
                    .Then(() => _workQueuesService.WorkQueueGetQueueItems(
                        workQueueId, (workQueueItemParameters ?? new WorkQueueItemParameters()).ToDomainObject()))
                    .Then(x => Succeed(x.ToModelItemsPage(item => item.ToModelNoDataXml())));

        [HttpPost, Route("{workQueueId}/items")]
        [OnSuccess(HttpStatusCode.Created)]
        public async Task<Result<CreateWorkQueueItemResponseModel>> CreateWorkQueueItems(Guid workQueueId,
            [FromBody] IEnumerable<CreateWorkQueueItemModel> requestModel) =>
            await
                ValidateModel()
                    .Then(() => _workQueuesService.CreateWorkQueueItems(workQueueId, requestModel.ToDomainModel()))
                    .Then(x => Succeed(new CreateWorkQueueItemResponseModel(x)));

        [HttpGet, Route("{workQueueId}")]
        public async Task<Result<WorkQueueModel>> GetWorkQueue(Guid workQueueId) =>
            await
                _workQueuesService.GetWorkQueue(workQueueId)
                    .Then((x) => Succeed(x.ToModel()));

    }
}
