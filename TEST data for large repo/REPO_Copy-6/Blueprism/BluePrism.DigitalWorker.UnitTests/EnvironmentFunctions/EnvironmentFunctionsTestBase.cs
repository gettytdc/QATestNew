using System;
using System.Collections.Generic;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.AutomateProcessCore;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Notifications;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.Utilities.Testing;
using Moq;
using NUnit.Framework;

namespace BluePrism.DigitalWorker.UnitTests.EnvironmentFunctions
{
    public abstract class EnvironmentFunctionsTestBase<T> : UnitTestBase<T>
    {
        private const string _testProcessXml = "<process />";
        private readonly clsSession _session = new clsSession(Guid.Parse("885fbeb2-1834-4d76-bb88-9ebf7a8ecdf5"), 0, new WebConnectionSettings(2, 5, 1, new List<UriWebConnectionSettings>()));

        protected IDigitalWorkerRunnerRecord TestDigitalWorkerRunnerRecord { get; private set; }
        protected IRunningSessionRegistry TestSessionRegistry { get; private set; }
        protected Guid SessionId => _session.ID;
        protected clsProcess Process => new clsProcess(GetMock<IGroupObjectDetails>().Object, AutomateProcessCore.Processes.DiagramType.Process, false) { Session = _session };
        protected DigitalWorkerName TestDigitalWorkerName = new DigitalWorkerName("Fred");
        protected string TestSessionStartedByUsername = "TestUser";
        protected readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            GetMock<ISystemClock>()
                .Setup(c => c.UtcNow)
                .Returns(Now);
        
            var process = new ProcessInfo(Guid.Parse("e17f204a-3325-4159-854b-db529be95628"), BusinessObjectRunMode.Exclusive, _testProcessXml);
            var context = new SessionContext(SessionId, process, TestSessionStartedByUsername);
            var runningSessionMonitor = new RunningSessionMonitor(new NotificationHandler(), GetMock<ISystemClock>().Object);

            var runnerRecordMock = new Mock<IDigitalWorkerRunnerRecord>();
            runnerRecordMock.Setup(x => x.SessionStarted).Returns(Now);
            runnerRecordMock.SetupProperty(x => x.StopRequested);
            TestDigitalWorkerRunnerRecord = runnerRecordMock.Object;
            
            TestSessionRegistry = new RunningSessionRegistry();
            TestSessionRegistry.Register(SessionId, TestDigitalWorkerRunnerRecord);
        }
    }
}
