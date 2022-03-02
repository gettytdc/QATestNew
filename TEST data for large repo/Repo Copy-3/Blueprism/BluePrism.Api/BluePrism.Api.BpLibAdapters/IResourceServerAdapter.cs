namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface IResourceServerAdapter : IServerAdapter
    {
        Task<Result<ItemsPage<Resource>>> GetResourcesData(ResourceParameters resourceParameters);

        Task<Result<Resource>> GetResourceData(Guid resourceId);

        Task<Result> RetireResource(Guid resourceId);

        Task<Result> UnretireResource(Guid resourceId);
    }
}
