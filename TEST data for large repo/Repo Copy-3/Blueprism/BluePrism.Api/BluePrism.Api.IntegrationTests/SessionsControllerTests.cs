namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Autofac;
    using BpLibAdapters;
    using ControllerClients;
    using Models;
    using Server.Domain.Models;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BpLibAdapters.Mappers;
    using Common.Security;
    using CommonTestClasses;
    using FluentAssertions;
    using Func;
    using Mappers;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Server.Domain.Models.DataFilters;
    using BpLibProcessSessionParameters = Server.Domain.Models.ProcessSessionParameters;
    using SessionParameters = Models.SessionParameters;
    using SessionStatus = Server.Domain.Models.SessionStatus;

    [TestFixture]
    public class SessionsControllerTests : ControllerTestBase<SessionsControllerClient>
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
        public async Task GetSessions_ShouldReturnSessions_WhenSuccessful()
        {
            var sessions = new[]
            {
                new clsProcessSession
                {
                    SessionID = Guid.NewGuid(), Status = SessionStatus.Running
                },
            };

            GetMock<IServer>()
                .Setup(x => x.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Returns(sessions);

            var result = await Subject.GetSessions()
                .Map(x => x.Content.ReadAsAsync<ItemsPageModel<SessionModel>>());

            var resultSessionsId = result.Items.Select(x => x.SessionNumber);
            var expectedSessionId = sessions.Select(x => x.SessionNum);

            resultSessionsId.Should().BeEquivalentTo(expectedSessionId);
        }


        [Test]
        public async Task GetSessions_ShouldReturnHttpStatusOkWithEmptyCollectionAndNullToken_WhenSessionsHasNoItems()
        {
            GetMock<IServer>()
                .Setup(m => m.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Returns(Array.Empty<clsProcessSession>());

            var result = await Subject.GetSessions();

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var sessionResult = await result.Map(x => x.Content.ReadAsAsync<ItemsPageModel<SessionModel>>());
            sessionResult.Items.Should().BeEmpty();
            sessionResult.PagingToken.Should().BeNull();
        }

        [Test]
        public async Task GetSessions_ShouldReturnHttpStatusCodeUnauthorized_WhenBluePrismLoginFailed()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.UnableToFindUser));

            var result = await Subject.GetSessions();

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetSessions_ShouldReturnHttpStatusCodeUnauthorized_WhenUserNotAuthenticated()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.NotAuthenticated));

            var result = await Subject.GetSessions();

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetSessions_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            GetMock<IServer>()
                .Setup(x => x.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Throws(new InvalidOperationException("ServerError message"));

            var result = await Subject.GetSessions();

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task GetSessions_ShouldReturnHttpStatusForbidden_WhenNoUserPermissions()
        {
            GetMock<IServer>()
                .Setup(x => x.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Throws(new PermissionException("Error message"));

            var result = await Subject.GetSessions();

            result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task GetSessions_ShouldReturnHttpStatusBadRequest_WhenInvalidStatusSupplied()
        {
            var result = await Subject.GetSessionsUsingQueryString("status=bvcxz");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestCaseSource(nameof(InvalidParameters))]
        public async Task GetSessions_ShouldReturnHttpStatusBadRequestWithInvalidField_WhenInvalidFilterParameterSupplied((SessionParameters, string) invalidParameter)
        {
            var (sessionParameters, expectedInvalidFieldName) = invalidParameter;

            var result = await Subject.GetSessions(parameters: sessionParameters);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await result.Content.ReadAsStringAsync())
                .Map(JsonConvert.DeserializeObject<IList<ValidationErrorModel>>)
                .First()
                .InvalidField
                .Should()
                .Be(expectedInvalidFieldName);
        }

        [Test]
        public async Task GetSessions_ShouldReturnHttpStatusBadRequestWithPagingInvalidFields_WhenInvalidPagingFilterParametersSupplied()
        {
            const string pagingToken = "eyJQcmV2aW91c0lkVmFsdWUiOjM4LCJQcmV2aW91c1NvcnRDb2x1bW5WYWx1ZSI6IjM4IiwiRGF0YVR5cGUiOiJJbnQzMiIsIlBhcmFtZXRlcnNIYXNoQ29kZSI6LTIwMTExODk0MzR9";
            const int itemsPerPage = 1001;

            var result = await Subject.GetSessions(pagingToken, itemsPerPage);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await result.Content.ReadAsStringAsync())
                .Map(JsonConvert.DeserializeObject<IList<ValidationErrorModel>>)
                .Select(t => t.InvalidField)
                .Should()
                .Contain(new[] { nameof(SessionParameters.PagingToken), nameof(SessionParameters.ItemsPerPage) });
        }

        [Test]
        public async Task GetSessions_OnInvalidStageStarted_ShouldReturnBadRequestStatusCode()
        {
            var result = await Subject.GetSessionsUsingQueryString("stageStarted.eq=Test");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Test]
        public async Task GetSessions_OnInvalidStartTime_ShouldReturnBadRequestStatusCode()
        {
            var result = await Subject.GetSessionsUsingQueryString("startTime.eq=Test");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Test]
        public async Task GetSessions_OnInvalidEndTime_ShouldReturnBadRequestStatusCode()
        {
            var result = await Subject.GetSessionsUsingQueryString("endTime.eq=Test");

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSessions_OnAllBlankValuesForFilter_ShouldReturnOkStatusCode()
        {
            GetMock<IServer>()
                .Setup(x => x.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Returns(new clsProcessSession[0]);

            var result = await Subject.GetSessionsUsingQueryString("startTime.eq=&startTime.gte=");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetSession_ShouldReturnOk_WhenValidSessionId()
        {
            var sessionId = Guid.NewGuid();
            var clsProcessSession = new clsProcessSession { SessionID = sessionId, Status = SessionStatus.Completed };

            GetMock<IServer>()
                .Setup(m => m.GetActualSessionById(It.IsAny<Guid>()))
                .Returns(clsProcessSession);

            var result = await Subject.GetSession(sessionId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetSession_ShouldReturnSession_WhenSuccessful()
        {
            var sessionId = Guid.NewGuid();
            var clsProcessSession = new clsProcessSession { SessionID = sessionId, Status = SessionStatus.Completed };

            GetMock<IServer>()
                .Setup(m => m.GetActualSessionById(It.IsAny<Guid>()))
                .Returns(clsProcessSession);

            var result = await Subject.GetSession(sessionId)
                .Map(x => x.Content.ReadAsAsync<SessionModel>());

            result.SessionId.Should().Be(sessionId);
        }

        [Test]
        public async Task GetSession_OnSessionNotFound_ReturnsSessionNotFoundResponse()
        {
            var sessionId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.GetActualSessions(It.IsAny<Guid>()))
                .Returns((clsProcessSession[])null);

            var result = await Subject.GetSession(sessionId);

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        [TestCase("SessionNumberDesc")]
        [TestCase("sessionNumberDesc")]
        [TestCase("Sessionnumberdesc")]
        [TestCase("SESSIONNUMBERDESC")]
        public async Task GetSessions_OnVariousSortByCasings_ShouldSortByExpectedField(string sortBy)
        {
            GetMock<IServer>()
                .Setup(m => m.GetActualSessionsFilteredAndOrdered(It.Is<BpLibProcessSessionParameters>(p => p.SortBy == ProcessSessionSortByProperty.SessionNumberDesc)))
                .Returns(new clsProcessSession[0]);

            var result = await Subject.GetSessionsUsingQueryString($"sortBy={sortBy}");

            result.IsSuccessStatusCode.Should().BeTrue();

            GetMock<IServer>()
                .Verify(
                    m => m.GetActualSessionsFilteredAndOrdered(It.Is<BpLibProcessSessionParameters>(p => p.SortBy == ProcessSessionSortByProperty.SessionNumberDesc)),
                    Times.Once);
        }

        [Test]
        [TestCase("")]
        [TestCase("sortBy=")]
        public async Task GetSessions_WithNoSortBy_ShouldUseSessionNumberAscForSorting(string sortBy)
        {
            GetMock<IServer>()
                .Setup(m => m.GetActualSessionsFilteredAndOrdered(It.Is<BpLibProcessSessionParameters>(p => p.SortBy == ProcessSessionSortByProperty.SessionNumberAsc)))
                .Returns(new clsProcessSession[0]);

            var result = await Subject.GetSessionsUsingQueryString(sortBy);
            if (!result.IsSuccessStatusCode)
            {
                Assert.Fail($"Call to API failed with message: {await result.Content.ReadAsStringAsync()}");
            }

            GetMock<IServer>()
                .Verify(
                    m => m.GetActualSessionsFilteredAndOrdered(It.Is<BpLibProcessSessionParameters>(p => p.SortBy == ProcessSessionSortByProperty.SessionNumberAsc)),
                    Times.Once);
        }

        [Test]
        public async Task GetSessions_WithCommaSeparatedStatuses_AppliesAllStatusFilters()
        {
            var passedParameters = default(BpLibProcessSessionParameters);

            GetMock<IServer>()
                .Setup(m => m.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Callback((BpLibProcessSessionParameters x) => passedParameters = x)
                .Returns(new clsProcessSession[0]);

            var result = await Subject.GetSessionsUsingQueryString("status=pending, stopped");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            ((MultiValueDataFilter<SessionStatus>)passedParameters.Status)
                .OfType<EqualsDataFilter<SessionStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { SessionStatus.Pending, SessionStatus.Stopped });
        }

        [Test]
        public async Task GetSessions_WithMultipleSeparateStatuses_AppliesAllStatusFilters()
        {
            var passedParameters = default(BpLibProcessSessionParameters);

            GetMock<IServer>()
                .Setup(m => m.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Callback((BpLibProcessSessionParameters x) => passedParameters = x)
                .Returns(new clsProcessSession[0]);

            var result = await Subject.GetSessionsUsingQueryString("status=pending&status=stopped");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            ((MultiValueDataFilter<SessionStatus>)passedParameters.Status)
                .OfType<EqualsDataFilter<SessionStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { SessionStatus.Pending, SessionStatus.Stopped });
        }

        [Test]
        public async Task GetSessions_WithFullPathSpecifications_AppliesAllStatusFilters()
        {
            var passedParameters = default(BpLibProcessSessionParameters);

            GetMock<IServer>()
                .Setup(m => m.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Callback((BpLibProcessSessionParameters x) => passedParameters = x)
                .Returns(new clsProcessSession[0]);

            var result = await Subject.GetSessionsUsingQueryString("sessionParameters.status=pending, stopped");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            ((MultiValueDataFilter<SessionStatus>)passedParameters.Status)
                .OfType<EqualsDataFilter<SessionStatus>>()
                .Select(x => x.EqualTo)
                .ShouldBeEquivalentTo(new[] { SessionStatus.Pending, SessionStatus.Stopped });
        }

        [Test]
        public async Task GetSessions_ShouldReturnSessionsWithPagingToken_WhenSuccessful()
        {
            var sessions = SessionsHelper.GetTestBluePrismClsProcessSession(10).ToList();

            var lastItem = sessions.OrderBy(x => x.SessionNum).Last();

            var sessionParameters = new SessionParameters() { ItemsPerPage = 5 };

            var testPagingToken = new PagingTokenModel<long>
            {
                PreviousIdValue = lastItem.SessionNum,
                DataType = lastItem.SessionNum.GetType().Name,
                ParametersHashCode = sessionParameters.ToDomainObject().GetHashCodeForValidation(),
                PreviousSortColumnValue = "1"
            };

            var testSessions = sessions.Select(x => x.ToDomainObject().ToModelObject()).ToList();
            GetMock<IServer>()
                .Setup(m => m.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Returns(sessions);

            var result = await Subject.GetSessions(testPagingToken.ToString(), sessionParameters.ItemsPerPage.Value)
                .Map(x => x.Content.ReadAsAsync<ItemsPageModel<SessionModel>>());

            result.Items.ShouldBeEquivalentTo(testSessions);
            result.PagingToken.Should().Be(testPagingToken.ToString());
        }

        [Test]
        public async Task GetSessions_ShouldReturnBadRequest_WhenDifferentParametersPagingTokenProvided()
        {
            var sessions = SessionsHelper.GetTestBluePrismClsProcessSession(10).ToList();

            var lastItem = sessions.OrderBy(x => x.SessionNum).Last();

            var initialSessionParameters = SessionsHelper.GetTestDomainProcessSessionParameters();

            var testPagingToken = new PagingTokenModel<long>
            {
                PreviousIdValue = lastItem.SessionNum,
                DataType = lastItem.SessionNum.GetType().Name,
                ParametersHashCode = initialSessionParameters.GetHashCodeForValidation(),
                PreviousSortColumnValue = "1"
            };

            var testSessionParameters = new SessionParameters
            {
                ItemsPerPage = 3,
                PagingToken = testPagingToken.ToString()
            };

            var result = await Subject.GetSessions(testSessionParameters.PagingToken, testSessionParameters.ItemsPerPage.Value);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSessions_ShouldReturnBadRequest_WhenInvalidPagingTokenProvided()
        {
            var pagingToken = "testToken";
            var result = await Subject.GetSessions(pagingToken);

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private static (SessionParameters, string)[] InvalidParameters() =>
            new[]
            {
                (new SessionParameters {ProcessName = GetInvalidStringFilterModel()}, "ProcessName"),
                (new SessionParameters {SessionNumber = GetInvalidStringFilterModel()}, "SessionNumber"),
                (new SessionParameters {UserName = GetInvalidStringFilterModel()}, "UserName"),
                (new SessionParameters {LatestStage = GetInvalidStringFilterModel()}, "LatestStage"),
                (new SessionParameters {ResourceName = GetInvalidStringFilterModel()}, "ResourceName"),
                (new SessionParameters {StageStarted = GetInvalidRangeFilterModel<DateTimeOffset?>(DateTimeOffset.UtcNow)}, "StageStarted"),
                (new SessionParameters {StartTime = GetInvalidRangeFilterModel<DateTimeOffset?>(DateTimeOffset.UtcNow)}, "StartTime"),
                (new SessionParameters {EndTime = GetInvalidRangeFilterModel<DateTimeOffset?>(DateTimeOffset.UtcNow)}, "EndTime"),
            };

        private static StartsWithOrContainsStringFilterModel GetInvalidStringFilterModel() =>
            new StartsWithOrContainsStringFilterModel
            {
                Eq = "a",
                Gte = "a",
                Lte = "a",
            };

        private static RangeFilterModel<T> GetInvalidRangeFilterModel<T>(T value) =>
            new RangeFilterModel<T>
            {
                Eq = value,
                Gte = value,
                Lte = value,
            };
    }
}
