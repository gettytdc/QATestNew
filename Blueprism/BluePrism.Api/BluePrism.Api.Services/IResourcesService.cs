namespace BluePrism.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface IResourcesService
    {
        Task<Result<ItemsPage<Resource>>> GetResources(ResourceParameters resourceParameters);
        Task<Result<Resource>> GetResource(Guid resourceId);
        Task<Result> ModifyResource(Guid resourceId, Resource resourceChanges);
    }
}
