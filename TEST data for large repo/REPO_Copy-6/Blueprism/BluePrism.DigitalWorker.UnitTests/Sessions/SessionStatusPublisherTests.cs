using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Messaging;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using BluePrism.AutomateAppCore;
using BluePrism.DigitalWorker.Messages.Events;

namespace BluePrism.DigitalWorker.UnitTests.Sessions
{
    [TestFixture]
    public class SessionStatusPublisherTests : UnitTestBase<SessionStatusPublisher>
    {
        private static readonly DateTimeOffset Now = DateTimeOffset.Now;
        private static readonly SessionContext TestSessionContext = new SessionContextBuilder().Build();
        private IRunningSessionRegistry _runningSessionRegistry;

        protected override SessionStatusPublisher TestClassConstructor()
        {
            return new SessionStatusPublisher(
                GetMock<IMessageBusWrapper>().Object,
                GetMock<ISystemClock>().Object,
                TestSessionContext);
        }

        public override void Setup()
        {
            base.Setup();

            GetMock<ISystemClock>()
                .Setup(c => c.Now)
                .Returns(Now);

            _runningSessionRegistry = new RunningSessionRegistry();

            var runnerRecordMock = GetMock<IDigitalWorkerRunnerRecord>();
            runnerRecordMock.Setup(r => r.StopRequested).Returns(false);
            runnerRecordMock.Setup(r => r.SessionStarted).Returns(Now);

            _runningSessionRegistry.Register(TestSessionContext.SessionId, runnerRecordMock.Object);
        }
        
        [Test]
        public void Initialise_NoBusProvided_ExceptionThrown()
        {
            Action create = () => new SessionStatusPublisher(null,
                GetMock<ISystemClock>().Object,
                TestSessionContext);

            create.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Initialise_NoClockProvided_ExceptionThrown()
        {
            Action create = () => new SessionStatusPublisher(
                GetMock<IMessageBusWrapper>().Object,
                null,
                TestSessionContext);

            create.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void SetPendingSessionRunning_SendsCorrectInformation()
        {
            ProcessStarted @event = null;

            GetMock<IMessageBusWrapper>()
                .Setup(b => b.Publish(It.IsAny<ProcessStarted>()))
                .Callback((ProcessStarted e) => @event = e).Returns(Task.CompletedTask);

            ClassUnderTest.SetPendingSessionRunning(Now);
            @event.Should().NotBeNull();
            @event.ShouldBeEquivalentTo(new { SessionId = TestSessionContext.SessionId, Date = Now});
        }

        [Test]
        public void SetSessionTerminated_SendsCorrectInformation()
        {
            var exceptionType = "Fatal Error";
            var exceptionMessage = "Everything went wrong";
            ClassUnderTest.SetSessionTerminated(SessionExceptionDetail.ProcessError(exceptionType, exceptionMessage));
                        
            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessTerminated>(It.Is<ProcessTerminated>(e => e.SessionId == TestSessionContext.SessionId && 
                e.Date == Now && e.TerminationReason == TerminationReason.ProcessError && 
                e.ExceptionType == exceptionType && e.ExceptionMessage == exceptionMessage)));
        }

        [Test]
        public void SetSessionStopped_SendsCorrectInformation()
        {
            ClassUnderTest.SetSessionStopped();

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStopped>(It.Is<ProcessStopped>(e => e.SessionId == TestSessionContext.SessionId && e.Date == Now)));
        }

        [Test]
        public void SetSessionCompleted_SendsCorrectInformation()
        {
            ClassUnderTest.SetSessionCompleted();

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessCompleted>(It.Is<ProcessCompleted>(e => e.SessionId == TestSessionContext.SessionId && e.Date == Now)));
        }
    }
}
