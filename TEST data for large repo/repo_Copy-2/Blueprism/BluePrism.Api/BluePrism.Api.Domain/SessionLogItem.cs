namespace BluePrism.Api.Domain
{
    using System;
    using Func;

    public class SessionLogItem
    {
        public long LogId { get; set; }
        public string StageName { get; set; }
        public StageTypes StageType { get; set; }
        public string Result { get; set; }
        public Option<DateTimeOffset> ResourceStartTime { get; set; }
        public bool HasParameters { get; set; }

        public override bool Equals(object obj) =>
            obj is SessionLogItem i
            && i.StageName.Equals(StageName)
            && i.StageType.Equals(StageType)
            && i.Result.Equals(Result)
            && i.ResourceStartTime.Equals(ResourceStartTime);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (StageName != null ? StageName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ StageType.GetHashCode();
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ResourceStartTime.GetHashCode();
                return hashCode;
            }
        }
    }
}
