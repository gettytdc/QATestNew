namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using Domain;
    using Domain.Errors;
    using Func;
    using Mappers;

    using static Func.ResultHelper;
    using static ServerResultTask;

    using ServerModels = Server.Domain.Models;

    public class ResourceServerAdapter : IResourceServerAdapter
    {
        private readonly IServer _server;

        public ResourceServerAdapter(IServer server) =>
            _server = server;

        public Task<Result<ItemsPage<Resource>>> GetResourcesData(ResourceParameters resourceParameters) =>
            RunOnServer(() => _server.GetResourcesData(resourceParameters.ToBluePrismObject())
                    .Select(x => x.ToDomainObject()).ToArray()
                    .Map(x => x.ToItemsPage(resourceParameters)))
                .Catch<ServerModels.PermissionException>(ex => ResultHelper<ItemsPage<Resource>>.Fail(new PermissionError(ex.Message)));

        public Task<Result<Resource>> GetResourceData(Guid resourceId) =>
            RunOnServer(() => _server.GetResourceData(resourceId)?.ToDomainObject())
                .Catch<ServerModels.PermissionException>(ex => ResultHelper<Resource>.Fail(new PermissionError(ex.Message)))
                .OnNull(() => ResultHelper<Resource>.Fail(new ResourceNotFoundError()));

        public Task<Result> RetireResource(Guid resourceId) =>
            RunOnServer(() => _server.RetireResource(resourceId))
                .Catch<ServerModels.PermissionException>(ex => Fail(new PermissionError(ex.Message)))
                .Catch<ServerModels.InvalidStateException>(ex => Fail(new CannotRetireResourceError(ex.Message)));

        public Task<Result> UnretireResource(Guid resourceId) =>
            RunOnServer(() => _server.UnretireResource(resourceId))
                .Catch<ServerModels.LicenseRestrictionException>(ex => Fail(new LicenseRestrictionError(ex.Message)));
    }
}
