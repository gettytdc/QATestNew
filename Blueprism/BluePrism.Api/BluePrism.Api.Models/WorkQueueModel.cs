namespace BluePrism.Api.Models
{
    using System;

    public class WorkQueueModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public QueueStatus Status { get; set; }
        public bool IsEncrypted { get; set; }
        public string KeyField { get; set; }
        public int MaxAttempts { get; set; }
        public int PendingItemCount { get; set; }
        public int CompletedItemCount { get; set; }
        public int LockedItemCount { get; set; }
        public int ExceptionedItemCount { get; set; }
        public int TotalItemCount { get; set; }
        public TimeSpan AverageWorkTime { get; set; }
        public TimeSpan TotalCaseDuration { get; set; }
        public string GroupName { get; set; }
        public Guid GroupId { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return obj is WorkQueueModel w
                && w.Id.Equals(Id)
                && string.Equals(w.Name, Name)
                && string.Equals(w.KeyField, KeyField)
                && w.MaxAttempts.Equals(MaxAttempts)
                && w.Status.Equals(Status)
                && w.PendingItemCount.Equals(PendingItemCount)
                && w.CompletedItemCount.Equals(CompletedItemCount)
                && w.LockedItemCount.Equals(LockedItemCount)
                && w.ExceptionedItemCount.Equals(ExceptionedItemCount)
                && w.TotalItemCount.Equals(TotalItemCount)
                && w.AverageWorkTime.Equals(AverageWorkTime)
                && w.TotalCaseDuration.Equals(TotalCaseDuration)
                && string.Equals(w.GroupName, GroupName)
                && w.GroupId.Equals(GroupId);
        }

        public override int GetHashCode() => Id.GetHashCode();
    }


}
