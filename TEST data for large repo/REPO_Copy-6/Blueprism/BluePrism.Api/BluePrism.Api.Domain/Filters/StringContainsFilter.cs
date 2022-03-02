namespace BluePrism.Api.Domain.Filters
{
    public class StringContainsFilter : Filter<string>
    {
        public string Contains { get; }

        public StringContainsFilter(string contains)
        {
            Contains = contains;
        }

        public override int GetHashCode() => Contains != null ? Contains.GetHashCode() : 0;
    }
}
