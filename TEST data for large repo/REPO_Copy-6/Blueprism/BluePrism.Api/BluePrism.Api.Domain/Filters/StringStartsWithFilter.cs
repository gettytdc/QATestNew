namespace BluePrism.Api.Domain.Filters
{
    public class StringStartsWithFilter : Filter<string>
    {
        public string StartsWith { get; set; }

        public StringStartsWithFilter(string startsWith)
        {
            StartsWith = startsWith;
        }

        public override int GetHashCode() => StartsWith != null ? StartsWith.GetHashCode() : 0;
    }
}
