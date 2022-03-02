namespace BluePrism.Api.Domain.Filters
{
    public class GreaterThanOrEqualToFilter<TValue> : Filter<TValue>
    {
        public TValue GreaterThanOrEqualTo { get; }

        public GreaterThanOrEqualToFilter(TValue greaterThanOrEqualTo)
        {
            GreaterThanOrEqualTo = greaterThanOrEqualTo;
        }

        public override int GetHashCode() => GreaterThanOrEqualTo != null ? GreaterThanOrEqualTo.GetHashCode() : 0;
    }
}
