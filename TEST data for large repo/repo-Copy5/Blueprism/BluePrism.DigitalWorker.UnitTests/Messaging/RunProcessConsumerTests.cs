using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.AutomateProcessCore;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands.Factory;
using BluePrism.DigitalWorker.Messages.Commands;
using BluePrism.DigitalWorker.Messages.Events;
using BluePrism.DigitalWorker.Messaging;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.DigitalWorker.Sessions.Coordination;
using BluePrism.UnitTesting.TestSupport.MassTransit;
using FluentAssertions;
using MassTransit;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace BluePrism.DigitalWorker.UnitTests.Messaging
{
    [TestFixture]
    public class RunProcessConsumerTests : ConsumerTestBase<RunProcessConsumer>
    {
        private static readonly Guid TestSessionId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");
        private static readonly Guid TestProcessId = Guid.Parse("d83871bc-45e1-45af-b47c-0bacac221845");
        private static readonly ProcessInfo TestProcessInfo = new ProcessInfo(TestProcessId, BusinessObjectRunMode.Foreground, "<process />");
        private const string SessionStartedByUsername = "Tester";
        private static readonly DigitalWorkerName TestDigitalWorkerName = new DigitalWorkerName("Worker1");
        private static readonly DigitalWorkerStartUpOptions StartUpOptions = new DigitalWorkerStartUpOptions { Name = TestDigitalWorkerName };
        private static readonly DigitalWorkerContext TestDigitalWorkerContext = new DigitalWorkerContext(StartUpOptions);

        protected override RunProcessConsumer TestClassConstructor()
        {
            return new RunProcessConsumer(
                GetMock<ISessionCoordinator>().Object,
                GetMock<IProcessInfoLoader>().Object,
                GetMock<ISessionServiceClient>().Object,
                TestDigitalWorkerContext);
        }

        private void SetupProcess()
        {
            GetMock<IProcessInfoLoader>().Setup(x => x.GetProcess(TestProcessId)).Returns(TestProcessInfo);
        }

        private void SetupSessionStatus(StartProcessStatus status)
        {
            var message = SessionServiceCommands.RequestStartProcessResponse(status);
            var response = TestResponse.Create(message);
            GetMock<ISessionServiceClient>().Setup(x =>
                    x.RequestStartProcess(TestSessionId, StartUpOptions.Name.FullName))
                .Returns(Task.FromResult(response.Message.Status));
        }

        private bool ValidateContext(SessionContext actual, SessionContext expected)
        {
            actual.ShouldBeEquivalentTo(expected);
            return true;
        }

        [Test]
        public async Task Consume_WithValidProcess_RunsProcessWithProcessData()
        {
            SetupProcess();
            SetupSessionStatus(StartProcessStatus.ReadyToStart);

            await Bus.InputQueueSendEndpoint.Send<RunProcess>(new { SessionId = TestSessionId, ProcessId = TestProcessId, Username = SessionStartedByUsername });

            Bus.Consumed.Select<RunProcess>().Should().HaveCount(1);
            var expectedContext = new SessionContext(TestSessionId, TestProcessInfo, SessionStartedByUsername);
            GetMock<ISessionCoordinator>().Verify(x => x.RunProcess(
                It.Is<SessionContext>(sc => ValidateContext(sc, expectedContext)),
                It.IsAny<CancellationToken>())
            );
        }

        [Test]
        public async Task Consume_ProcessCompletes_Exits()
        {
            SetupProcess();
            SetupSessionStatus(StartProcessStatus.ReadyToStart);

            GetMock<ISessionCoordinator>().Setup(x => x.RunProcess(
                    It.IsAny<SessionContext>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await Bus.InputQueueSendEndpoint.Send<RunProcess>(new { SessionId = TestSessionId, ProcessId = TestProcessId });

            var consumed = Bus.Consumed.Select<RunProcess>().FirstOrDefault();
            consumed.Should().NotBeNull();
            consumed.Exception.Should().BeNull();
        }

        [Test]
        public async Task Consume_Timeout_ThrowsAllowingRetry()
        {
            SetupProcess();
            SetupSessionStatus(StartProcessStatus.ReadyToStart);

            GetMock<ISessionCoordinator>().Setup(x => x.RunProcess(
                    It.IsAny<SessionContext>(),
                    It.IsAny<CancellationToken>()))
                .Returns((SessionContext _, CancellationToken t) => Task.FromCanceled(new CancellationToken(true)));

            await Bus.InputQueueSendEndpoint.Send<RunProcess>(new { SessionId = TestSessionId, ProcessId = TestProcessId });

            Bus.Consumed.Select<RunProcess>().Should().HaveCount(1);
            Bus.Consumed.Select<RunProcess>().First().Exception.Should().BeOfType<DigitalWorkerBusyException>();
        }

        [Test]
        public async Task Consume_ProcessAlreadyStarted_DoesNotRestartSession()
        {
            SetupProcess();
            SetupSessionStatus(StartProcessStatus.AlreadyStarted);

            await Bus.InputQueueSendEndpoint.Send<RunProcess>(new { SessionId = TestSessionId, ProcessId = TestProcessId });

            Bus.Consumed.Select<RunProcess>().Should().HaveCount(1);
            GetMock<ISessionCoordinator>().Verify(s => s.RunProcess(
                It.IsAny<SessionContext>(), It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Test]
        public async Task Consume_WithInvalidProcess_DoesNotStartSession()
        {
            await Bus.InputQueueSendEndpoint.Send<RunProcess>(new { SessionId = TestSessionId });

            Bus.Consumed.Select<RunProcess>().Should().HaveCount(1);
            Bus.Consumed.Select<RunProcess>().First().Exception.Should().NotBeNull();
            GetMock<ISessionCoordinator>().Verify(s => s.RunProcess(
                It.IsAny<SessionContext>(), It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Test]
        public async Task Consume_RequestToStartProcessTimesOut_ProcessPreStartFailedPublished()
        {
            SetupProcess();

            GetMock<ISessionServiceClient>()
                .Setup(x =>
                    x.RequestStartProcess(TestSessionId, StartUpOptions.Name.FullName))
                .Throws(new RequestTimeoutException());

            await Bus.InputQueueSendEndpoint.Send<RunProcess>(new { SessionId = TestSessionId, ProcessId = TestProcessId });

            Bus.Published.Select<ProcessPreStartFailed>().Should().HaveCount(1);
        }

        [Test]
        public async Task Consume_RequestToStartProcessThrowsException_ShouldRethrowException()
        {
            var testException = new Exception("It's all gone a bit wrong");
            SetupProcess();

            GetMock<ISessionServiceClient>()
                .Setup(x =>
                    x.RequestStartProcess(TestSessionId, StartUpOptions.Name.FullName))
                .Throws(testException);

            await Bus.InputQueueSendEndpoint.Send<RunProcess>(new { SessionId = TestSessionId, ProcessId = TestProcessId });

            var faultedRunProcessMessage = Bus.Published.Select<Fault<RunProcess>>();
            var exception = faultedRunProcessMessage.First().Context.Message.Exceptions.First();

            exception.Message.Should().Be(testException.Message);
            exception.ExceptionType.Should().Be(testException.GetType().ToString());
        }

    }
}
