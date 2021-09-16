namespace BluePrism.Api.Domain
{
    using System.Collections.Generic;

    public sealed class DataCollection
    {
        public IReadOnlyCollection<IReadOnlyDictionary<string, DataValue>> Rows { get; set; }
    }
}
