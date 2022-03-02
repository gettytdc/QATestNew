using System;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Messages.Events;
using BluePrism.DigitalWorker.Messages.Events.Factory;
using BluePrism.DigitalWorker.Messages.Events.LogEntryData;
using BluePrism.DigitalWorker.Messaging;

namespace BluePrism.DigitalWorker.Sessions
{
    public class LogEventPublisher : ILogEventPublisher
    {
        private readonly IMessageBusWrapper _bus;
        private int _nextEntryNumber = 1;
        private readonly ISystemClock _clock;
        
        public LogEventPublisher(IMessageBusWrapper bus, ISystemClock clock)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        private int GetEntryNumber() => _nextEntryNumber++;

        private void Publish(Guid sessionId, Guid? stageId, string stageName, ProcessStageLogEntryType entryType, ProcessLogEntryData data)
        {
            var message = DigitalWorkerEvents.ProcessStageLogged(sessionId, GetEntryNumber(), _clock.Now, stageId, stageName, entryType, data);
            _bus.Publish(message).Wait();
        }

        public void ActionStageEnded(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ActionStageEnded, new ActionLogEntryData { ObjectName = objectName, ActionName = actionName });

        public void ActionStageStarted(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName)
             => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ActionStageStarted, new ActionLogEntryData { ObjectName = objectName, ActionName = actionName });

        public void AlertStageEnded(Guid sessionId, Guid stageId, string stageName, string alert)
             => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.AlertStageEnded, new AlertLogEntryData { Alert = alert });

        public void CalculationStageEnded(Guid sessionId, Guid stageId, string stageName, string result, int dataType)
             => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.CalculationStageEnded, new CalculationLogEntryData { Result = result, DataType = dataType});

        public void ChoiceStageEnded(Guid sessionId, Guid stageId, string stageName, string choiceTaken, int choiceNumber)
             => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ChoiceStageEnded, new ChoiceLogEntryData{ ChoiceTaken = choiceTaken, ChoiceNumber = choiceNumber });

        public void CodeStageEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.CodeStageEnded, new EmptyLogEntryData());

        public void DebugDataEmitted(Guid sessionId, Guid stageId, string stageName, string message)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.DebugDataEmitted, new DebugLogEntryData { Message = message });

        public void DecisionStageEnded(Guid sessionId, Guid stageId, string stageName, string result)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.DecisionStageEnded, new DecisionLogEntryData { Result = result });

        public void LoopEndStageEnded(Guid sessionId, Guid stageId, string stageName, string iteration)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.LoopEndStageEnded, new LoopLogEntryData { Iteration = iteration});

        public void LoopStartStageEnded(Guid sessionId, Guid stageId, string stageName, string iteration)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.LoopStartStageEnded, new LoopLogEntryData { Iteration = iteration});

        public void MultipleCalculationStageEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.MultipleCalculationStageEnded, new EmptyLogEntryData());

        public void NavigationStageEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.NavigationStageEnded, new EmptyLogEntryData());

        public void NoteStageStarted(Guid sessionId, Guid stageId, string stageName, string narrative)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.NoteStageStarted, new NoteLogEntryData { Narrative = narrative });

        public void ObjectEnded(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ObjectEnded, new ActionLogEntryData { ObjectName = objectName, ActionName = actionName });

        public void ObjectStarted(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ObjectStarted, new EmptyLogEntryData());

        public void ProcessEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ProcessEnded, new EmptyLogEntryData());

        public void ProcessReferenceEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ProcessReferenceEnded, new EmptyLogEntryData());

        public void ProcessReferenceStarted(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ProcessReferenceStarted, new EmptyLogEntryData());

        public void ProcessStageErrored(Guid sessionId, Guid stageId, string stageName, string errorMessage)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ProcessStageErrored, new ErrorLogEntryData { ErrorMessage = errorMessage });

        public void ProcessStarted(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ProcessStarted, new EmptyLogEntryData());

        public void ProcessStopped(Guid sessionId, string stopReason)
            => Publish(sessionId, null, string.Empty, ProcessStageLogEntryType.ProcessStopped, new ProcessStoppedLogEntryData() { StopReason = stopReason });

        public void ReadStageEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ReadStageEnded, new EmptyLogEntryData());

        public void RecoverStageEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.RecoverStageEnded, new EmptyLogEntryData());

        public void ResumeStageEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.ResumeStageEnded, new EmptyLogEntryData());

        public void SkillStageEnded(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.SkillStageEnded, new ActionLogEntryData { ObjectName = objectName, ActionName = actionName });

        public void SkillStageStarted(Guid sessionId, Guid stageId, string stageName, string objectName, string actionName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.SkillStageStarted, new ActionLogEntryData { ObjectName = objectName, ActionName = actionName });

        public void SubsheetEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.SubsheetEnded, new EmptyLogEntryData());

        public void SubsheetReferenceStageEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.SubsheetReferenceStageEnded, new EmptyLogEntryData());

        public void SubsheetReferenceStageStarted(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.SubsheetReferenceStageStarted, new EmptyLogEntryData());

        public void SubsheetStarted(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.SubsheetStarted, new EmptyLogEntryData());

        public void UnexpectedException(Guid sessionId)
            => Publish(sessionId, null, string.Empty, ProcessStageLogEntryType.UnexpectedException, new EmptyLogEntryData());

        public void WaitStageEnded(Guid sessionId, Guid stageId, string stageName, string choiceTaken, int choiceNumber)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.WaitStageEnded, new WaitLogEntryData { ChoiceName = choiceTaken, ChoiceNumber = choiceNumber });

        public void WriteStageEnded(Guid sessionId, Guid stageId, string stageName)
            => Publish(sessionId, stageId, stageName, ProcessStageLogEntryType.WriteStageEnded, new EmptyLogEntryData());
        
    }
}
