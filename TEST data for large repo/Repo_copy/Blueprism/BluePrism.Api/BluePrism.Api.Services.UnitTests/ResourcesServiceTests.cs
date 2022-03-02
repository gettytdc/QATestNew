namespace BluePrism.Api.Services.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Autofac;
    using BpLibAdapters;
    using Domain;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;
    using Domain.Groups;

    using static Func.ResultHelper;

    [TestFixture]
    public class ResourcesServiceTests : UnitTestBase<ResourcesService>
    {
        public override void Setup() =>
            base.Setup(builder =>
            {
                builder.RegisterGeneric(typeof(MockAdapterAuthenticatedMethodRunner<>)).As(typeof(IAdapterAuthenticatedMethodRunner<>));
            });

        [Test]
        public async Task GetResources_ShouldReturnSuccess_WhenSuccessful()
        {
            var resourcesPage = new ItemsPage<Resource> {Items = new List<Resource> {new Resource {Id = new Guid()}}};

            GetMock<IResourceServerAdapter>()
                .Setup(x => x.GetResourcesData(It.IsAny<ResourceParameters>()))
                .ReturnsAsync(Succeed(resourcesPage));

            var result = await ClassUnderTest.GetResources(new ResourceParameters());

            result.Should().BeAssignableTo<Success>();
            result.OnSuccess(x => x.ShouldBeEquivalentTo(resourcesPage));
        }

        [Test]
        public async Task GetResources_OnPermissionError_ReturnsPermissionError()
        {
            GetMock<IResourceServerAdapter>()
                .Setup(x => x.GetResourcesData(It.IsAny<ResourceParameters>()))
                .ReturnsAsync(ResultHelper<ItemsPage<Resource>>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetResources(new ResourceParameters());

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task GetResource_ShouldReturnSuccess_WhenSuccessful()
        {
            var resource = new Resource { Id = new Guid() };

            GetMock<IResourceServerAdapter>()
                .Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                .ReturnsAsync(Succeed(resource));

            var result = await ClassUnderTest.GetResource(Guid.NewGuid());

            result.Should().BeAssignableTo<Success>();
            result.OnSuccess(x => x.ShouldBeEquivalentTo(resource));
        }

        [Test]
        public async Task GetResource_OnPermissionError_ReturnsPermissionError()
        {
            GetMock<IResourceServerAdapter>()
                .Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                .ReturnsAsync(ResultHelper<Resource>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetResource(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task ModifyResource_ShouldReturnSuccess_WhenUnretireResourceSuccessful()
        {
            var resourceId = Guid.NewGuid();
            var retiredResource = new Resource { Id = resourceId, Attributes = ResourceAttribute.Retired };
            var resourceChanges = new Resource { Id = resourceId, Attributes = ResourceAttribute.None };
            var id = Guid.NewGuid();

            GetMock<IResourceServerAdapter>()
                .Tee(adapter => adapter.Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                    .ReturnsAsync(Succeed(retiredResource)))
                .Tee(adapter => adapter.Setup(x => x.UnretireResource(resourceId))
                    .ReturnsAsync(Succeed));

            GetMock<IGroupsServerAdapter>()
                .Tee(adapter => adapter.Setup(x => x.GetDefaultGroupId(It.IsAny<GroupTreeType>()))
                    .ReturnsAsync(Succeed(id)))
                .Tee(adapter => adapter.Setup(x => x.AddToGroup(GroupTreeType.Resources, id, It.IsAny<IEnumerable<GroupMember>>()))
                    .ReturnsAsync(Succeed));

            var result = await ClassUnderTest.ModifyResource(resourceId, resourceChanges);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task ModifyResource_ShouldMoveResourceToDefaultGroup_WhenUnretiringResourceSuccessfully()
        {
            var resourceId = Guid.NewGuid();
            var retiredResource = new Resource { Id = resourceId, Attributes = ResourceAttribute.Retired };
            var resourceChanges = new Resource { Id = resourceId, Attributes = ResourceAttribute.None };
            var id = Guid.NewGuid();

            var groupServerAdapter = GetMock<IGroupsServerAdapter>();
            groupServerAdapter
                .Tee(adapter => adapter.Setup(x => x.GetDefaultGroupId(It.IsAny<GroupTreeType>()))
                    .ReturnsAsync(Succeed(id))
                    .Verifiable())
                .Tee(adapter => adapter.Setup(x => x.GetDefaultGroupId(It.IsAny<GroupTreeType>()))
                    .ReturnsAsync(Succeed(id))
                    .Verifiable());

            GetMock<IResourceServerAdapter>()
                .Tee(adapter => adapter.Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                    .ReturnsAsync(Succeed(retiredResource)))
                .Tee(adapter => adapter.Setup(x => x.UnretireResource(resourceId))
                    .ReturnsAsync(Succeed));

            await ClassUnderTest.ModifyResource(resourceId, resourceChanges);

            groupServerAdapter.Verify(x => x.GetDefaultGroupId(GroupTreeType.Resources), Times.Once);
            groupServerAdapter.Verify(x => x.AddToGroup(GroupTreeType.Resources, id, It.IsAny<IEnumerable<GroupMember>>()), Times.Once);
        }

        [Test]
        public async Task ModifyResource_ShouldReturnSuccess_WhenRetireResourceSuccessful()
        {
            var resourceId = Guid.NewGuid();
            var originalResource = new Resource { Id = resourceId, Attributes = ResourceAttribute.None };
            var resourceChanges = new Resource { Id = resourceId, Attributes = ResourceAttribute.Retired };

            GetMock<IResourceServerAdapter>()
                .Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                .ReturnsAsync(Succeed(originalResource));
            GetMock<IResourceServerAdapter>()
                .Setup(x => x.RetireResource(resourceId))
                .ReturnsAsync(Succeed);

            var result = await ClassUnderTest.ModifyResource(resourceId, resourceChanges);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task ModifyResource_ShouldReturnFailure_WhenResourceAlreadyRetiredErrorThrown()
        {
            var resourceId = Guid.NewGuid();
            var retiredResource = new Resource { Id = resourceId, Attributes = ResourceAttribute.Retired };
            var resourceChanges = new Resource { Id = resourceId, Attributes = ResourceAttribute.Retired };

            GetMock<IResourceServerAdapter>()
                .Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                .ReturnsAsync(Succeed(retiredResource));

            var result = await ClassUnderTest.ModifyResource(resourceId, resourceChanges);

            (result is Failure<ResourceAlreadyRetiredError>).Should().BeTrue();
        }

        [Test]
        public async Task ModifyResource_ShouldReturnFailure_WhenResourceNotRetiredErrorThrown()
        {
            var resourceId = Guid.NewGuid();
            var originalResource = new Resource { Id = resourceId, Attributes = ResourceAttribute.None };
            var resourceChanges = new Resource { Id = resourceId, Attributes = ResourceAttribute.None };

            GetMock<IResourceServerAdapter>()
                .Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                .ReturnsAsync(Succeed(originalResource));

            var result = await ClassUnderTest.ModifyResource(resourceId, resourceChanges);

            (result is Failure<ResourceNotRetiredError>).Should().BeTrue();
        }

        [Test]
        public async Task ModifyResource_ShouldReturnFailure_WhenResourceNotFoundErrorThrown()
        {
            var resourceId = Guid.NewGuid();
            var originalResource = new Resource { Id = Guid.NewGuid() };
            var resourceChanges = new Resource { Id = resourceId };

            GetMock<IResourceServerAdapter>()
                .Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                .ReturnsAsync(ResultHelper<Resource>.Fail(new ResourceNotFoundError()));

            var result = await ClassUnderTest.ModifyResource(resourceId, resourceChanges);

            (result is Failure<ResourceNotFoundError>).Should().BeTrue();
        }

        [Test]
        public async Task ModifyResource_ShouldReturnFailure_WhenPermissionErrorCaughtOnUnretire()
        {
            var resourceId = Guid.NewGuid();
            var retiredResource = new Resource { Id = resourceId, Attributes = ResourceAttribute.Retired };
            var resourceChanges = new Resource { Id = resourceId, Attributes = ResourceAttribute.None };
            var id = Guid.NewGuid();

            GetMock<IResourceServerAdapter>()
                .Tee(adapter => adapter.Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                    .ReturnsAsync(Succeed(retiredResource)))
                .Tee(adapter => adapter.Setup(x => x.UnretireResource(resourceId))
                    .ReturnsAsync(Succeed));

            GetMock<IGroupsServerAdapter>()
                .Tee(adapter => adapter.Setup(x => x.GetDefaultGroupId(It.IsAny<GroupTreeType>()))
                    .ReturnsAsync(Succeed(id)))
                .Tee(adapter => adapter.Setup(x => x.AddToGroup(GroupTreeType.Resources, id, It.IsAny<IEnumerable<GroupMember>>()))
                    .ReturnsAsync(Fail(new PermissionError("error"))));

            var result = await ClassUnderTest.ModifyResource(resourceId, resourceChanges);

            (result is Failure<PermissionError>).Should().BeTrue();
        }
    }
}
