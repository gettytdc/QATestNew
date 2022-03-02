namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Autofac;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using AutomateAppCore.Resources;
    using BpLibAdapters;
    using ControllerClients;
    using FluentAssertions;
    using Models;
    using Moq;
    using NUnit.Framework;
    using Func;
    using AutomateAppCore.Groups;
    using BluePrism.Server.Domain.Models;
    using BpLibAdapters.Mappers;
    using Domain;
    using Domain.PagingTokens;
    using Server.Domain.Models.DataFilters;
    using Mappers;
    using ResourceAttribute = Core.Resources.ResourceAttribute;
    using BpLibResourceParameters = Server.Domain.Models.ResourceParameters;
    using ResourceDisplayStatus = Server.Domain.Models.ResourceDisplayStatus;

    [TestFixture]
    public class ResourcesControllerTests : ControllerTestBase<ResourcesControllerClient>
    {
        [SetUp]
        public override void Setup() =>
            Setup(() =>
            {
                GetMock<IServer>()
                    .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                    .Returns(new LoginResultWithReloginToken(LoginResultCode.Success));

                GetMock<IBluePrismServerFactory>()
                    .Setup(m => m.ClientInit())
                    .Returns(() => GetMock<IServer>().Object);

                RegisterMocks(builder =>
                {
                    builder.Register(_ => GetMock<IBluePrismServerFactory>().Object);
                    return builder;
                });
            });

        [Test]
        public async Task GetResources_ShouldReturnResourcesCorrectByName_WhenSuccessful()
        {
            var resources = new[]
            {
                new ResourceInfo { Name = "B" },
                new ResourceInfo { Name = "C" },
                new ResourceInfo { Name = "A" }
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Returns(resources);

            var result = await Subject.GetResources()
                .Map(x => x.Content.ReadAsAsync<ItemsPageModel<ResourceModel>>());

            var resultResourceNames = result.Items.Select(x => x.Name);
            var expectedResourceName = resources.Select(x => x.Name);

            resultResourceNames.Should().BeEquivalentTo(expectedResourceName);
        }

        [Test]
        public async Task GetResources_ShouldReturnHttpStatusCodeUnauthorized_WhenBluePrismLoginFailed()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.UnableToFindUser));

            var result = await Subject.GetResources();

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetResources_ShouldReturnHttpStatusOk_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Returns(Array.Empty<ResourceInfo>());

            var result = await Subject.GetResources();

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetResources_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetResources();

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetResources_ShouldReturnForbiddenStatusCode_WhenNoPermissionToGetResources()
        {
            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Throws<PermissionException>();

            var result = await Subject.GetResources();

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetResources_ShouldReturnResourcesWithPagingToken_WhenSuccessful()
        {
            var resources = new List<ResourceInfo>
            {
                new ResourceInfo {Name = "res1"}, new ResourceInfo {Name = "res2"}, new ResourceInfo {Name = "res3"}
            };
            var lastItem = resources.Last();
            var resourceParameters = new Models.ResourceParameters{ ItemsPerPage = 2};

            var testPagingToken = new PagingToken<string>
            {
                PreviousIdValue = lastItem.Name,
                PreviousSortColumnValue = lastItem.Name,
                DataType = lastItem.Name.GetType().Name,
                ParametersHashCode = resourceParameters.ToDomainObject().GetHashCodeForValidation()
            };

            resourceParameters.PagingToken = testPagingToken.ToString();

            var testResources = resources.Select(x => x.ToDomainObject().ToModelObject()).ToList();
            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Returns(resources);

            var result = await Subject.GetResourcesWithParameters(resourceParameters)
                .Map(x => x.Content.ReadAsAsync<ItemsPageModel<ResourceModel>>());

            result.Items.ShouldBeEquivalentTo(testResources);
            result.PagingToken.Should().Be(testPagingToken.ToString());
        }

        [Test]
        public async Task GetResources_ShouldReturnBadRequest_WhenInvalidPagingTokenProvided()
        {
            var lastItem = new Resource{ Name = "" };
            var testPagingToken = new PagingToken<string>
            {
                PreviousIdValue = lastItem.Name,
                DataType = lastItem.Name.GetType().Name,
                ParametersHashCode = "1"
            };

            var testResourceParameters = new Models.ResourceParameters
            {
                ItemsPerPage = 3,
                PagingToken = testPagingToken.ToString()
            };

            var result = await Subject.GetResourcesWithParameters(testResourceParameters);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task GetResources_ShouldReturnBadRequest_WhenItemsPerPageBelowOne(int value)
        {
            var resourceParameters = new Models.ResourceParameters {ItemsPerPage = value};

            var result = await Subject.GetResourcesWithParameters(resourceParameters);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetResources_ShouldReturnBadRequest_WhenItemsPerPageIsADecimal()
        {
            var result = await Subject.GetResourcesUsingQueryString("itemsPerPage=1.5");
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetResources_ShouldReturnBadRequestWithInvalidFields_WhenInvalidInputsSupplied()
        {
            var result = await Subject.GetResourcesUsingQueryString("itemsPerPage=1001&pagingToken=£$£$%$£^%&%NFGHDZS3214242");
            var content = await result.Map(x => x.Content.ReadAsAsync<IEnumerable<ValidationErrorModel>>());

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            content
                .Select(x => x.InvalidField)
                .Should()
                .Contain(new[] { "ItemsPerPage", "PagingToken" });
        }

        [Test]
        public async Task GetResources_ShouldPassDownSortOrder_WhenSortOrderIsValid()
        {
            await Subject.GetResourcesWithParameters(
                new Models.ResourceParameters {SortBy = Models.ResourceSortBy.GroupAsc});

            GetMock<IServer>()
                .Verify(
                    m => m.GetResourcesData(It.Is<Server.Domain.Models.ResourceParameters>(p => p.SortBy == Server.Domain.Models.ResourceSortBy.GroupNameAscending)),
                    Times.Once);
        }

        [Test]
        public async Task GetResources_ShouldDefaultToNameAscending_WhenNoSortOrderIsProvided()
        {
            await Subject.GetResourcesWithParameters(new Models.ResourceParameters());

            GetMock<IServer>()
                .Verify(
                    m => m.GetResourcesData(It.Is<Server.Domain.Models.ResourceParameters>(p => p.SortBy == Server.Domain.Models.ResourceSortBy.NameAscending)),
                    Times.Once);
        }

        [Test]
        public async Task GetResources_ShouldReturnHttpStatusBadRequest_WhenInvalidFilterSupplied()
        {
            var result = await Subject.GetResourcesUsingQueryString("displayStatus=qwer");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetResources_ShouldReturnHttpStatusOK_WhenEmptyFiltersSupplied()
        {
            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<BpLibResourceParameters>()))
                .Returns(Array.Empty<ResourceInfo>());

            var result = await Subject.GetResourcesUsingQueryString("name.strtw=&displayStatus=");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetResources_WithCommaSeparatedStatuses_AppliesAllDisplayStatusFilters()
        {
            var passedParameters = default(BpLibResourceParameters);

            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<BpLibResourceParameters>()))
                .Callback((BpLibResourceParameters x) => passedParameters = x)
                .Returns(Array.Empty<ResourceInfo>());

            var result = await Subject.GetResourcesUsingQueryString("displayStatus=working, idle, warning, offline, missing, loggedOut, private");

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            ((MultiValueDataFilter<ResourceDisplayStatus>)passedParameters.DisplayStatus)
                .OfType<EqualsDataFilter<ResourceDisplayStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[]
                {
                    Domain.ResourceDisplayStatus.Working,
                    Domain.ResourceDisplayStatus.Idle,
                    Domain.ResourceDisplayStatus.Warning,
                    Domain.ResourceDisplayStatus.Offline,
                    Domain.ResourceDisplayStatus.Missing,
                    Domain.ResourceDisplayStatus.LoggedOut,
                    Domain.ResourceDisplayStatus.Private
                });
        }

        [TestCase(ResourceAttribute.None, Models.ResourceAttribute.Retired)]
        [TestCase(ResourceAttribute.Retired, Models.ResourceAttribute.None)]
        public async Task UpdateResource_ShouldReturnHttpStatusNoContent_WhenUpdateResourceRetireStatusSuccessful(ResourceAttribute originalAttribute, Models.ResourceAttribute updateAttribute)
        {
            var resourceId = Guid.NewGuid();
            var resource = new ResourceInfo { ID = resourceId, Attributes = originalAttribute };
            var updateResource = new UpdateResourceModel { Attributes = new[] { updateAttribute } };

            GetMock<IServer>()
                .Setup(m => m.GetResourceData(It.IsAny<Guid>()))
                .Returns(resource);

            var result = await Subject.ModifyResource(resourceId, updateResource);

            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task UpdateResource_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            var resourceId = Guid.NewGuid();
            var updateResource = new UpdateResourceModel { Attributes = new[] { Models.ResourceAttribute.Retired } };

            GetMock<IServer>()
               .Setup(m => m.GetResourceData(It.IsAny<Guid>()))
               .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.ModifyResource(resourceId, updateResource);

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task UpdateResource_ShouldReturnHttpStatusNotFound_WhenResourceNotFound()
        {
            var resourceId = Guid.NewGuid();
            var updateResource = new UpdateResourceModel { Attributes = new[] { Models.ResourceAttribute.Retired } };

            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<BluePrism.Server.Domain.Models.ResourceParameters>()))
                .Returns(Array.Empty<ResourceInfo>());

            var result = await Subject.ModifyResource(resourceId, updateResource);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestCase(ResourceAttribute.Retired, Models.ResourceAttribute.Retired)]
        [TestCase(ResourceAttribute.None, Models.ResourceAttribute.None)]
        public async Task UpdateResource_ShouldReturnHttpStatusConflict_WhenResourceAttributeAlreadySet(ResourceAttribute originalAttribute, Models.ResourceAttribute updatedAttribute)
        {
            var resourceId = Guid.NewGuid();
            var resource = new ResourceInfo { ID = resourceId, Attributes = originalAttribute };
            var updateResource = new UpdateResourceModel { Attributes = new[] { updatedAttribute } };

            GetMock<IServer>()
                .Setup(m => m.GetResourceData(It.IsAny<Guid>()))
                .Returns(resource);

            var result = await Subject.ModifyResource(resourceId, updateResource);

            result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task UpdateResource_ShouldReturnHttpStatusForbidden_WhenInsufficientPermissionToMoveResourceToDefaultGroupOnUnretire()
        {
            var resourceId = Guid.NewGuid();
            var resource = new ResourceInfo { ID = resourceId, Attributes = ResourceAttribute.Retired };
            var updateResource = new UpdateResourceModel { Attributes = new[] {Models.ResourceAttribute.None} };

            GetMock<IServer>()
                .Setup(m => m.GetResourceData(It.IsAny<Guid>()))
                .Returns(resource);

            GetMock<IServer>()
                .Setup(m => m.AddToGroup(It.IsAny<GroupTreeType>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<GroupMember>>()))
                .Throws<PermissionException>();

            var result = await Subject.ModifyResource(resourceId, updateResource);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestCase("invalid")]
        [TestCase("123")]
        [TestCase("2021-04-30T12:34:56Z")]
        public async Task UpdateResource_ShouldReturnHttpStatusBadRequest_WhenInvalidResourceIdIsPassed(string resourceIdValue)
        {
            var result = await Subject.ModifyResource(resourceIdValue, new UpdateResourceModel { Attributes = new[] { Models.ResourceAttribute.Retired }});

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
