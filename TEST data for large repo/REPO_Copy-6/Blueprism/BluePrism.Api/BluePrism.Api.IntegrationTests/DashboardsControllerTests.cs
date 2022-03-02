namespace BluePrism.Api.IntegrationTests
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
    using Server.Domain.Models.Dashboard;

    public class DashboardsControllerTests : ControllerTestBase<DashboardsControllerClient>
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
        public async Task GetWorkQueueCompositions_OnSuccess_ReturnsOkStatusCode()
        {
            var workQueueIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            GetMock<IServer>()
                .Setup(m => m.GetWorkQueueCompositions(workQueueIds))
                .Returns(GetTestWorkQueueCompositionItems());

            var result = await Subject.GetWorkQueueCompositions(workQueueIds);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetWorkQueueCompositions_OnSuccess_ReturnsExpectedGetWorkQueueCompositions()
        {
            var workQueueIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var workQueueCompositions = GetTestWorkQueueCompositionItems().ToList();

            GetMock<IServer>()
                .Setup(m => m.GetWorkQueueCompositions(workQueueIds))
                .Returns(workQueueCompositions);

            var result = await Subject.GetWorkQueueCompositions(workQueueIds);

            var resultContent = JsonConvert.DeserializeObject<IEnumerable<WorkQueueCompositionModel>>(await result.Content.ReadAsStringAsync());

            resultContent.ShouldAllBeEquivalentTo(workQueueCompositions);
        }

        [Test]
        public async Task GetWorkQueueCompositions_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetWorkQueueCompositions(new List<Guid> { Guid.NewGuid() });

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetWorkQueueCompositions_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            GetMock<IServer>()
                .Setup(x => x.GetWorkQueueCompositions(It.IsAny<IEnumerable<Guid>>()))
                .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetWorkQueueCompositions(new List<Guid> { Guid.NewGuid() });

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetWorkQueueCompositions_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.GetWorkQueueCompositions(It.IsAny<IEnumerable<Guid>>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetWorkQueueCompositions(new List<Guid> { Guid.NewGuid() });

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestCase(null)]
        [TestCase("workQueueIds=")]
        [TestCase("workQueueIds=00000000-0000-0000-0000-000000000000")]
        [TestCase("workQueueIds=4682CA5C-8140-42D4-BEBF-4D099F183FD5&workQueueIds=4682CA5C-8140-42D4-BEBF-4D099F183FD5")]
        public async Task GetWorkQueueCompositions_OnInvalidParameterSupplied_ReturnsBadRequestStatusCode(string queryString)
        {
            var result = await Subject.GetWorkQueueCompositionsUsingQueryString(queryString);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private static IEnumerable<WorkQueueComposition> GetTestWorkQueueCompositionItems() =>
            new List<WorkQueueComposition>
            {
                new WorkQueueComposition
                {
                    Id = Guid.NewGuid(),
                    Name = "TEST 1",
                    Completed = 4,
                    Locked = 5,
                    Deferred = 6,
                    Exceptioned = 7,
                    Pending = 8,
                },
                new WorkQueueComposition
                {
                    Id = Guid.NewGuid(),
                    Name = "TEST 2",
                    Completed = 0,
                    Locked = 1,
                    Deferred = 2,
                    Exceptioned = 3,
                    Pending = 4,
                },
                new WorkQueueComposition
                {
                    Id = Guid.NewGuid(),
                    Name = "TEST 3",
                    Completed = 1,
                    Locked = 2,
                    Deferred = 3,
                    Exceptioned = 4,
                    Pending = 5,
                },
            };
    }
}
