namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class StartsWithDataFilter : DataFilter<string>
    {
        public string StartsWith { get; set; }

        public override bool Equals(object obj) =>
            obj is StartsWithDataFilter s
                && s.StartsWith == StartsWith;

        public override int GetHashCode() => StartsWith.GetHashCode();

    }
}
