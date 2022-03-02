using System;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.AutomateProcessCore;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Notifications;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ProcessInfo = BluePrism.DigitalWorker.Sessions.ProcessInfo;

namespace BluePrism.DigitalWorker.UnitTests.Sessions
{
    [TestFixture]
    public class RunningSessionMonitorTests : UnitTestBase<RunningSessionMonitor>
    {
        private static readonly Guid TestSessionId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");
        private static readonly Guid TestProcessId = Guid.Parse("d83871bc-45e1-45af-b47c-0bacac221845");
        private static readonly string TestProcessXml = "<process />";
        private const string SessionStartedByUsername = "Tester";
        private const string Message = "Message";
        private string ExpectedMessage(string message) 
            => $"[{string.Format("{0:u}", Now.DateTime)}] {message}";

        private DateTimeOffset Now = DateTimeOffset.Now;

        public override void Setup()
        {
            base.Setup();

            GetMock<ISystemClock>()
                .Setup(c => c.Now)
                .Returns(Now);
        }

        private void VerifyHandledNotification(ResourceNotificationLevel level, string message)
            => GetMock<INotificationHandler>()
                .Verify(h => h.HandleNotification(
                    It.Is<ResourceNotification>(n => n.Level == level && n.Text == ExpectedMessage(message))));

        [Test]
        public void Initialise_NoHandlerProvided_ExceptionThrown()
        {
            Action create = () => new RunningSessionMonitor(null, GetMock<ISystemClock>().Object);
            create.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Initialise_NoClockProvided_ExceptionThrown()
        {
            Action create = () => new RunningSessionMonitor(GetMock<INotificationHandler>().Object, null);
            create.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void HandleSessionStatusFailure_RaiseErrorMessage()
        {
            var process = new ProcessInfo(TestProcessId, BusinessObjectRunMode.Exclusive, TestProcessXml);
            var context = new SessionContext(TestSessionId, process, SessionStartedByUsername);
            var runnerRecord = new DigitalWorkerRunnerRecord(context, 
                ClassUnderTest, 
                GetMock<ISessionStatusPublisher>().Object,
                GetMock<ISessionCleanup>().Object,
                (_, __) => Moq.Mock.Of<clsDBLoggingEngine>());
            ClassUnderTest.HandleSessionStatusFailure(runnerRecord, Message);

            VerifyHandledNotification(ResourceNotificationLevel.Error, $"Session failed ({TestSessionId}): {Message}");
        }

        [Test]
        public void RaiseError_RaiseErrorMessage()
        {
            ClassUnderTest.RaiseError(Message);

            VerifyHandledNotification(ResourceNotificationLevel.Error, Message);
        }

        [Test]
        public void RaiseError_WithParams_RaiseErrorMessage()
        {
            ClassUnderTest.RaiseError(Message + ": {0} {1} {2}", new string[] { "param1", "param2", "param3" });

            VerifyHandledNotification(ResourceNotificationLevel.Error, Message + ": param1 param2 param3");
        }

        [Test]
        public void RaiseInfo_RaiseNotificationMessage()
        {
            ClassUnderTest.RaiseInfo(Message);

            VerifyHandledNotification(ResourceNotificationLevel.Comment, Message);
        }

        [Test]
        public void RaiseInfo_WithParams_RaiseNotificationMessage()
        {
            ClassUnderTest.RaiseInfo(Message + ": {0} {1} {2}", new string[] { "param1", "param2", "param3" });

            VerifyHandledNotification(ResourceNotificationLevel.Comment, Message + ": param1 param2 param3");
        }

        [Test]
        public void RaiseWarn_RaiseWarningMessage()
        {
            ClassUnderTest.RaiseWarn(Message);

            VerifyHandledNotification(ResourceNotificationLevel.Warning, Message);
        }

        [Test]
        public void RaiseWarn_WithParams_RaiseWarningMessage()
        {
            ClassUnderTest.RaiseWarn(Message + ": {0} {1} {2}", new string[] { "param1", "param2", "param3" });

            VerifyHandledNotification(ResourceNotificationLevel.Warning, Message + ": param1 param2 param3");
        }

        [Test]
        public void AddNotification_RaiseInfomationMessage()
        {
            ClassUnderTest.AddNotification(Message);

            VerifyHandledNotification(ResourceNotificationLevel.Comment, Message);
        }

        [Test]
        public void NotifyStatus_RaiseNotificationMessage()
        {
            ClassUnderTest.NotifyStatus();

            VerifyHandledNotification(ResourceNotificationLevel.Verbose, "Status update");
        }

        [Test]
        public void VarChanged_RaiseNotificationMessage()
        {
            ClassUnderTest.VarChanged(GetMock<clsSessionVariable>().Object);

            VerifyHandledNotification(ResourceNotificationLevel.Verbose, "Var changed");
        }
    }
}