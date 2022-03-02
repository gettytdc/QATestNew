namespace BluePrism.Api.Models
{
    public class StartsWithOrContainsStringFilterModel : StartsWithStringFilterModel
    {
        public string Ctn { get; set; }

        public override BasicFilterModel<string> GetLowestBaseFilter() =>
            string.IsNullOrWhiteSpace(Ctn)
                ? new StartsWithStringFilterModel { Eq = Eq, Lte = Lte, Gte = Gte, Strtw = Strtw }.GetLowestBaseFilter()
                : this;
    }
}
