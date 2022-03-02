namespace BluePrism.Server.Domain.Models.DataFilters
{
    public class ContainsDataFilter : DataFilter<string>
    {
        public string ContainsValue { get; set; }

        public override bool Equals(object obj) =>
           obj is ContainsDataFilter s
               && s.ContainsValue == ContainsValue;

        public override int GetHashCode() => ContainsValue.GetHashCode();
    }
}
