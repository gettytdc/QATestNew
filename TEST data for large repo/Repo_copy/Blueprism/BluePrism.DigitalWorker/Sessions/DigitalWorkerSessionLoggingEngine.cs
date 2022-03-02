using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.Stages;
using BluePrism.BPCoreLib;
using System;
using BluePrism.DigitalWorker.Messaging;

namespace BluePrism.DigitalWorker.Sessions
{
    public class DigitalWorkerSessionLoggingEngine : clsLoggingEngine
    {
        private readonly Guid _sessionId;
        private readonly ILogEventPublisher _publisher;

        public DigitalWorkerSessionLoggingEngine(ILogEventPublisher publisher, IProcessLogContext logContext, Guid sessionId)
        : base(logContext)
        {
            _sessionId = sessionId;
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        private void Publish(LogInfo info, Action publish)
        {
            if (info.Inhibit)
                return;

            publish();
        }

        public override void ActionEpilogue(LogInfo info, clsActionStage stg, string objectName, string actionName, clsArgumentList outputs)
            => Publish(info, () => _publisher.ActionStageEnded(_sessionId, stg.Id, stg.Name, objectName, actionName));

        public override void ActionPrologue(LogInfo info, clsActionStage stg, string objectName, string actionName, clsArgumentList inputs)
            => Publish(info, () => _publisher.ActionStageStarted(_sessionId, stg.Id, stg.Name, objectName, actionName));

        public override void AlertEpilogue(LogInfo info, clsAlertStage stg, string alert)
            => Publish(info, () => _publisher.AlertStageEnded(_sessionId, stg.Id, stg.Name, alert));

        public override void AlertPrologue(LogInfo info, clsAlertStage stg) {}

        public override void CalculationEpilogue(LogInfo info, clsCalculationStage stg, clsProcessValue resultVal)
            => Publish(info, () => _publisher.CalculationStageEnded(_sessionId, stg.Id, stg.Name, resultVal.LoggableValue, (int)resultVal.DataType));

        public override void CalculationPrologue(LogInfo info, clsCalculationStage stg) {}

        public override void ChoiceEpilogue(LogInfo info, clsChoiceStartStage stg, string choiceTaken, int choiceNumber)
            => Publish(info, () => _publisher.ChoiceStageEnded(_sessionId, stg.Id, stg.Name, choiceTaken, choiceNumber));

        public override void ChoicePrologue(LogInfo info, clsChoiceStartStage stg){}

        public override void CodeEpilogue(LogInfo info, clsCodeStage stg, clsArgumentList inputs, clsArgumentList outputs)
            => Publish(info, () => _publisher.CodeStageEnded(_sessionId, stg.Id, stg.Name));

        public override void CodePrologue(LogInfo info, clsCodeStage stg, clsArgumentList inputs) { }

        public override void DecisionEpilogue(LogInfo info, clsDecisionStage stg, string result)
            => Publish(info, () => _publisher.DecisionStageEnded(_sessionId, stg.Id, stg.Name, result));

        public override void DecisionPrologue(LogInfo info, clsDecisionStage stg){}

        public override void ExceptionEpilogue(LogInfo info, clsExceptionStage stg) { }

        public override void ExceptionPrologue(LogInfo info, clsExceptionStage stg) { }

        public override void LogDiagnostic(LogInfo info, clsProcessStage stg, string message)
            => Publish(info, () => _publisher.DebugDataEmitted(_sessionId, stg.Id, stg.Name, message));

        public override void LogError(LogInfo info, clsProcessStage stg, string errorMessage)
            => Publish(info, () => _publisher.ProcessStageErrored(_sessionId, stg.Id, stg.Name, errorMessage));

        public override void LogExceptionScreenshot(LogInfo info, clsProcessStage stg, string processName, clsPixRect image, DateTimeOffset timestamp) { }

        public override void LogStatistic(Guid sessId, string statName, clsProcessValue val) { }

        public override void LoopEndEpilogue(LogInfo info, clsLoopEndStage stg, string iteration)
            => Publish(info, () => _publisher.LoopEndStageEnded(_sessionId, stg.Id, stg.Name, iteration));

        public override void LoopEndPrologue(LogInfo info, clsLoopEndStage stg) { }

        public override void LoopStartEpilogue(LogInfo info, clsLoopStartStage stg, string count)
            => Publish(info, () => _publisher.LoopStartStageEnded(_sessionId, stg.Id, stg.Name, count));

        public override void LoopStartPrologue(LogInfo info, clsLoopStartStage stg) { }

        public override void MultipleCalculationEpilogue(LogInfo info, clsMultipleCalculationStage stg, clsArgumentList results)
            => Publish(info, () => _publisher.MultipleCalculationStageEnded(_sessionId, stg.Id, stg.Name));

        public override void MultipleCalculationPrologue(LogInfo info, clsMultipleCalculationStage stg) { }

        public override void NavigateEpilogue(LogInfo info, clsNavigateStage stg)
            => Publish(info, () => _publisher.NavigationStageEnded(_sessionId, stg.Id, stg.Name));

        public override void NavigatePrologue(LogInfo info, clsNavigateStage stg) { }

        public override void NoteEpilogue(LogInfo info, clsNoteStage stg) { }

        public override void NotePrologue(LogInfo info, clsNoteStage stg)
            => Publish(info, () => _publisher.NoteStageStarted(_sessionId, stg.Id, stg.Name, stg.GetNarrative()));

        public override void ObjectEpilogue(LogInfo info, clsEndStage stg, string objectName, string actionName, clsArgumentList outputs)
            => Publish(info, () => _publisher.ObjectEnded(_sessionId, stg.Id, stg.Name, objectName, actionName));

        public override void ObjectPrologue(LogInfo info, clsStartStage stg, string objectName, string actionName, clsArgumentList inputs)
            => Publish(info, () => _publisher.ObjectStarted(_sessionId, stg.Id, stg.Name));

        public override void ProcessEpilogue(LogInfo info, clsEndStage stg, clsArgumentList outputs)
            => Publish(info, () => _publisher.ProcessEnded(_sessionId, stg.Id, stg.Name));

        public override void ProcessPrologue(LogInfo info, clsStartStage stg, clsArgumentList inputs)
            => Publish(info, () => _publisher.ProcessStarted(_sessionId, stg.Id, stg.Name));

        public override void ProcessRefEpilogue(LogInfo info, clsSubProcessRefStage stg, clsArgumentList outputs)
            => Publish(info, () => _publisher.ProcessReferenceEnded(_sessionId, stg.Id, stg.Name));

        public override void ProcessRefPrologue(LogInfo info, clsSubProcessRefStage stg, clsArgumentList inputs)
            => Publish(info, () => _publisher.ProcessReferenceStarted(_sessionId, stg.Id, stg.Name));

        public override void ReadEpilogue(LogInfo info, clsReadStage stg, clsArgumentList outputs)
            => Publish(info, () => _publisher.ReadStageEnded(_sessionId, stg.Id, stg.Name));

        public override void ReadPrologue(LogInfo info, clsReadStage stg){}

        public override void RecoverEpilogue(LogInfo info, clsRecoverStage stg)
            => Publish(info, () => _publisher.RecoverStageEnded(_sessionId, stg.Id, stg.Name));

        public override void RecoverPrologue(LogInfo info, clsRecoverStage stg){}

        public override void ResumeEpilogue(LogInfo info, clsResumeStage stg)
            => Publish(info, () => _publisher.ResumeStageEnded(_sessionId, stg.Id, stg.Name));

        public override void ResumePrologue(LogInfo info, clsResumeStage stg){}

        public override void SkillEpilogue(LogInfo info, clsSkillStage stg, string objectName, string actionName, clsArgumentList outputs)
            => Publish(info, () => _publisher.SkillStageEnded(_sessionId, stg.Id, stg.Name, objectName, actionName));

        public override void SkillPrologue(LogInfo info, clsSkillStage stg, string objectName, string actionName, clsArgumentList inputs)
            => Publish(info, () => _publisher.SkillStageStarted(_sessionId, stg.Id, stg.Name, objectName, actionName));

        public override void SubSheetEpilogue(LogInfo info, clsEndStage stg, clsArgumentList outputs)
            => Publish(info, () => _publisher.SubsheetEnded(_sessionId, stg.Id, stg.Name));

        public override void SubSheetPrologue(LogInfo info, clsStartStage stg, clsArgumentList inputs)
            => Publish(info, () => _publisher.SubsheetStarted(_sessionId, stg.Id, stg.Name));

        public override void SubSheetRefEpilogue(LogInfo info, clsSubSheetRefStage stg, clsArgumentList outputs)
            => Publish(info, () => _publisher.SubsheetReferenceStageEnded(_sessionId, stg.Id, stg.Name));

        public override void SubSheetRefPrologue(LogInfo info, clsSubSheetRefStage stg, clsArgumentList inputs)
            => Publish(info, () => _publisher.SubsheetReferenceStageStarted(_sessionId, stg.Id, stg.Name));

        public override void WaitEpilogue(LogInfo info, clsWaitStartStage stg, string choiceName, int choiceNumber)
            => Publish(info, () => _publisher.WaitStageEnded(_sessionId, stg.Id, stg.Name, choiceName, choiceNumber));

        public override void WaitPrologue(LogInfo info, clsWaitStartStage stg){}

        public override void WriteEpilogue(LogInfo info, clsWriteStage stg, clsArgumentList inputs)
            => Publish(info, () => _publisher.WriteStageEnded(_sessionId, stg.Id, stg.Name));

        public override void WritePrologue(LogInfo info, clsWriteStage stg) { }

        public override void ImmediateStop(LogInfo info, string stopReason)
            => Publish(info, () => _publisher.ProcessStopped(_sessionId, stopReason));

        public override void UnexpectedException(LogInfo info)
            => Publish(info, () => _publisher.UnexpectedException(_sessionId));


    }
}
