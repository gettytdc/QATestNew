using System;
using BluePrism.AutomateProcessCore;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Messages.Events;
using BluePrism.DigitalWorker.Messages.Events.LogEntryData;
using BluePrism.DigitalWorker.Messaging;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BluePrism.DigitalWorker.UnitTests.Sessions
{
    [TestFixture]
    public class SessionLogPublisherTests : UnitTestBase<LogEventPublisher>
    {
        private static readonly Guid TestSessionId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");
        private static readonly Guid TestStageId = Guid.NewGuid();
        private const string TestStageName = "theStage";
        private const string TestObjectName = "theObject";
        private const string TestActionName = "theAction";
        private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

        public override void Setup()
        {
            base.Setup();

            GetMock<ISystemClock>()
                .Setup(c => c.Now)
                .Returns(Now);
        }

        [Test]
        public void Initialise_NoBusProvided_ErrorThrown()
        {
            Action initialise = () => new LogEventPublisher(null, GetMock<ISystemClock>().Object);
            initialise.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Initialise_NoClockProvided_ErrorThrown()
        {
            Action initialise = () => new LogEventPublisher(GetMock<IMessageBusWrapper>().Object, null);
            initialise.ShouldThrow<ArgumentNullException>();
        }

        private bool StageEventHasCorrectInformation(ProcessStageLogged stageEvent)
            => stageEvent.SessionId == TestSessionId
                && stageEvent.EntryNumber == 1
                && stageEvent.Date == Now
                && stageEvent.StageId == TestStageId
                && stageEvent.StageName == TestStageName;

        [Test]
        public void StageEvent_MultiplePreviousEvents_IncrementedEntryNumber()
        {
            ClassUnderTest.ActionStageEnded(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName);
            ClassUnderTest.CalculationStageEnded(TestSessionId, TestStageId, TestStageName, "result", (int)DataType.text);
            ClassUnderTest.AlertStageEnded(TestSessionId, TestStageId, TestStageName, "alert");

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(It.Is<ProcessStageLogged>(s => s.EntryNumber == 1)),
                            Times.Once);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(It.Is<ProcessStageLogged>(s => s.EntryNumber == 2)),
                            Times.Once);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(It.Is<ProcessStageLogged>(s => s.EntryNumber == 3)),
                            Times.Once);
        }

        [Test]
        public void ActionStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.ActionStageEnded(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.ActionStageEnded
                    && ((ActionLogEntryData) s.Data).ObjectName == TestObjectName
                    && ((ActionLogEntryData)s.Data).ActionName == TestActionName)), Times.Once);
        }

        [Test]
        public void ActionStageStarted_PublishesCorrectInformation()
        {
            ClassUnderTest.ActionStageStarted(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.ActionStageStarted
                    && ((ActionLogEntryData)s.Data).ObjectName == TestObjectName
                    && ((ActionLogEntryData)s.Data).ActionName == TestActionName)), Times.Once);
        }

        [Test]
        public void AlertStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.AlertStageEnded(TestSessionId, TestStageId, TestStageName, "alert");

            GetMock<IMessageBusWrapper>()
                 .Verify(b => b.Publish<ProcessStageLogged>(
                     It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                     && s.EntryType == ProcessStageLogEntryType.AlertStageEnded
                     && ((AlertLogEntryData)s.Data).Alert == "alert")), Times.Once);
        }

        [Test]
        public void CalculationStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.CalculationStageEnded(TestSessionId, TestStageId, TestStageName, "result", (int)DataType.text);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.CalculationStageEnded
                    && ((CalculationLogEntryData)s.Data).Result == "result"
                    && ((CalculationLogEntryData)s.Data).DataType == (int)DataType.text)), Times.Once);
        }

        [Test]
        public void ChoiceStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.ChoiceStageEnded(TestSessionId, TestStageId, TestStageName, "choice", 2);

            GetMock<IMessageBusWrapper>()
                 .Verify(b => b.Publish<ProcessStageLogged>(
                     It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                     && s.EntryType == ProcessStageLogEntryType.ChoiceStageEnded
                     && ((ChoiceLogEntryData)s.Data).ChoiceTaken == "choice"
                     && ((ChoiceLogEntryData)s.Data).ChoiceNumber == 2)), Times.Once);
        }

        [Test]
        public void CodeStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.CodeStageEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.CodeStageEnded
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void DebugDataEmitted_PublishesCorrectInformation()
        {
            ClassUnderTest.DebugDataEmitted(TestSessionId, TestStageId, TestStageName, "message");

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.DebugDataEmitted
                    && ((DebugLogEntryData)s.Data).Message == "message")), Times.Once);
        }

        [Test]
        public void DecisionStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.DecisionStageEnded(TestSessionId, TestStageId, TestStageName, "result");

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.DecisionStageEnded
                    && ((DecisionLogEntryData)s.Data).Result == "result")), Times.Once);
        }

        [Test]
        public void LoopEndStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.LoopEndStageEnded(TestSessionId, TestStageId, TestStageName, "iteration");

            GetMock<IMessageBusWrapper>()
                 .Verify(b => b.Publish<ProcessStageLogged>(
                     It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                     && s.EntryType == ProcessStageLogEntryType.LoopEndStageEnded
                     && ((LoopLogEntryData)s.Data).Iteration == "iteration")), Times.Once);
        }

        [Test]
        public void LoopStartStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.LoopStartStageEnded(TestSessionId, TestStageId, TestStageName, "iteration");

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.LoopStartStageEnded
                    && ((LoopLogEntryData)s.Data).Iteration == "iteration")), Times.Once);
        }

        [Test]
        public void MultipleCalculationStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.MultipleCalculationStageEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.MultipleCalculationStageEnded
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void NavigationStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.NavigationStageEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.NavigationStageEnded
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void NoteStageStarted_PublishesCorrectInformation()
        {
            ClassUnderTest.NoteStageStarted(TestSessionId, TestStageId, TestStageName, "note");

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.NoteStageStarted
                    && ((NoteLogEntryData)s.Data).Narrative == "note")), Times.Once);
        }

        [Test]
        public void ObjectEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.ObjectEnded(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.ObjectEnded
                    && ((ActionLogEntryData)s.Data).ObjectName == TestObjectName
                    && ((ActionLogEntryData)s.Data).ActionName == TestActionName)), Times.Once);
        }

        [Test]
        public void ObjectStarted_PublishesCorrectInformation()
        {
            ClassUnderTest.ObjectStarted(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                 .Verify(b => b.Publish<ProcessStageLogged>(
                     It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                     && s.EntryType == ProcessStageLogEntryType.ObjectStarted
                     && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void ProcessEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.ProcessEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                 .Verify(b => b.Publish<ProcessStageLogged>(
                     It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                     && s.EntryType == ProcessStageLogEntryType.ProcessEnded
                     && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void ProcessStarted_PublishesCorrectInformation()
        {
            ClassUnderTest.ProcessStarted(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.ProcessStarted
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void ProcessReferenceEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.ProcessReferenceEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.ProcessReferenceEnded
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void ProcessReferenceStarted_PublishesCorrectInformation()
        {
            ClassUnderTest.ProcessReferenceStarted(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.ProcessReferenceStarted
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void ProcessStageErrored_PublishesCorrectInformation()
        {
            ClassUnderTest.ProcessStageErrored(TestSessionId, TestStageId, TestStageName, "error");

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.ProcessStageErrored
                    && ((ErrorLogEntryData)s.Data).ErrorMessage == "error")), Times.Once);
        }

        [Test]
        public void ReadStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.ReadStageEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.ReadStageEnded
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void RecoverStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.RecoverStageEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.RecoverStageEnded
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void ResumeStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.ResumeStageEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                 .Verify(b => b.Publish<ProcessStageLogged>(
                     It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                     && s.EntryType == ProcessStageLogEntryType.ResumeStageEnded
                     && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void SkillStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.SkillStageEnded(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.SkillStageEnded
                    && ((ActionLogEntryData)s.Data).ObjectName == TestObjectName
                    && ((ActionLogEntryData)s.Data).ActionName == TestActionName)), Times.Once);
        }

        [Test]
        public void SkillStageStarted_PublishesCorrectInformation()
        {
            ClassUnderTest.SkillStageStarted(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.SkillStageStarted
                    && ((ActionLogEntryData)s.Data).ObjectName == TestObjectName
                    && ((ActionLogEntryData)s.Data).ActionName == TestActionName)), Times.Once);
        }

        [Test]
        public void SubsheetEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.SubsheetEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.SubsheetEnded
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void SubsheetReferenceStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.SubsheetReferenceStageEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.SubsheetReferenceStageEnded
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void SubsheetReferenceStageStarted_PublishesCorrectInformation()
        {
            ClassUnderTest.SubsheetReferenceStageStarted(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.SubsheetReferenceStageStarted
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void SubsheetStarted_PublishesCorrectInformation()
        {
            ClassUnderTest.SubsheetStarted(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.SubsheetStarted
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }

        [Test]
        public void WaitStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.WaitStageEnded(TestSessionId, TestStageId, TestStageName, "choice", 2);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.WaitStageEnded
                    && ((WaitLogEntryData)s.Data).ChoiceName == "choice"
                    && ((WaitLogEntryData)s.Data).ChoiceNumber == 2)), Times.Once);
        }

        [Test]
        public void WriteStageEnded_PublishesCorrectInformation()
        {
            ClassUnderTest.WriteStageEnded(TestSessionId, TestStageId, TestStageName);

            GetMock<IMessageBusWrapper>()
                .Verify(b => b.Publish<ProcessStageLogged>(
                    It.Is<ProcessStageLogged>(s => StageEventHasCorrectInformation(s)
                    && s.EntryType == ProcessStageLogEntryType.WriteStageEnded
                    && (EmptyLogEntryData)s.Data != null)), Times.Once);
        }
    }
}
