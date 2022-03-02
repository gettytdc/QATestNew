namespace BluePrism.Api.Domain.Filters
{
    public class EqualsFilter<TValue> : Filter<TValue>
    {
        public TValue EqualTo { get; }

        public EqualsFilter(TValue equalTo)
        {
            EqualTo = equalTo;
        }

         public override int GetHashCode() => EqualTo != null ? EqualTo.GetHashCode() : 0;
    }
}
