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
    using Models.Dashboard;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Server.Domain.Models;
    using UtilizationHeatMap = Server.Domain.Models.Dashboard.UtilizationHeatMap;
    using UtilizationHeatMapParameters = Models.UtilizationHeatMapParameters;

    public class UtilizationHeatMapDashboardControllerTests : ControllerTestBase<DashboardsControllerClient>
    {
        [SetUp]
        public override void Setup() =>
            Setup(() =>
            {
                GetMock<IServer>()
                    .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<SafeString>(), It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new LoginResult(LoginResultCode.Success));

                GetMock<IBluePrismServerFactory>()
                    .Setup(m => m.ClientInit(It.IsAny<ConnectionSettingProperties>()))
                    .Returns(() => GetMock<IServer>().Object);

                RegisterMocks(builder =>
                {
                    builder.Register(_ => GetMock<IBluePrismServerFactory>().Object);
                    return builder;
                });
            });

        [Test]
        public async Task GetUtilizationHeatMap_OnSuccess_ReturnsOkStatusCode()
        {
            var utilizationHeatMapParameters = new UtilizationHeatMapParameters()
            {
                StartDate = new DateTime(2021, 2, 1),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(m => m.GetUtilizationHeatMap(It.IsAny<Server.Domain.Models.UtilizationHeatMapParameters>()))
                .Returns(new List<UtilizationHeatMap>());

            var result = await Subject.GetUtilizationHeatMap(utilizationHeatMapParameters);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetUtilizationHeatMap_OnSuccess_ReturnsExpectedUtilizationHeatMapResponse()
        {
            var utilizationHeatMapParameters = new UtilizationHeatMapParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                ResourceIds = new Guid[] { }
            };

            var expectedUtilizationHeatMaps = new List<UtilizationHeatMap>()
            {
                new UtilizationHeatMap()
                {
                    Usage = 1,
                    VirtualWorkerName = "worker1",
                    ResourceId = Guid.NewGuid(),
                    Dates = DateTime.UtcNow
                },
                new UtilizationHeatMap()
                {
                    Usage = 2,
                    VirtualWorkerName = "worker2",
                    ResourceId = Guid.NewGuid(),
                    Dates = DateTime.UtcNow
                },
            };

            GetMock<IServer>()
                .Setup(m => m.GetUtilizationHeatMap(It.IsAny<Server.Domain.Models.UtilizationHeatMapParameters>()))
                .Returns(expectedUtilizationHeatMaps);

            var result = await Subject.GetUtilizationHeatMap(utilizationHeatMapParameters);

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultContent = JsonConvert.DeserializeObject<List<Api.Models.Dashboard.UtilizationHeatMap>>(await result.Content.ReadAsStringAsync());

            resultContent.ShouldAllBeEquivalentTo(expectedUtilizationHeatMaps);
        }

        [Test]
        public async Task GetUtilizationHeatMap_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            var utilizationHeatMapParameters = new UtilizationHeatMapParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<SafeString>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new LoginResult(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetUtilizationHeatMap(utilizationHeatMapParameters);

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetUtilizationHeatMap_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            var utilizationHeatMapParameters = new UtilizationHeatMapParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(m => m.GetUtilizationHeatMap(It.IsAny<Server.Domain.Models.UtilizationHeatMapParameters>()))
                 .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetUtilizationHeatMap(utilizationHeatMapParameters);

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetUtilizationHeatMap_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            var utilizationHeatMapParameters = new UtilizationHeatMapParameters()
            {
                StartDate = new DateTime(2021, 1, 1),
                ResourceIds = new Guid[] { }
            };

            GetMock<IServer>()
                .Setup(x => x.GetUtilizationHeatMap(It.IsAny<Server.Domain.Models.UtilizationHeatMapParameters>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetUtilizationHeatMap(utilizationHeatMapParameters);

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestCase(null)]
        [TestCase("startDate=")]
        [TestCase("startDate=01-01-2021&resourceIds=not-valid")]
        public async Task GetUtilizationHeatMap_OnInvalidParameterSupplied_ReturnsBadRequestStatusCode(string queryString)
        {
            var result = await Subject.GetUtilizationHeatMap(queryString);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
