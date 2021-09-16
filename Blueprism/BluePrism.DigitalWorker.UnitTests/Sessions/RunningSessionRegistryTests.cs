using System;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.Utilities.Testing;
using NUnit.Framework;
using FluentAssertions;

namespace BluePrism.DigitalWorker.UnitTests.Sessions
{
    [TestFixture]
    public class RunningSessionRegistryTests : UnitTestBase<RunningSessionRegistry>
    {
        private IDigitalWorkerRunnerRecord _runner;
        private Guid _sessionId;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _runner = GetMock<IDigitalWorkerRunnerRecord>().Object;
            _sessionId = Guid.NewGuid();
        }

        [Test]
        public void Register_WithNoRegisteredSession_ShouldNotThrow()
        {
            Action action = () => ClassUnderTest.Register(_sessionId, _runner);

            action.ShouldNotThrow();
        }

        [Test]
        public void Register_WithRegisteredSession_ShouldThrow()
        {
            ClassUnderTest.Register(_sessionId, _runner);

            Action action = () => ClassUnderTest.Register(_sessionId, _runner);

            action.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void Get_WithRegisteredSession_ShouldReturnRunnerRecord()
        {
            ClassUnderTest.Register(_sessionId, _runner);
            var runnerRecord = ClassUnderTest.Get(_sessionId);
            runnerRecord.Should().Be(_runner);
        }

        [Test]
        public void Get_WithRegisteredSession_ShouldNotThrow()
        {
            ClassUnderTest.Register(_sessionId, _runner);

            Action action = () => ClassUnderTest.Get(_sessionId);
            
            action.ShouldNotThrow();
        }

        [Test]
        public void Get_NoExistingSession_WillReturnNull()
        {
            var runnerRecord = ClassUnderTest.Get(_sessionId);

            runnerRecord.ShouldBeEquivalentTo(null);
        }

        [Test]
        public void Unregister_SessionAlreadyRegistered_ShouldNotThrow()
        {
            ClassUnderTest.Register(_sessionId, _runner);

            Action action = () => ClassUnderTest.Unregister(_sessionId);
            
            action.ShouldNotThrow();
        }

        [Test]
        public void Unregister_NoExistingSession_ShouldThrow()
        {
            Action action = () => ClassUnderTest.Unregister(_sessionId);

            action.ShouldThrow<ArgumentException>();
        }
    }
}
