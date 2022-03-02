namespace BluePrism.Api.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class CreateWorkQueueItemModel
    {
        [JsonConverter(typeof(CustomDataCollectionModelConverter))]
        public DataCollectionModel Data { get; set; }
        public DateTimeOffset? DeferredDate { get; set; }
        public int Priority { get; set; }
        public IReadOnlyCollection<string> Tags { get; set; }
        public string Status { get; set; }
    }
}
