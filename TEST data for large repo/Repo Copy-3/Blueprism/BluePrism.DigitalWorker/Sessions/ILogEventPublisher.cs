using System;

namespace BluePrism.DigitalWorker.Sessions
{
    public interface ILogEventPublisher
    {
        void ActionStageStarted(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName);
        void ActionStageEnded(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName);
        void AlertStageEnded(Guid sessionId, Guid stageId, string stageName, string alert);
        void CalculationStageEnded(Guid sessionId, Guid stageId, string stageName, string result, int dataType);
        void ChoiceStageEnded(Guid sessionId, Guid stageId, string stageName, string choiceTaken, int choiceNumber);
        void CodeStageEnded(Guid sessionId, Guid stageId, string stageName);
        void DecisionStageEnded(Guid sessionId, Guid stageId, string stageName, string result);
        void DebugDataEmitted(Guid sessionId, Guid stageId, string stageName, string message);
        void LoopStartStageEnded(Guid sessionId, Guid stageId, string stageName, string iteration);
        void LoopEndStageEnded(Guid sessionId, Guid stageId, string stageName, string iteration);
        void MultipleCalculationStageEnded(Guid sessionId, Guid stageId, string stageName);
        void NavigationStageEnded(Guid sessionId, Guid stageId, string stageName);
        void NoteStageStarted(Guid sessionId, Guid stageId, string stageName, string narrative);
        void ObjectStarted(Guid sessionId, Guid stageId, string stageName);
        void ObjectEnded(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName);
        void ProcessStageErrored(Guid sessionId, Guid stageId, string stageName, string errorMessage);
        void ProcessStarted(Guid sessionId, Guid stageId, string stageName);
        void ProcessEnded(Guid sessionId, Guid stageId, string stageName);
        void ProcessReferenceStarted(Guid sessionId, Guid stageId, string stageName);
        void ProcessReferenceEnded(Guid sessionId, Guid stageId, string stageName);
        void ProcessStopped(Guid sessionId, string stopReason);
        void ReadStageEnded(Guid sessionId, Guid stageId, string stageName);
        void RecoverStageEnded(Guid sessionId, Guid stageId, string stageName);
        void ResumeStageEnded(Guid sessionId, Guid stageId, string stageName);
        void SkillStageEnded(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName);
        void SkillStageStarted(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName);
        void SubsheetEnded(Guid sessionId, Guid stageId, string stageName);
        void SubsheetStarted(Guid sessionId, Guid stageId, string stageName);
        void SubsheetReferenceStageEnded(Guid sessionId, Guid stageId, string stageName);
        void SubsheetReferenceStageStarted(Guid sessionId, Guid stageId, string stageName);
        void UnexpectedException(Guid sessionId);
        void WaitStageEnded(Guid sessionId, Guid stageId, string stageName, string choiceTaken, int choiceNumber);
        void WriteStageEnded(Guid sessionId, Guid stageId, string stageName);
    }
}
