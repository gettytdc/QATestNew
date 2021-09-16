namespace BluePrism.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Func;
    using Mappers;
    using Models;
    using Services;
    using System.Net;
    using Func.AspNet;

    using static Func.ResultHelper;

    [RoutePrefix("resources")]
    public class ResourcesController : ResultControllerBase
    {
        private readonly IResourcesService _resourcesService;

        public ResourcesController(IResourcesService resourcesService) =>
            _resourcesService = resourcesService;

        [HttpGet, Route("")]
        public async Task<Result<ItemsPageModel<ResourceModel>>> GetResources([FromUri] ResourceParameters resourceParameters) =>
            await ValidateModel()
                .Then(() => Succeed((resourceParameters ?? new ResourceParameters()).ToDomainObject()))
                .Then(x => _resourcesService.GetResources(x))
                .Then(x => Succeed(x.ToModelItemsPage(y => y.ToModelObject())));

        [HttpPut, Route("{resourceId}")]
        [OnSuccess(HttpStatusCode.NoContent)]
        public async Task<Result> UpdateResource([FromUri] Guid resourceId, [FromBody] UpdateResourceModel resourceChanges) =>
            await _resourcesService.ModifyResource(resourceId, resourceChanges.ToDomain());
    }
}
