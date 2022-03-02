namespace BluePrism.Api.Domain
{
    using System;

    public class Resource
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid PoolId { get; set; }
        public string PoolName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public ResourceAttribute Attributes { get; set; }
        public int ActiveSessionCount { get; set; }
        public int WarningSessionCount { get; set; }
        public int PendingSessionCount { get; set; }
        public ResourceDbStatus DatabaseStatus { get; set; }
        public ResourceDisplayStatus DisplayStatus { get; set; }

        public override bool Equals(object obj) =>
            obj is Resource other
            && Id.Equals(other.Id)
            && Name == other.Name
            && PoolId.Equals(other.PoolId)
            && PoolName == other.PoolName
            && GroupId.Equals(other.GroupId)
            && GroupName == other.GroupName
            && Attributes == other.Attributes
            && ActiveSessionCount == other.ActiveSessionCount
            && WarningSessionCount == other.WarningSessionCount
            && PendingSessionCount == other.PendingSessionCount
            && DatabaseStatus == other.DatabaseStatus
            && DisplayStatus == other.DisplayStatus;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PoolId.GetHashCode();
                hashCode = (hashCode * 397) ^ (PoolName != null ? PoolName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ GroupId.GetHashCode();
                hashCode = (hashCode * 397) ^ (GroupName != null ? GroupName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Attributes;
                hashCode = (hashCode * 397) ^ ActiveSessionCount;
                hashCode = (hashCode * 397) ^ WarningSessionCount;
                hashCode = (hashCode * 397) ^ PendingSessionCount;
                hashCode = (hashCode * 397) ^ (int) DatabaseStatus;
                hashCode = (hashCode * 397) ^ (int) DisplayStatus;
                return hashCode;
            }
        }
    }
}
