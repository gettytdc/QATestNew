namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BluePrism.Api.Domain.PagingTokens;
    using BpLibAdapters.Mappers;
    using BpLibAdapters.Mappers.FilterMappers;
    using CommonTestClasses;
    using CommonTestClasses.Extensions;
    using Domain;
    using Domain.Errors;
    using Domain.Filters;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Server.Domain.Models;
    using Server.Domain.Models.Pagination;
    using Utilities.Testing;

    using static Func.OptionHelper;
    using BpLibProcessSessionParameters = Server.Domain.Models.ProcessSessionParameters;
  
    using SessionStatus = Domain.SessionStatus;

    [TestFixture]
    public class SessionServerAdapterTests : UnitTestBase<SessionServerAdapter>
    {
        public override void OneTimeSetup() =>
            FilterMapper.SetFilterMappers(new List<IFilterMapper>
            {
                new NullFilterMapper(),
            });

        [Test]
        public async Task GetSessions_ShouldReturnSuccess_WhenSuccessful()
        {
            var sessions = new[] { new clsProcessSession() };

            GetMock<IServer>()
                .Setup(x => x.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Returns(sessions);

            var result = await ClassUnderTest.GetSessions(GetTestProcessSessionParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetSessions_ShouldReturnSuccess_WhenNoSessions()
        {
            GetMock<IServer>()
                .Setup(x => x.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Returns(Array.Empty<clsProcessSession>());

            var result = await ClassUnderTest.GetSessions(GetTestProcessSessionParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetSessions_ShouldReturnFailure_WhenPermissionExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.GetActualSessionsFilteredAndOrdered(It.IsAny<BpLibProcessSessionParameters>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetSessions(GetTestProcessSessionParameters());

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetSessionById_ShouldReturnSuccess_WhenSuccessful()
        {
            var session = new clsProcessSession();

            GetMock<IServer>()
                .Setup(x => x.GetActualSessionById(It.IsAny<Guid>()))
                .Returns(session);

            var result = await ClassUnderTest.GetActualSessionById(Guid.NewGuid());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetSessionById_ShouldReturnFailure_WhenNoSessionFound()
        {
            GetMock<IServer>()
                .Setup(x => x.GetActualSessions(It.IsAny<Guid>()))
                .Returns(Array.Empty<clsProcessSession>());

            var result = await ClassUnderTest.GetActualSessionById(Guid.Empty);

            (result is Failure<SessionNotFoundError>).Should().BeTrue();
        }

        [Test]
        public async Task GetSessionById_ShouldReturnFailure_WhenPermissionExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.GetActualSessionById(It.IsAny<Guid>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.GetActualSessionById(Guid.Empty);

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetSessionNumber_OnSuccess_ReturnsSuccess()
        {
            var sessionId = Guid.NewGuid();

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .Returns(1234);

            var result = await ClassUnderTest.GetSessionNumber(sessionId);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetSessionNumber_OnSuccess_ReturnsExpectedValue()
        {
            var sessionId = Guid.NewGuid();
            const int expectedSessionNumber = 1234;

            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .Returns(expectedSessionNumber);

            var result = await ClassUnderTest.GetSessionNumber(sessionId);

            ((Success<int>)result).Value.Should().Be(expectedSessionNumber);
        }

        [Test]
        public async Task GetSessionNumber_OnSessionNotFound_ReturnsSessionNotFoundError()
        {
            GetMock<IServer>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .Returns(-1);

            var result = await ClassUnderTest.GetSessionNumber(Guid.NewGuid());

            result.Should().BeAssignableTo<Failure<SessionNotFoundError>>();
        }

        [Test]
        [TestCaseSource(nameof(SortByPagingCases))]
        public async Task GetSessions_OnSuccess_ReturnsExpectedPagingToken(SessionSortByProperty sortByCase, object sortByValue)
        {
            var sessions = GetTestClsProcessSession(15).ToList();
            var sessionParameters = GetTestProcessSessionParameters();
            sessionParameters.SortBy = sortByCase;

            var testPagingToken = new PagingToken<long>
            {
                PreviousIdValue = sessions.Last().SessionNum,
                DataType = sortByValue.GetTypeName(),
                ParametersHashCode = sessionParameters.GetHashCodeForValidation(),
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(sortByValue)
            }.ToString();

            GetMock<IServer>()
                .Setup(m => m.GetActualSessionsFilteredAndOrdered(It.IsAny<Server.Domain.Models.ProcessSessionParameters>()))
                .Returns(sessions);

            var result = await ClassUnderTest.GetSessions(sessionParameters);
            var resultValue = ((Success<ItemsPage<Session>>)result).Value.PagingToken;

            ((Some<string>)resultValue).Value.Should().Be(testPagingToken);
        }


        [Test]
        public async Task GetSessions_OnSuccessWhenNoMoreItemsLeftToReturn_ReturnsNonePagingToken()
        {
            var sessions = SessionsHelper.GetTestBluePrismClsProcessSession(5).ToList();

            var sessionParameters = GetTestProcessSessionParameters();

            GetMock<IServer>()
                .Setup(m => m.GetActualSessionsFilteredAndOrdered(It.IsAny<Server.Domain.Models.ProcessSessionParameters>()))
                .Returns(sessions);

            var result = await ClassUnderTest.GetSessions(sessionParameters);
            var resultValue = ((Success<ItemsPage<Session>>)result).Value.PagingToken;

            resultValue.Should().BeAssignableTo<None<string>>();
        }

        private static IEnumerable<TestCaseData> SortByPagingCases => new (SessionSortByProperty, object)[]
            {
                (SessionSortByProperty.StageStartedAsc,  new DateTimeOffset(new DateTime(2020,2,1))),
                (SessionSortByProperty.StageStartedDesc, new DateTimeOffset(new DateTime(2020,2,1))),
                (SessionSortByProperty.ProcessNameAsc, "some process name"),
                (SessionSortByProperty.ProcessNameDesc, "some process name"),
                (SessionSortByProperty.ResourceNameAsc, "some resource name"),
                (SessionSortByProperty.ResourceNameDesc, "some resource name"),
                (SessionSortByProperty.UserAsc, "some user name"),
                (SessionSortByProperty.UserDesc, "some user name"),
                (SessionSortByProperty.StatusAsc, SessionStatus.Completed),
                (SessionSortByProperty.StatusDesc, SessionStatus.Completed),
                (SessionSortByProperty.ExceptionTypeAsc, "some exception type"),
                (SessionSortByProperty.ExceptionTypeDesc, "some exception type"),
                (SessionSortByProperty.StartTimeAsc, new DateTimeOffset(new DateTime(2020,2,1))),
                (SessionSortByProperty.StartTimeDesc, new DateTimeOffset(new DateTime(2020,2,1))),
                (SessionSortByProperty.EndTimeAsc, new DateTimeOffset(new DateTime(2020,2,1))),
                (SessionSortByProperty.EndTimeDesc, new DateTimeOffset(new DateTime(2020,2,1))),
                (SessionSortByProperty.LatestStageAsc, "some stage"),
                (SessionSortByProperty.LatestStageDesc, "some stage"),
                (SessionSortByProperty.StageStartedAsc, new DateTimeOffset(new DateTime(2020,2,1))),
                (SessionSortByProperty.StageStartedDesc, new DateTimeOffset(new DateTime(2020,2,1)))
            }
            .ToTestCaseData();

        private static IEnumerable<clsProcessSession> GetTestClsProcessSession(int count = 1)
        {
            var sessionsList = new List<clsProcessSession>(count);
            for (var i = 0; i < count; i++)
            {
                sessionsList.Add(new clsProcessSession
                {
                    SessionID = Guid.NewGuid(),
                    SessionNum = 1,
                    SessionStart = new DateTimeOffset(new DateTime(2020,2,1)),
                    SessionEnd = new DateTimeOffset(new DateTime(2020, 2, 1)),
                    LastUpdated = new DateTimeOffset(new DateTime(2020, 2, 1)),
                    LastStage = "some stage",
                    ProcessID = Guid.NewGuid(),
                    ProcessName = "some process name",
                    ResourceID = Guid.NewGuid(),
                    ResourceName = "some resource name",
                    Status = Server.Domain.Models.SessionStatus.Completed,
                    UserName = "some user name",
                    ExceptionMessage = "message",
                    ExceptionType = "some exception type",
                    SessionTerminationReason = AutomateAppCore.SessionTerminationReason.InternalError
                });
            }

            return sessionsList;
        }


        private SessionParameters GetTestProcessSessionParameters() =>
            new SessionParameters
            {
                ItemsPerPage = 10,
                PagingToken = Some(new Domain.PagingTokens.PagingToken<long>()),
                ProcessName = new NullFilter<string>(),
                SessionNumber = new NullFilter<string>(),
                ResourceName = new NullFilter<string>(),
                User = new NullFilter<string>(),
                Status = new NullFilter<Domain.SessionStatus>(),
                StartTime = new NullFilter<DateTimeOffset>(),
                EndTime = new NullFilter<DateTimeOffset>(),
                LatestStage = new NullFilter<string>(),
                StageStarted = new NullFilter<DateTimeOffset>(),
            };
    }
}
