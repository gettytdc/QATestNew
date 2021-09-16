namespace BluePrism.Api.Domain.Filters
{
    public class LessThanOrEqualToFilter<TValue> : Filter<TValue>
    {
        public TValue LessThanOrEqualTo { get; }

        public LessThanOrEqualToFilter(TValue lessThanOrEqualTo)
        {
            LessThanOrEqualTo = lessThanOrEqualTo;
        }

        public override int GetHashCode() => LessThanOrEqualTo != null ? LessThanOrEqualTo.GetHashCode() : 0;
    }
}
