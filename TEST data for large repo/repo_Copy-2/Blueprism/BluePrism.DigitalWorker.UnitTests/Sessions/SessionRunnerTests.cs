using BluePrism.AutomateAppCore;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BluePrism.DigitalWorker.UnitTests.Sessions
{
    [TestFixture]
    public class SessionRunnerTests : UnitTestBase<SessionRunner>
    {
        private SessionContextBuilder _contextBuilder;

        public override void Setup()
        {
            _contextBuilder = new SessionContextBuilder();
            base.Setup();
        }

        [Test]
        public void Initialise_FactoryNotProvided_ErrorThrown()
        {
            Action initialise = () => new SessionRunner(null, null);
            initialise.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public async Task RunAsync_ShouldStart()
        {
            var context = _contextBuilder.Exclusive().Build();

            await ClassUnderTest.RunAsync(context);

            GetMock<IDigitalWorkerRunnerRecord>().Verify(x => x.RunnerMethod(), Times.Once);
        }

        [Test]
        public async Task RunAsync_ShouldRegisterRunner()
        {
            var context = _contextBuilder.Exclusive().Build();
            var runnerRecordMock = GetMock<IDigitalWorkerRunnerRecord>();

            await ClassUnderTest.RunAsync(context);

            GetMock<IRunningSessionRegistry >().Verify(x => x.Register(context.SessionId, runnerRecordMock.Object), Times.Once);
        }

        [Test]
        public async Task RunAsync_ShouldUnregisterRunnerWhenComplete()
        {
            var context = _contextBuilder.Exclusive().Build();
            var autoEvent = new AutoResetEvent(false);
            var runnerRecordMock = GetMock<IDigitalWorkerRunnerRecord>();

            // Simulate a long running process
            runnerRecordMock.Setup(x => x.RunnerMethod()).Callback(() => autoEvent.WaitOne());

            // Start the process run
            var task = ClassUnderTest.RunAsync(context);

            // Test that the unregister method has not been called
            GetMock<IRunningSessionRegistry >().Verify(x => x.Unregister(context.SessionId), Times.Never);
            
            // Allow the long running process to continue 
            autoEvent.Set();
            await task;

            // Test that the unregister method has now been called
            GetMock<IRunningSessionRegistry >().Verify(x => x.Unregister(context.SessionId), Times.Once);
        }
    }
}
