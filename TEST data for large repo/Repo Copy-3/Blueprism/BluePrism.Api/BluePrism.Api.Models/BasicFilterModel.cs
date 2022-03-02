namespace BluePrism.Api.Models
{
    public class BasicFilterModel<TValue>
    {
        public TValue Eq { get; set; }

        public virtual BasicFilterModel<TValue> GetLowestBaseFilter() =>
            this;
    }
}
