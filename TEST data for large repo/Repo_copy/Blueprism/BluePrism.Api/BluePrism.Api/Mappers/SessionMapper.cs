namespace BluePrism.Api.Mappers
{
    using System;
    using Domain;
    using Models;
    using Func;

    public static class SessionMapper
    {
        public static SessionModel ToModelObject(this Session @this) =>
           new SessionModel
           {
               SessionId = @this.SessionId,
               SessionNumber = @this.SessionNumber,
               ProcessId = @this.ProcessId,
               ProcessName = @this.ProcessName,
               UserName = @this.UserName,
               ResourceId = @this.ResourceId,
               ResourceName = @this.ResourceName,
               Status = @this.Status.ToModel(),
               StartTime = @this.StartTime is Some<DateTimeOffset> startDate ? startDate.Value : (DateTimeOffset?)null,
               EndTime = @this.EndTime is Some<DateTimeOffset> endDate ? endDate.Value : (DateTimeOffset?)null,
               StageStarted = @this.StageStarted is Some<DateTimeOffset> stageDate ? stageDate.Value : (DateTimeOffset?)null,
               LatestStage = @this.LatestStage,
               ExceptionMessage = @this.ExceptionMessage,
               ExceptionType = @this.ExceptionType is Some<string> exceptionType ? exceptionType.Value : null,
               TerminationReason = @this.TerminationReason.ToModel()
           };

        private static Models.SessionStatus ToModel(this Domain.SessionStatus sessionDbStatus)
        {
            switch (sessionDbStatus)
            {
                case Domain.SessionStatus.Completed:
                    return Models.SessionStatus.Completed;
                case Domain.SessionStatus.Terminated:
                    return Models.SessionStatus.Terminated;
                case Domain.SessionStatus.Pending:
                    return Models.SessionStatus.Pending;
                case Domain.SessionStatus.Running:
                    return Models.SessionStatus.Running;
                case Domain.SessionStatus.Warning:
                    return Models.SessionStatus.Warning;
                case Domain.SessionStatus.Stopping:
                    return Models.SessionStatus.Stopping;
                case Domain.SessionStatus.Stopped:
                    return Models.SessionStatus.Stopped;
                default:
                    throw new ArgumentException("Unexpected session status", nameof(sessionDbStatus));
            }
        }

        private static Models.SessionTerminationReason ToModel(this Domain.SessionTerminationReason sessionDbTerminationReason)
        {
            switch (sessionDbTerminationReason)
            {
                case Domain.SessionTerminationReason.None:
                    return Models.SessionTerminationReason.None;
                case Domain.SessionTerminationReason.InternalError:
                    return Models.SessionTerminationReason.InternalError;
                case Domain.SessionTerminationReason.ProcessError:
                    return Models.SessionTerminationReason.ProcessError;
                default:
                    throw new ArgumentException("Unexpected session termination reason", nameof(sessionDbTerminationReason));
            }
        }
    }
}
