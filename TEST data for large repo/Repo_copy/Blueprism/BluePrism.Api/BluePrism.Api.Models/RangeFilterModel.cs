namespace BluePrism.Api.Models
{
    public class RangeFilterModel<TValue> : BasicFilterModel<TValue>
    {
        public TValue Gte { get; set; }
        public TValue Lte { get; set; }

        public override BasicFilterModel<TValue> GetLowestBaseFilter() =>
            Gte == null && Lte == null
                ? new BasicFilterModel<TValue> { Eq = Eq }.GetLowestBaseFilter()
                : this;
    }
}
