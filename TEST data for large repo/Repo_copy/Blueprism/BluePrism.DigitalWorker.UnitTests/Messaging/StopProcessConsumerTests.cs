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
    public class StopProcessConsumerTests : ConsumerTestBase<StopProcessConsumer>
    {
        private static string _testUsername = "Bob";
        private static string _testSessionStopReason = "";
        private static readonly Guid _testSessionId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");
        private Mock<IDigitalWorkerRunnerRecord> _runnerRecordMock;
        private IRunningSessionRegistry  _runningSessionRegistry;
        private Mock<StopProcess> _messageMock;
        private Mock<ConsumeContext<StopProcess>> _consumeContextMock;

        protected override StopProcessConsumer TestClassConstructor()
            => new StopProcessConsumer(_runningSessionRegistry);

        public override void Setup()
        {
            base.Setup();

            _runnerRecordMock = GetMock<IDigitalWorkerRunnerRecord>();

            _runningSessionRegistry = new RunningSessionRegistry ();
            _runningSessionRegistry.Register(_testSessionId, _runnerRecordMock.Object);

            _messageMock = GetMock<StopProcess>();
            _messageMock.Setup(m => m.SessionId).Returns(_testSessionId);
            _messageMock.Setup(m => m.Username).Returns(_testUsername);

            var uri = new Uri("http://test/");

            _consumeContextMock = GetMock<ConsumeContext<StopProcess>>();
            _consumeContextMock.Setup(x => x.Message).Returns(_messageMock.Object);
            _consumeContextMock.Setup(x => x.DestinationAddress).Returns(uri);
        }

        [Test]
        public async Task Consume_StopSession_Exits()
        {
            await Bus.InputQueueSendEndpoint.Send<StopProcess>(new { SessionId = _testSessionId, Username = _testUsername });

            var consumed = Bus.Consumed.Select<StopProcess>().FirstOrDefault();

            consumed.Should().NotBeNull();

            consumed?.Exception.Should().BeNull();
        }

        [Test]
        public async Task StopSession_ShouldCallStopProcessOnRunner()
        {
            await ClassUnderTest.Consume(_consumeContextMock.Object);

            _runnerRecordMock.Verify(x => x.StopProcess(_testUsername, string.Empty, _testSessionStopReason), Times.Once);
        }
    }
}
