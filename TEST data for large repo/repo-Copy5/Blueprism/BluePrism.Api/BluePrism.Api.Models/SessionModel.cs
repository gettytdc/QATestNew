namespace BluePrism.Api.Models
{
    using System;

    public class SessionModel
    {
        public Guid SessionId { get; set; }
        public int SessionNumber { get; set; }
        public Guid ProcessId { get; set; }
        public string ProcessName { get; set; }
        public Guid ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string UserName { get; set; }
        public SessionStatus Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string LatestStage { get; set; }
        public DateTimeOffset? StageStarted { get; set; }
        public string ExceptionMessage { get; set; }
        public SessionTerminationReason TerminationReason { get; set; }
        public string ExceptionType { get; set; }
    }
}
