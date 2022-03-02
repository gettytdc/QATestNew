namespace BluePrism.Api.Domain
{
    using System;
    using System.Collections.Generic;
    using Func;

    public sealed class WorkQueueItem
    {
        public Guid Id { get; set; }
        public int Priority { get; set; }
        public long Ident { get; set; }
        public WorkQueueItemState State { get; set; }
        public string KeyValue { get; set; }
        public string Status { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
        public int AttemptNumber { get; set; }
        public Option<DateTimeOffset> LoadedDate { get; set; }
        public Option<DateTimeOffset> DeferredDate { get; set; }
        public Option<DateTimeOffset> LockedDate { get; set; }
        public Option<DateTimeOffset> CompletedDate { get; set; }
        public Option<DateTimeOffset> ExceptionedDate { get; set; }
        public Option<DateTimeOffset> LastUpdated { get; set; }
        public int WorkTimeInSeconds { get; set; }
        public int AttemptWorkTimeInSeconds { get; set; }
        public DataCollection Data { get; set; }
        public string ExceptionReason { get; set; }
        public string Resource { get; set; }

        private bool Equals(WorkQueueItem other) =>
            Id.Equals(other.Id)
            && Priority == other.Priority
            && Ident == other.Ident
            && State == other.State
            && KeyValue == other.KeyValue
            && Status == other.Status
            && Equals(Tags, other.Tags)
            && AttemptNumber == other.AttemptNumber
            && LoadedDate.Equals(other.LoadedDate)
            && DeferredDate.Equals(other.DeferredDate)
            && LockedDate.Equals(other.LockedDate)
            && CompletedDate.Equals(other.CompletedDate)
            && ExceptionedDate.Equals(other.ExceptionedDate)
            && LastUpdated.Equals(other.LastUpdated)
            && WorkTimeInSeconds == other.WorkTimeInSeconds
            && AttemptWorkTimeInSeconds == other.AttemptWorkTimeInSeconds
            && ExceptionReason == other.ExceptionReason
            && Resource == other.Resource;

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || (obj is WorkQueueItem other && Equals(other));
        public override int GetHashCode() => Id.GetHashCode();

    }
}
