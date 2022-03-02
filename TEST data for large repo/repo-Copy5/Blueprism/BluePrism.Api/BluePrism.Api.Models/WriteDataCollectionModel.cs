namespace BluePrism.Api.Models
{
    using System.Collections.Generic;

    public class WriteDataCollectionModel
    {
        public IReadOnlyCollection<IReadOnlyDictionary<string, WriteDataValueModel>> Rows { get; set; }
    }
}
