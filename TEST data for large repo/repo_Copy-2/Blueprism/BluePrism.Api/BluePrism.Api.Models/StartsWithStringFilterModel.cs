namespace BluePrism.Api.Models
{
    public class StartsWithStringFilterModel : RangeFilterModel<string>
    {
        public string Strtw { get; set; }

        public override BasicFilterModel<string> GetLowestBaseFilter() =>
            string.IsNullOrWhiteSpace(Strtw)
                ? new RangeFilterModel<string> { Eq = Eq, Lte = Lte, Gte = Gte }.GetLowestBaseFilter()
                : this;
    }
}
