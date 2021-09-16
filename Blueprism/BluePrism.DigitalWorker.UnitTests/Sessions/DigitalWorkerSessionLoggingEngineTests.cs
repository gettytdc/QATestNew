using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.Processes;
using BluePrism.AutomateProcessCore.Stages;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace BluePrism.DigitalWorker.UnitTests.Sessions
{
    [TestFixture]
    public class DigitalWorkerSessionLoggingEngineTests : UnitTestBase<DigitalWorkerSessionLoggingEngine>
    {
        private static readonly Guid TestSessionId = Guid.Parse("d83371bc-45e1-45af-b47c-0bacac221845");
        private static LogInfo UninhibitedLogInfo = new LogInfo();
        private static LogInfo InhibitedLogInfo = new LogInfo { Inhibit = true };
        private static clsProcess ParentProcess = new clsProcess(Moq.Mock.Of<IGroupObjectDetails>(), DiagramType.Process, false);
        private const string TestObjectName = "theObject";
        private const string TestActionName = "theAction";
        private const string TestStageName = "theStage";
        private static Guid TestStageId = Guid.NewGuid();

        protected override DigitalWorkerSessionLoggingEngine TestClassConstructor()
            => new DigitalWorkerSessionLoggingEngine(
                GetMock<ILogEventPublisher>().Object,
                GetMock<IProcessLogContext>().Object,
                TestSessionId);

        [Test]
        public void Initialise_NoPublisherProvided_ErrorThrown()
        {
            Action initialise = () => new DigitalWorkerSessionLoggingEngine(
                null,
                GetMock<IProcessLogContext>().Object,
                TestSessionId);

            initialise.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void ActionEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsActionStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.ActionEpilogue(UninhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ActionStageEnded(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName), Times.Once);
        }

        [Test]
        public void ActionEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsActionStage(ParentProcess);

            ClassUnderTest.ActionEpilogue(InhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ActionStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ActionPrologue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsActionStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.ActionPrologue(UninhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ActionStageStarted(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName), Times.Once);
        }

        [Test]
        public void ActionPrologue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsActionStage(ParentProcess);

            ClassUnderTest.ActionPrologue(InhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ActionStageStarted(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void AlertEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsAlertStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.AlertEpilogue(UninhibitedLogInfo, stage, "alert");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.AlertStageEnded(TestSessionId, TestStageId, TestStageName, "alert"), Times.Once);
        }

        [Test]
        public void AlertEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsAlertStage(ParentProcess);

            ClassUnderTest.AlertEpilogue(InhibitedLogInfo, stage, "alert");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.AlertStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void CalculationEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsCalculationStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.CalculationEpilogue(UninhibitedLogInfo, stage, new clsProcessValue("result"));

            GetMock<ILogEventPublisher>()
                .Verify(p => p.CalculationStageEnded(TestSessionId, TestStageId, TestStageName, "result", (int)DataType.text), Times.Once);
        }

        [Test]
        public void CalculationEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsCalculationStage(ParentProcess);

            ClassUnderTest.CalculationEpilogue(InhibitedLogInfo, stage, "alert");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.CalculationStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void ChoiceEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsChoiceStartStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.ChoiceEpilogue(UninhibitedLogInfo, stage, "choiceTaken", 2);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ChoiceStageEnded(TestSessionId, TestStageId, TestStageName, "choiceTaken", 2), Times.Once);
        }

        [Test]
        public void ChoiceEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsChoiceStartStage(ParentProcess);

            ClassUnderTest.ChoiceEpilogue(InhibitedLogInfo, stage, "choiceTaken", 2);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ChoiceStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void CodeEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsCodeStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.CodeEpilogue(UninhibitedLogInfo, stage, new clsArgumentList(), new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.CodeStageEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void CodeEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsCodeStage(ParentProcess);

            ClassUnderTest.CodeEpilogue(InhibitedLogInfo, stage, new clsArgumentList(), new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.CodeStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void DecisionEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsDecisionStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.DecisionEpilogue(UninhibitedLogInfo, stage, "result");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.DecisionStageEnded(TestSessionId, TestStageId, TestStageName, "result"), Times.Once);
        }

        [Test]
        public void Decisionpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsDecisionStage(ParentProcess);

            ClassUnderTest.DecisionEpilogue(InhibitedLogInfo, stage, "result");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.DecisionStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void LogDiagnostic_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsActionStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.LogDiagnostic(UninhibitedLogInfo, stage, "message");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.DebugDataEmitted(TestSessionId, TestStageId, TestStageName, "message"), Times.Once);
        }

        [Test]
        public void LogDiagnostic_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsActionStage(ParentProcess);

            ClassUnderTest.LogDiagnostic(InhibitedLogInfo, stage, "message");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.DebugDataEmitted(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void LogError_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsActionStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.LogError(UninhibitedLogInfo, stage, "message");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessStageErrored(TestSessionId, TestStageId, TestStageName, "message"), Times.Once);
        }

        [Test]
        public void LogError_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsActionStage(ParentProcess);

            ClassUnderTest.LogError(InhibitedLogInfo, stage, "message");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessStageErrored(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void LoopEndEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsLoopEndStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.LoopEndEpilogue(UninhibitedLogInfo, stage, "iteration1");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.LoopEndStageEnded(TestSessionId, TestStageId, TestStageName, "iteration1"), Times.Once);
        }

        [Test]
        public void LoopEndEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsLoopEndStage(ParentProcess);

            ClassUnderTest.LoopEndEpilogue(InhibitedLogInfo, stage, "iteration1");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.LoopEndStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void LoopStartEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsLoopStartStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.LoopStartEpilogue(UninhibitedLogInfo, stage, "count");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.LoopStartStageEnded(TestSessionId, TestStageId, TestStageName, "count"), Times.Once);
        }

        [Test]
        public void LoopStartEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsLoopStartStage(ParentProcess);

            ClassUnderTest.LoopStartEpilogue(InhibitedLogInfo, stage, "count");

            GetMock<ILogEventPublisher>()
                .Verify(p => p.LoopStartStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void MultipleCalculationEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsMultipleCalculationStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.MultipleCalculationEpilogue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.MultipleCalculationStageEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void MultipleCalculationEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsMultipleCalculationStage(ParentProcess);

            ClassUnderTest.MultipleCalculationEpilogue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.MultipleCalculationStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void NavigateEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsNavigateStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName
            };

            ClassUnderTest.NavigateEpilogue(UninhibitedLogInfo, stage);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.NavigationStageEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void NavigateEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsNavigateStage(ParentProcess);

            ClassUnderTest.NavigateEpilogue(InhibitedLogInfo, stage);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.NavigationStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void NotePrologue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsNoteStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
                Narrative = "narrative"
            };

            ClassUnderTest.NotePrologue(UninhibitedLogInfo, stage);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.NoteStageStarted(TestSessionId, TestStageId, TestStageName, "narrative"), Times.Once);
        }

        [Test]
        public void NotePrologue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsNoteStage(ParentProcess);

            ClassUnderTest.NotePrologue(InhibitedLogInfo, stage);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.NoteStageStarted(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ObjectEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsEndStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.ObjectEpilogue(UninhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ObjectEnded(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName), Times.Once);
        }

        [Test]
        public void ObjectEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsEndStage(ParentProcess);

            ClassUnderTest.ObjectEpilogue(InhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ObjectEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ObjectPrologue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsStartStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.ObjectPrologue(UninhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ObjectStarted(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void ObjectPrologue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsStartStage(ParentProcess);

            ClassUnderTest.ObjectPrologue(InhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ObjectStarted(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ProcessEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsEndStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.ProcessEpilogue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void ProcessEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsEndStage(ParentProcess);

            ClassUnderTest.ProcessEpilogue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ProcessPrologue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsStartStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.ProcessPrologue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessStarted(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void ProcessPrologue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsStartStage(ParentProcess);

            ClassUnderTest.ProcessPrologue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessStarted(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ProcessRefEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsSubProcessRefStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.ProcessRefEpilogue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessReferenceEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void ProcessRefEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsSubProcessRefStage(ParentProcess);

            ClassUnderTest.ProcessRefEpilogue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessReferenceEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ProcessRefPrologue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsSubProcessRefStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.ProcessRefPrologue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessReferenceStarted(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void ProcessRefPrologue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsSubProcessRefStage(ParentProcess);

            ClassUnderTest.ProcessRefPrologue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ProcessReferenceStarted(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ReadEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsReadStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.ReadEpilogue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ReadStageEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void ReadEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsReadStage(ParentProcess);

            ClassUnderTest.ReadEpilogue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ReadStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void RecoverEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsRecoverStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.RecoverEpilogue(UninhibitedLogInfo, stage);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.RecoverStageEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void RecoverEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsRecoverStage(ParentProcess);

            ClassUnderTest.RecoverEpilogue(InhibitedLogInfo, stage);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.RecoverStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ResumeEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsResumeStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.ResumeEpilogue(UninhibitedLogInfo, stage);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ResumeStageEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void ResumeEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsResumeStage(ParentProcess);

            ClassUnderTest.ResumeEpilogue(InhibitedLogInfo, stage);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.ResumeStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SkillEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsSkillStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.SkillEpilogue(UninhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SkillStageEnded(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName), Times.Once);
        }

        [Test]
        public void SkillEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsSkillStage(ParentProcess);

            ClassUnderTest.SkillEpilogue(InhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SkillStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SkillPrologue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsSkillStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.SkillPrologue(UninhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SkillStageStarted(TestSessionId, TestStageId, TestStageName, TestObjectName, TestActionName), Times.Once);
        }

        [Test]
        public void SkillPrologue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsSkillStage(ParentProcess);

            ClassUnderTest.SkillPrologue(InhibitedLogInfo, stage, TestObjectName, TestActionName, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SkillStageStarted(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SubSheetEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsEndStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.SubSheetEpilogue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SubsheetEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void SubSheetEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsEndStage(ParentProcess);

            ClassUnderTest.SubSheetEpilogue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SubsheetEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SubSheetPrologue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsStartStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.SubSheetPrologue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SubsheetStarted(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void SubSheetPrologue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsStartStage(ParentProcess);

            ClassUnderTest.SubSheetPrologue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SubsheetStarted(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SubSheetRefEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsSubSheetRefStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.SubSheetRefEpilogue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SubsheetReferenceStageEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void SubSheetRefEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsSubSheetRefStage(ParentProcess);

            ClassUnderTest.SubSheetRefEpilogue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SubsheetReferenceStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void SubSheetRefPrologue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsSubSheetRefStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.SubSheetRefPrologue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SubsheetReferenceStageStarted(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void SubSheetRefPrologue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsSubSheetRefStage(ParentProcess);

            ClassUnderTest.SubSheetRefPrologue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.SubsheetReferenceStageStarted(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void WaitEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsWaitStartStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.WaitEpilogue(UninhibitedLogInfo, stage, "choice", 2);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.WaitStageEnded(TestSessionId, TestStageId, TestStageName, "choice", 2), Times.Once);
        }

        [Test]
        public void WaitEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsWaitStartStage(ParentProcess);

            ClassUnderTest.WaitEpilogue(InhibitedLogInfo, stage, "choice", 2);

            GetMock<ILogEventPublisher>()
                .Verify(p => p.WaitStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void WriteEpilogue_LoggingNotInhibited_PublishesCorrectly()
        {
            var stage = new clsWriteStage(ParentProcess)
            {
                Id = TestStageId,
                Name = TestStageName,
            };

            ClassUnderTest.WriteEpilogue(UninhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.WriteStageEnded(TestSessionId, TestStageId, TestStageName), Times.Once);
        }

        [Test]
        public void WriteEpilogue_LoggingInhibited_DoesNotPublish()
        {
            var stage = new clsWriteStage(ParentProcess);

            ClassUnderTest.WriteEpilogue(InhibitedLogInfo, stage, new clsArgumentList());

            GetMock<ILogEventPublisher>()
                .Verify(p => p.WriteStageEnded(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }
    }
}
