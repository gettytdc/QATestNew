namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using AutomateAppCore.Resources;
    using BluePrism.Api.Domain.Filters;
    using BluePrism.Server.Domain.Models;
    using BpLibAdapters.Mappers;
    using BpLibAdapters.Mappers.FilterMappers;
    using Domain;
    using Domain.Errors;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;
    using ResourceParameters = Domain.ResourceParameters;

    [TestFixture]
    public class ResourcesServerAdapterTests : UnitTestBase<ResourceServerAdapter>
    {
        [SetUp]
        public void SetUp() =>
            FilterMapper.SetFilterMappers(new IFilterMapper[]
            {
                new NullFilterMapper(),
                new StringStartsWithFilterMapper(),
                new GreaterThanOrEqualToFilterMapper(),
                new MultiValueFilterMapper(),
                new EqualsFilterMapper()
            });

        [Test]
        public async Task GetResources_ShouldReturnSuccess_WhenSuccessful()
        {
            var resources = new[] { new ResourceInfo() };

            GetMock<IServer>()
                .Setup(x => x.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Returns(resources);

            var resourceParameters = GetTestResourceParameters();
            var result = await ClassUnderTest.GetResourcesData(resourceParameters);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetResources_ReturnsPermissionError_WhenPermissionError()
        {
            GetMock<IServer>()
                .Setup(x => x.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Throws<PermissionException>();

            var resourceParameters = GetTestResourceParameters();
            var result = await ClassUnderTest.GetResourcesData(resourceParameters);

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }

        [Test]
        public async Task GetResources_ReturnsEmptyCollection_WhenNoResources()
        {
            var resources = new List<ResourceInfo>();

            GetMock<IServer>()
                .Setup(x => x.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Returns(resources);

            var resourceParameters = GetTestResourceParameters();
            var result = await ClassUnderTest.GetResourcesData(resourceParameters);

            ((Success<ItemsPage<Resource>>)result).Value.Items.Should().BeEmpty();
        }

        [Test]
        public async Task GetResources_OnSuccess_ReturnsExpectedResource()
        {
            var resourceId = Guid.NewGuid();
            var poolId = Guid.NewGuid();
            var groupId = Guid.NewGuid();

            var resources = new[] { new ResourceInfo
            {
                ID = resourceId,
                PoolName = "TestGroup",
                Pool = poolId,
                GroupID = groupId,
                GroupName = "TestGroup",
                DisplayStatus = ResourceStatus.Working,
                Name = "TestResource",
                ActiveSessions = 1234,
                PendingSessions = 12345,
                WarningSessions = 123
            }};

            var expectedResource = new Resource
            {
                Id = resourceId,
                PoolName = "TestGroup",
                PoolId = poolId,
                GroupId = groupId,
                GroupName = "TestGroup",
                DisplayStatus = Domain.ResourceDisplayStatus.Working,
                Name = "TestResource",
                ActiveSessionCount = 1234,
                PendingSessionCount = 12345,
                WarningSessionCount = 123
            };

            GetMock<IServer>()
                .Setup(x => x.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Returns(resources);

            var resourceParameters = GetTestResourceParameters();
            var result = await ClassUnderTest.GetResourcesData(resourceParameters);
            var resultItem = ((Success<ItemsPage<Resource>>)result).Value.Items.First();
            resultItem.ShouldBeEquivalentTo(expectedResource);
        }

        [Test]
        public async Task GetResources_OnSuccess_ReturnsExpectedCountOfResources()
        {
            var resourceId = Guid.NewGuid();
            var poolId = Guid.NewGuid();
            var groupId = Guid.NewGuid();

            var resources = new[] {
                new ResourceInfo
                {
                    ID = resourceId,
                    PoolName = "TestGroup",
                    Pool = poolId,
                    GroupID = groupId,
                    GroupName = "TestGroup",
                    DisplayStatus = ResourceStatus.Working,
                    Name = "TestResource",
                    ActiveSessions = 1234,
                    PendingSessions = 12345,
                    WarningSessions = 123
                },
                new ResourceInfo
                {
                    ID = resourceId,
                    PoolName = "TestGroup",
                    Pool = poolId,
                    GroupID = groupId,
                    GroupName = "TestGroup",
                    DisplayStatus = ResourceStatus.Working,
                    Name = "TestResource",
                    ActiveSessions = 1234,
                    PendingSessions = 12345,
                    WarningSessions = 123
                }
            };

            var expectedResources = new[] {
                new Resource
                {
                    Id = resourceId,
                    PoolName = "TestGroup",
                    PoolId = poolId,
                    GroupId = groupId,
                    GroupName = "TestGroup",
                    DisplayStatus = Domain.ResourceDisplayStatus.Working,
                    Name = "TestResource",
                    ActiveSessionCount = 1234,
                    PendingSessionCount = 12345,
                    WarningSessionCount = 123
                },
                new Resource
                {
                    Id = resourceId,
                    PoolName = "TestGroup",
                    PoolId = poolId,
                    GroupId = groupId,
                    GroupName = "TestGroup",
                    DisplayStatus = Domain.ResourceDisplayStatus.Working,
                    Name = "TestResource",
                    ActiveSessionCount = 1234,
                    PendingSessionCount = 12345,
                    WarningSessionCount = 123
                }
            };

            var resourceParameters = GetTestResourceParameters();

            GetMock<IServer>()
                .Setup(x => x.GetResourcesData(resourceParameters.ToBluePrismObject()))
                .Returns(resources);

            var result = await ClassUnderTest.GetResourcesData(resourceParameters);
            var resourcesCount = ((Success<ItemsPage<Resource>>)result).Value.Items.Count();
            resourcesCount.Should().Be(expectedResources.Length);
        }

        [Test]
        public async Task GetResource_ShouldReturnSuccess_WhenSuccessful()
        {
            var resource = new ResourceInfo();

            GetMock<IServer>()
                .Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                .Returns(resource);

            var result = await ClassUnderTest.GetResourceData(Guid.NewGuid());

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetResource_OnSuccess_ReturnsExpectedResource()
        {
            var resourceId = Guid.NewGuid();
            var poolId = Guid.NewGuid();
            var groupId = Guid.NewGuid();

            var resource = new ResourceInfo
            {
                ID = resourceId,
                PoolName = "TestGroup",
                Pool = poolId,
                GroupID = groupId,
                GroupName = "TestGroup",
                DisplayStatus = ResourceStatus.Working,
                Name = "TestResource",
                ActiveSessions = 1234,
                PendingSessions = 12345,
                WarningSessions = 123
            };

            var expectedResource = new Resource
            {
                Id = resourceId,
                PoolName = "TestGroup",
                PoolId = poolId,
                GroupId = groupId,
                GroupName = "TestGroup",
                DisplayStatus = Domain.ResourceDisplayStatus.Working,
                Name = "TestResource",
                ActiveSessionCount = 1234,
                PendingSessionCount = 12345,
                WarningSessionCount = 123
            };

            GetMock<IServer>()
                .Setup(x => x.GetResourceData(It.IsAny<Guid>()))
                .Returns(resource);

            var result = await ClassUnderTest.GetResourceData(Guid.NewGuid());
            var resultItem = ((Success<Resource>)result).Value;
            resultItem.ShouldBeEquivalentTo(expectedResource);
        }

        [Test]
        public async Task GetResource_ReturnsNotFoundError_WhenResourceNotFound()
        {
            var result = await ClassUnderTest.GetResourceData(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<ResourceNotFoundError>>();
        }

        private ResourceParameters GetTestResourceParameters() =>
            new ResourceParameters
            {
                ItemsPerPage = 5,
                Name = new NullFilter<string>(),
                GroupName = new NullFilter<string>(),
                PoolName = new NullFilter<string>(),
                ActiveSessionCount = new NullFilter<int>(),
                PendingSessionCount = new NullFilter<int>(),
                DisplayStatus = new NullFilter<Domain.ResourceDisplayStatus>(),
                PagingToken = OptionHelper.None<PagingToken<string>>()
            };

        [Test]
        public async Task RetireResource_ShouldReturnSuccess_WhenSuccessful()
        {
            var result = await ClassUnderTest.RetireResource(Guid.NewGuid());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task RetireResource_ShouldReturnFailure_WhenPermissionExceptionThrown()
        {
            var resourceId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.RetireResource(resourceId))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.RetireResource(resourceId);

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task RetireResource_ShouldReturnFailure_WhenInvalidStateExceptionThrown()
        {
            var resourceId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.RetireResource(resourceId))
                .Throws<InvalidStateException>();

            var result = await ClassUnderTest.RetireResource(resourceId);

            (result is Failure<CannotRetireResourceError>).Should().BeTrue();
        }

        [Test]
        public async Task UnretireResource_ShouldReturnSuccess_WhenSuccessful()
        {
            var result = await ClassUnderTest.UnretireResource(Guid.NewGuid());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task UnretireResource_ShouldReturnFailure_WhenLicenseRestrictionExceptionThrown()
        {
            var resourceId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.UnretireResource(resourceId))
                .Throws<LicenseRestrictionException>();

            var result = await ClassUnderTest.UnretireResource(resourceId);

            (result is Failure<LicenseRestrictionError>).Should().BeTrue();
        }
    }
}
