namespace BluePrism.Api.Models
{
    using System.Collections.Generic;

    public class DataCollectionModel
    {
        public IReadOnlyCollection<IReadOnlyDictionary<string, DataValueModel>> Rows { get; set; }
    }
}
