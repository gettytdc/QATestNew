namespace BluePrism.Api.Models
{
    using System;
    using System.Collections.Generic;

    public class WorkQueueItemModel
    {
        public Guid Id { get; set; }
        public int Priority { get; set; }
        public long Ident { get; set; }
        public WorkQueueItemState State { get; set; }
        public string KeyValue { get; set; }
        public string Status { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
        public int AttemptNumber { get; set; }
        public DateTimeOffset? LoadedDate { get; set; }
        public DateTimeOffset? DeferredDate { get; set; }
        public DateTimeOffset? LockedDate { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public DateTimeOffset? ExceptionedDate { get; set; }
        public DateTimeOffset? LastUpdated { get; set; }
        public int WorkTimeInSeconds { get; set; }
        public int AttemptWorkTimeInSeconds { get; set; }
        public DataCollectionModel Data { get; set; }
        public string ExceptionReason { get; set; }
        public string Resource { get; set; }

        private bool Equals(WorkQueueItemModel other) =>
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


        public override bool Equals(object obj) => ReferenceEquals(this, obj) || (obj is WorkQueueItemModel other && Equals(other));
        public override int GetHashCode() => Id.GetHashCode();
    }
}
