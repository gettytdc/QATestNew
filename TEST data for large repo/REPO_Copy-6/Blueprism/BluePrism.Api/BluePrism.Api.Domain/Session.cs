namespace BluePrism.Api.Domain
{
    using System;
    using Func;

    public class Session
    {
        public Guid SessionId { get; set; }
        public int SessionNumber { get; set; }
        public Guid ProcessId { get; set; }
        public string ProcessName { get; set; }
        public Guid ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string UserName { get; set; }
        public SessionStatus Status { get; set; }
        public Option<DateTimeOffset> StartTime { get; set; }
        public Option<DateTimeOffset> EndTime { get; set; }
        public Option<DateTimeOffset> StageStarted { get; set; }
        public string LatestStage { get; set; }
        public string ExceptionMessage { get; set; }
        public SessionTerminationReason TerminationReason { get; set; }
        public Option<string> ExceptionType { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (SessionNumber != null ? SessionNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ProcessId.GetHashCode();
                hashCode = (hashCode * 397) ^ (ProcessName != null ? ProcessName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ResourceId != null ? ResourceId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ResourceName != null ? ResourceName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UserName != null ? ResourceName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Status.GetHashCode();
                hashCode = (hashCode * 397) ^ (StartTime != null ? StartTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EndTime != null ? EndTime.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StageStarted != null ? StageStarted.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LatestStage != null ? LatestStage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExceptionMessage != null ? ExceptionMessage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TerminationReason.GetHashCode();
                hashCode = (hashCode * 397) ^ (ExceptionType != null ? ExceptionType.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}
