namespace BluePrism.Api.IntegrationTests.Dashboards
{
    using System;
    using System.Collections.Generic;
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

    public class ResourcesSummaryUtilizationDashboardControllerTests : ControllerTestBase<DashboardsControllerClient>
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
        public async Task GetResourcesSummaryUtilization_OnSuccess_ReturnsOkStatusCode()
        {
            var resourceUtilizationParameters = new Models.ResourcesSummaryUtilizationParameters()
            {
                StartDate = new DateTime(2021, 2, 1),
                EndDate = new DateTime(2021, 2, 2),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourcesSummaryUtilization(It.IsAny<ResourcesSummaryUtilizationParameters>()))
                .Returns(new List<ResourcesSummaryUtilization>());

            var result = await Subject.GetResourcesSummaryUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetResourcesSummaryUtilization_OnSuccess_ReturnsExpectedResourceUtilizationResponseWithNoPaging()
        {
            var resourceUtilizationParameters = new Models.ResourcesSummaryUtilizationParameters()
            {
                StartDate = new DateTime(2021, 2, 1),
                EndDate = new DateTime(2021, 2, 2),
                ResourceIds = new Guid[] { }
            };

            var expectedResourceUtilization = new List<ResourcesSummaryUtilization>()
            {
                new ResourcesSummaryUtilization()
                {
                    Usage = 1,
                    Dates = DateTime.Now.ToUniversalTime()
                },
                new ResourcesSummaryUtilization()
                {
                    Usage = 2,
                    Dates = DateTime.Now.ToUniversalTime()
                },
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourcesSummaryUtilization(It.IsAny<ResourcesSummaryUtilizationParameters>()))
                .Returns(expectedResourceUtilization);

            var result = await Subject.GetResourcesSummaryUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultContent = JsonConvert.DeserializeObject<List<Api.Models.Dashboard.ResourcesSummaryUtilization>>(await result.Content.ReadAsStringAsync());

            resultContent.ShouldAllBeEquivalentTo(expectedResourceUtilization);
        }

        [Test]
        public async Task GetResourcesSummaryUtilization_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            var resourceUtilizationParameters = new Models.ResourcesSummaryUtilizationParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                EndDate = new DateTime(2021, 1, 2),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetResourcesSummaryUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetResourcesSummaryUtilization_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            var resourceUtilizationParameters = new Models.ResourcesSummaryUtilizationParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                EndDate = new DateTime(2021, 1, 2),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourcesSummaryUtilization(It.IsAny<ResourcesSummaryUtilizationParameters>()))
                 .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetResourcesSummaryUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetResourcesSummaryUtilization_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            var resourceUtilizationParameters = new Models.ResourcesSummaryUtilizationParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                EndDate = new DateTime(2021, 1, 2),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(x => x.GetResourcesSummaryUtilization(It.IsAny<ResourcesSummaryUtilizationParameters>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetResourcesSummaryUtilization(resourceUtilizationParameters);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestCase(null)]
        [TestCase("startDate=")]
        [TestCase("endDate=")]
        [TestCase("startDate=01-01-2021&resourceIds=not-valid")]
        [TestCase("startDate=01-01-2021")]
        [TestCase("startDate=01-01-2021=not-valid")]
        public async Task GetResourcesSummaryUtilization_OnInvalidParameterSupplied_ReturnsBadRequestStatusCode(string queryString)
        {
            var result = await Subject.GetResourcesSummaryUtilization(queryString);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
