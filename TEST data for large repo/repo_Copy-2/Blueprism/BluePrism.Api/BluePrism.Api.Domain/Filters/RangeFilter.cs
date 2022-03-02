namespace BluePrism.Api.Domain.Filters
{
    public class RangeFilter<TValue> : Filter<TValue>
    {
        public TValue GreaterThanOrEqualTo { get; }
        public TValue LessThanOrEqualTo { get; }

        public RangeFilter(TValue greaterThanOrEqualTo, TValue lessThanOrEqualTo)
        {
            GreaterThanOrEqualTo = greaterThanOrEqualTo;
            LessThanOrEqualTo = lessThanOrEqualTo;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (GreaterThanOrEqualTo != null ? GreaterThanOrEqualTo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LessThanOrEqualTo != null ? LessThanOrEqualTo.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
