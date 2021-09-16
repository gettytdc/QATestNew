using System;
using BluePrism.DigitalWorker.EnvironmentFunctions;
using NUnit.Framework;
using FluentAssertions;
using BluePrism.DigitalWorker.Sessions;
using Moq;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateAppCore;

namespace BluePrism.DigitalWorker.UnitTests.EnvironmentFunctions
{
    [TestFixture]
    public class GetUserNameFunctionTests : EnvironmentFunctionsTestBase<GetUserNameFunction>
    {
        private Mock<IRunningSessionRegistry> _runningSessionRegistryMock;
        private IRunningSessionRegistry _runningSessionRegistry;

        protected override GetUserNameFunction TestClassConstructor()
            => new GetUserNameFunction(_runningSessionRegistry);

        public override void Setup()
        {
            base.Setup();

            _runningSessionRegistryMock = GetMock<IRunningSessionRegistry>();

            var testSessionContext = new SessionContext(SessionId, 
                new BluePrism.DigitalWorker.Sessions.ProcessInfo(Guid.NewGuid(), BusinessObjectRunMode.Background, string.Empty), TestSessionStartedByUsername);
            var testDigitalWorkerRunnerRecord = new DigitalWorkerRunnerRecord(testSessionContext, null, null, 
                GetMock<ISessionCleanup>().Object, (_, __) => Moq.Mock.Of<clsDBLoggingEngine>());

            _runningSessionRegistryMock.Setup(s => s.Get(SessionId)).Returns(testDigitalWorkerRunnerRecord);
            
            _runningSessionRegistry = _runningSessionRegistryMock.Object;
        }
        
        [Test]
        public void Evaluate_WithUsername_ShouldReturnUsername()
        {
            var evaluateResult = ClassUnderTest.Evaluate(null, Process);

            evaluateResult.EncodedValue.ShouldBeEquivalentTo(TestSessionStartedByUsername);
        }

        [Test]
        public void Evaluate_WithNoProcessSession_ShouldReturnEmptyString()
        {
            var evaluateResult = ClassUnderTest.Evaluate(null, 
                new clsProcess(GetMock<IGroupObjectDetails>().Object, AutomateProcessCore.Processes.DiagramType.Process, false));

            evaluateResult.EncodedValue.ShouldBeEquivalentTo(string.Empty);
        }

        [Test]
        public void Evaluate_SessionNotInDWSessionCoordinator_ShouldReturnEmptyString()
        {
            _runningSessionRegistryMock.Setup(s => s.Get(SessionId)).Returns((IDigitalWorkerRunnerRecord)null);

            var evaluateResult = ClassUnderTest.Evaluate(null, Process);

            evaluateResult.EncodedValue.ShouldBeEquivalentTo(string.Empty);
        }
    }
}