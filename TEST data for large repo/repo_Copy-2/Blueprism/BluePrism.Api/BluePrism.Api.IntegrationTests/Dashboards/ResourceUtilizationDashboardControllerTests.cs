namespace BluePrism.Api.IntegrationTests.Dashboards
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Autofac;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BpLibAdapters;
    using Common.Security;
    using ControllerClients;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Server.Domain.Models;
    using Server.Domain.Models.Dashboard;
    using ResourceUtilizationParameters = Server.Domain.Models.ResourceUtilizationParameters;

    public class ResourceUtilizationDashboardControllerTests : ControllerTestBase<DashboardsControllerClient>
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
        public async Task GetResourceUtilization_OnSuccess_ReturnsOkStatusCode()
        {
            var resourceUtilizationParameters = new Models.ResourceUtilizationParameters()
            {
                StartDate = new DateTime(2021, 2, 1),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                .Returns(new List<ResourceUtilization>());

            var result = await Subject.GetResourceUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetResourceUtilization_OnSuccess_ReturnsExpectedResourceUtilizationResponseWithNoPaging()
        {
            var resourceUtilizationParameters = new Models.ResourceUtilizationParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                ResourceIds = new Guid[] { }
            };

            var expectedResourceUtilization = new List<ResourceUtilization>()
            {
                new ResourceUtilization()
                {
                    Usages = Enumerable.Range(1, 24).ToArray(),
                    DigitalWorkerName = "worker1",
                    ResourceId = Guid.NewGuid(),
                    UtilizationDate = DateTime.Now.ToUniversalTime()
                },
                new ResourceUtilization()
                {
                    Usages = Enumerable.Range(1, 24).ToArray(),
                    DigitalWorkerName = "worker2",
                    ResourceId = Guid.NewGuid(),
                    UtilizationDate = DateTime.Now.ToUniversalTime()
                },
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                .Returns(expectedResourceUtilization);

            var result = await Subject.GetResourceUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultContent = JsonConvert.DeserializeObject<List<Api.Models.Dashboard.ResourceUtilization>>(await result.Content.ReadAsStringAsync());

            resultContent.ShouldAllBeEquivalentTo(expectedResourceUtilization);
        }

        [Test]
        public async Task GetResourceUtilization_OnSuccess_ReturnsExpectedResourceUtilizationResponseWithPaging()
        {
            var resourceUtilizationParameters = new Models.ResourceUtilizationParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                ResourceIds = new Guid[] { },
                PageNumber = 1,
                PageSize = 2
            };

            var expectedResourceUtilization = new List<ResourceUtilization>()
            {
                new ResourceUtilization()
                {
                    Usages = Enumerable.Range(1, 24).ToArray(),
                    DigitalWorkerName = "worker1",
                    ResourceId = Guid.NewGuid(),
                    UtilizationDate = DateTime.Now.ToUniversalTime()
                },
                new ResourceUtilization()
                {
                    Usages = Enumerable.Range(1, 24).ToArray(),
                    DigitalWorkerName = "worker2",
                    ResourceId = Guid.NewGuid(),
                    UtilizationDate = DateTime.Now.ToUniversalTime()
                },
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                .Returns(expectedResourceUtilization);

            var result = await Subject.GetResourceUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultContent = JsonConvert.DeserializeObject<List<Api.Models.Dashboard.ResourceUtilization>>(await result.Content.ReadAsStringAsync());

            resultContent.ShouldAllBeEquivalentTo(expectedResourceUtilization);
        }

        [Test]
        public async Task GetResourceUtilization_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            var resourceUtilizationParameters = new Models.ResourceUtilizationParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetResourceUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetResourceUtilization_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            var resourceUtilizationParameters = new Models.ResourceUtilizationParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                 .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetResourceUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetResourceUtilization_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            var resourceUtilizationParameters = new Models.ResourceUtilizationParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(x => x.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetResourceUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestCase(null)]
        [TestCase("startDate=")]
        [TestCase("startDate=01-01-2021&resourceIds=not-valid")]
        [TestCase("startDate=01-01-2021&pageNumber=not-valid")]
        [TestCase("startDate=01-01-2021&pageSize=not-valid")]
        public async Task GetResourceUtilization_OnInvalidParameterSupplied_ReturnsBadRequestStatusCode(string queryString)
        {
            var result = await Subject.GetResourceUtilization(queryString);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
