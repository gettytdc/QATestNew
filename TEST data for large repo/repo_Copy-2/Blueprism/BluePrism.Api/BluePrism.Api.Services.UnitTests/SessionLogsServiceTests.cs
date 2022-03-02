namespace BluePrism.Api.Services.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using Autofac;
    using BpLibAdapters;
    using CommonTestClasses.Extensions;
    using Domain;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    using static Func.ResultHelper;

    [TestFixture]
    public class SessionLogsServiceTests : UnitTestBase<SessionLogsService>
    {
        public override void Setup() =>
            base.Setup(builder =>
            {
                builder.RegisterGeneric(typeof(MockAdapterAuthenticatedMethodRunner<>)).As(typeof(IAdapterAuthenticatedMethodRunner<>));
                builder.RegisterGeneric(typeof(MockAdapterAnonymousMethodRunner<>)).As(typeof(IAdapterAnonymousMethodRunner<>));
                builder.RegisterInstance(new SessionLogConfiguration {MaxResultStringLength = 10});
            });

        [Test]
        public async Task GetLogs_OnSuccess_ReturnsSuccess()
        {
            var sessionId = Guid.NewGuid();

            GetMock<ISessionServerAdapter>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .ReturnsAsync(Succeed(1234));

            GetMock<ISessionLogsServerAdapter>()
                .Setup(m => m.GetLogs(1234, It.IsAny<SessionLogsParameters>()))
                .ReturnsAsync(Succeed(new ItemsPage<SessionLogItem>()));

            var result = await ClassUnderTest.GetSessionLogs(sessionId, new SessionLogsParameters());

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetLogs_OnInvalidId_ReturnsSessionNotFoundError()
        {
            GetMock<ISessionServerAdapter>()
                .Setup(m => m.GetSessionNumber(It.IsAny<Guid>()))
                .ReturnsAsync(ResultHelper<int>.Fail<SessionNotFoundError>());

            var result = await ClassUnderTest.GetSessionLogs(Guid.NewGuid(), new SessionLogsParameters());

            result.Should().BeAssignableTo<Failure<SessionNotFoundError>>();
        }

        [Test]
        public async Task GetLogs_OnSuccess_ReturnsExpectedLogs()
        {
            var sessionId = Guid.NewGuid();

            var expectedLog = new SessionLogItem
                {
                    LogId = 1234,
                    StageName = "TestStage",
                    StageType = StageTypes.Action,
                    ResourceStartTime = OptionHelper.Some(DateTimeOffset.UtcNow),
                    Result = "TestResult",
                };

            GetMock<ISessionServerAdapter>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .ReturnsAsync(Succeed(1234));

            GetMock<ISessionLogsServerAdapter>()
                .Setup(m => m.GetLogs(1234, It.IsAny<SessionLogsParameters>()))
                .ReturnsAsync(Succeed(new ItemsPage<SessionLogItem> { Items = new[] { expectedLog }}));

            var result = await ClassUnderTest.GetSessionLogs(sessionId, new SessionLogsParameters());

            ((Success<ItemsPage<SessionLogItem>>)result).Value.Items.ShouldRuntimeTypesBeEquivalentTo(new[] { expectedLog });
        }

        [Test]
        public async Task GetLogs_OnSuccess_TruncatesLongStrings()
        {
            var sessionId = Guid.NewGuid();

            var testLog = new SessionLogItem
                {
                    LogId = 1234,
                    StageName = "TestStage",
                    StageType = StageTypes.Action,
                    ResourceStartTime = OptionHelper.Some(DateTimeOffset.UtcNow),
                    Result = "ThisIsLongerThanConfiguredMax",
                };

            var expectedLog = new SessionLogItem
                {
                    LogId = 1234,
                    StageName = testLog.StageName,
                    StageType = testLog.StageType,
                    ResourceStartTime = testLog.ResourceStartTime,
                    Result = "ThisIsLong...",
                };

            GetMock<ISessionServerAdapter>()
                .Setup(m => m.GetSessionNumber(sessionId))
                .ReturnsAsync(Succeed(1234));

            GetMock<ISessionLogsServerAdapter>()
                .Setup(m => m.GetLogs(1234, It.IsAny<SessionLogsParameters>()))
                .ReturnsAsync(Succeed(new ItemsPage<SessionLogItem> { Items = new[] { testLog }}));

            var result = await ClassUnderTest.GetSessionLogs(sessionId, new SessionLogsParameters());

            ((Success<ItemsPage<SessionLogItem>>)result).Value.Items.ShouldRuntimeTypesBeEquivalentTo(new[] { expectedLog });
        }

        [Test]
        public async Task GetLogParameters_OnSuccess_ReturnsSuccess()
        {
            GetMock<ISessionLogsServerAdapter>()
                .Setup(x => x.GetLogParameters(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync(Succeed(new SessionLogItemParameters()));

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetLogParameters_OnInvalidSessionId_ReturnsSessionNotFoundError()
        {
            GetMock<ISessionLogsServerAdapter>()
                .Setup(m => m.GetLogParameters(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync(ResultHelper<SessionLogItemParameters>.Fail<SessionNotFoundError>());

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Failure<SessionNotFoundError>>();
        }

        [Test]
        public async Task GetLogParameters_OnInvalidLogId_ReturnsLogNotFoundError()
        {
            GetMock<ISessionLogsServerAdapter>()
                .Setup(m => m.GetLogParameters(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync(ResultHelper<SessionLogItemParameters>.Fail<LogNotFoundError>());

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Failure<LogNotFoundError>>();
        }

        [Test]
        public async Task GetLogParameters_OnPermissionError_ReturnsPermissionError()
        {
            GetMock<ISessionLogsServerAdapter>()
                .Setup(m => m.GetLogParameters(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync(ResultHelper<SessionLogItemParameters>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Failure<PermissionError>>();
        }
    }
}
