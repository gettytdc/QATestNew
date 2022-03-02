using System;
using System.Linq;
using System.Threading.Tasks;
using BluePrism.DigitalWorker.Messages.Commands;
using BluePrism.DigitalWorker.Messaging;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.UnitTesting.TestSupport.MassTransit;
using FluentAssertions;
using MassTransit;
using Moq;
using NUnit.Framework;

namespace BluePrism.DigitalWorker.UnitTests.Messaging
{
    [TestFixture]
    public class RequestStopProcessConsumerTests : ConsumerTestBase<RequestStopProcessConsumer>
    {
        private static readonly Guid _testSessionId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");
        private Mock<IDigitalWorkerRunnerRecord> _runnerRecordMock;
        private IRunningSessionRegistry _runningSessionRegistry;
        private Mock<RequestStopProcess> _messageMock;
        private Mock<ConsumeContext<RequestStopProcess>> _consumeContextMock;
        
        protected override RequestStopProcessConsumer TestClassConstructor()
            => new RequestStopProcessConsumer(_runningSessionRegistry);

        public override void Setup()
        {
            base.Setup();

            _runnerRecordMock = GetMock<IDigitalWorkerRunnerRecord>();
            
            _runningSessionRegistry = new RunningSessionRegistry ();
            _runningSessionRegistry.Register(_testSessionId, _runnerRecordMock.Object);

            _messageMock = GetMock<RequestStopProcess>();
            _messageMock.Setup(m => m.SessionId).Returns(_testSessionId);

            var uri = new Uri("http://test/");

            _consumeContextMock = GetMock<ConsumeContext<RequestStopProcess>>();
            _consumeContextMock.Setup(x => x.Message).Returns(_messageMock.Object);
            _consumeContextMock.Setup(x => x.DestinationAddress).Returns(uri);
        }

        [Test]
        public async Task Consume_StopSession_Exits()
        {
            await Bus.InputQueueSendEndpoint.Send<RequestStopProcess>(new { SessionId = _testSessionId });

            var consumed = Bus.Consumed.Select<RequestStopProcess>().FirstOrDefault();

            consumed.Should().NotBeNull();

            consumed?.Exception.Should().BeNull();
        }

        [Test]
        public async Task RequestStopProcess_CurrentlyRunningProcess_ShouldSetStopRequestedFlagOnRunnerRecord()
        {
            _runnerRecordMock.SetupSet(x => x.StopRequested = true).Verifiable();

            await ClassUnderTest.Consume(_consumeContextMock.Object);

            _runnerRecordMock.Verify();
        }

        [Test]
        public void RequestStopProcess_SessionAlreadyFinished_ShouldNotThrow()
        {
            _runningSessionRegistry.Unregister(_testSessionId);

            Action action = () => ClassUnderTest.Consume(_consumeContextMock.Object).Wait();

            action.ShouldNotThrow();
        }
    }
}
