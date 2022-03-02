namespace BluePrism.Api.Models
{
    using System;

    public class SessionLogItemModel
    {
        public long LogId { get; set; }
        public string StageName { get; set; }
        public StageTypes StageType { get; set; }
        public string Result { get; set; }
        public DateTimeOffset? ResourceStartTime { get; set; }
        public bool HasParameters { get; set; }

        public override bool Equals(object obj) =>
            obj is SessionLogItemModel m
                && Equals(m);

        protected bool Equals(SessionLogItemModel other) => StageName == other.StageName && StageType == other.StageType && Result == other.Result && ResourceStartTime.Equals(other.ResourceStartTime);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (StageName != null ? StageName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) StageType;
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ResourceStartTime.GetHashCode();
                return hashCode;
            }
        }
    }
}
