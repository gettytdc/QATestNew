namespace BluePrism.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Domain;
    using Domain.Errors;
    using Domain.Groups;
    using Func;
    using Logging;

    using static Func.ResultHelper;

    public class ResourcesService : IResourcesService
    {
        private readonly IAdapterAuthenticatedMethodRunner<IResourceServerAdapter> _resourceMethodRunner;
        private readonly IAdapterAuthenticatedMethodRunner<IGroupsServerAdapter> _groupMethodRunner;
        private readonly ILogger<ResourcesService> _logger;

        public ResourcesService(IAdapterAuthenticatedMethodRunner<IResourceServerAdapter> resourceMethodRunner,
            IAdapterAuthenticatedMethodRunner<IGroupsServerAdapter> groupMethodRunner,
            ILogger<ResourcesService> logger)
        {
            _resourceMethodRunner = resourceMethodRunner;
            _groupMethodRunner = groupMethodRunner;
            _logger = logger;
        }

        public Task<Result<ItemsPage<Resource>>> GetResources(ResourceParameters resourceParameters) =>
            _resourceMethodRunner.ExecuteForUser(x => x.GetResourcesData(resourceParameters))
                .OnSuccess(() => _logger.Debug("Sucessfully retrieved resources"))
                .OnError((PermissionError _) => _logger.Info("Attempted to get resources"));

        public Task<Result<Resource>> GetResource(Guid resourceId) =>
            _resourceMethodRunner.ExecuteForUser(x => x.GetResourceData(resourceId))
                .OnSuccess(() => _logger.Debug("Sucessfully retrieved a resource"))
                .OnError((PermissionError _) => _logger.Info("Attempted to get a resource"));

        public Task<Result> ModifyResource(Guid resourceId, Resource resourceChanges) =>
            GetResource(resourceId)
                .Then(ApplyRetireChanges(resourceChanges));

        private Func<Resource, Task<Result>> ApplyRetireChanges(Resource resourceChanges) =>
            originalResource =>
            {
                if (CanRetireResource(resourceChanges, originalResource))
                    return RetireResource(originalResource.Id);

                if (CanUnretireResource(resourceChanges, originalResource))
                    return UnretireResource(originalResource.Id)
                        .Then(MoveToDefaultGroup(originalResource.Id));

                return (IsRetired(resourceChanges)
                    ? Fail<ResourceAlreadyRetiredError>()
                    : Fail<ResourceNotRetiredError>()).ToTask();
            };

        private static bool CanRetireResource(Resource resourceChanges, Resource originalResource) =>
            !IsRetired(originalResource) && IsRetired(resourceChanges);

        private static bool CanUnretireResource(Resource resourceChanges, Resource originalResource) =>
            IsRetired(originalResource) && !IsRetired(resourceChanges);

        private Task<Result> RetireResource(Guid resourceId) =>
            _resourceMethodRunner.ExecuteForUser(x => x.RetireResource(resourceId));

        private Task<Result> UnretireResource(Guid resourceId) =>
            _resourceMethodRunner.ExecuteForUser(x => x.UnretireResource(resourceId));

        private static bool IsRetired(Resource resource) =>
            (resource.Attributes & ResourceAttribute.Retired) != 0;

        private Func<Task<Result>> MoveToDefaultGroup(Guid resourceId) => () =>
            GetDefaultGroupId()
                .Then(AddToGroup(resourceId));

        private Task<Result<Guid>> GetDefaultGroupId() =>
            _groupMethodRunner.ExecuteForUser(x => x.GetDefaultGroupId(GroupTreeType.Resources));

        private Func<Guid, Task<Result>> AddToGroup(Guid resourceId) => groupId =>
            _groupMethodRunner.ExecuteForUser(x =>
                x.AddToGroup(GroupTreeType.Resources, groupId, new[] { new ResourceGroupMember { Id = resourceId } }))
                .OnError<PermissionError>(_ => _logger.Warn("Attempting to move resource {0} to default group after unretiring, but insufficient permissions to do so."));
    }
}
