namespace BluePrism.Api.Domain
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public class WorkQueue
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public QueueStatus Status { get; set; }
        public string KeyField { get; set; }
        public int MaxAttempts { get; set; }
        public bool IsEncrypted { get; set; }
        public int EncryptionKeyId { get; set; }
        public int PendingItemCount { get; set; }
        public int CompletedItemCount { get; set; }
        public int ExceptionedItemCount { get; set; }
        public int LockedItemCount { get; set; }
        public int TotalItemCount { get; set; }
        public TimeSpan AverageWorkTime { get; set; }
        public TimeSpan TotalCaseDuration { get; set; }
        public Guid ProcessId { get; set; }
        public Guid ResourceGroupId { get; set; }
        public int TargetSessionCount { get; set; }
        public string GroupName { get; set; }
        public Guid GroupId { get; set; }
        public int Ident { get; set; }

        public override bool Equals(object obj) =>
            obj is WorkQueue w
            && w.Id.Equals(Id)
            && w.Ident.Equals(Ident)
            && w.Name.Equals(Name)
            && w.KeyField.Equals(KeyField)
            && w.MaxAttempts.Equals(MaxAttempts)
            && w.IsEncrypted.Equals(IsEncrypted)
            && w.Status.Equals(Status)
            && w.EncryptionKeyId.Equals(EncryptionKeyId)
            && w.PendingItemCount.Equals(PendingItemCount)
            && w.CompletedItemCount.Equals(CompletedItemCount)
            && w.ExceptionedItemCount.Equals(ExceptionedItemCount)
            && w.LockedItemCount.Equals(LockedItemCount)
            && w.TotalItemCount.Equals(TotalItemCount)
            && w.AverageWorkTime.Equals(AverageWorkTime)
            && w.TotalCaseDuration.Equals(TotalCaseDuration)
            && w.EncryptionKeyId.Equals(EncryptionKeyId)
            && w.ProcessId.Equals(ProcessId)
            && w.ResourceGroupId.Equals(ResourceGroupId)
            && w.TargetSessionCount.Equals(TargetSessionCount)
            && w.GroupName.Equals(GroupName)
            && w.GroupId.Equals(GroupId);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() => Id.GetHashCode();
    }
}
