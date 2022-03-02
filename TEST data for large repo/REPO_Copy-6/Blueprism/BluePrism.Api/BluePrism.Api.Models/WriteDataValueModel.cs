namespace BluePrism.Api.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class WriteDataValueModel
    {
        [JsonConverter(typeof(CustomCreateWorkQueueItemValueConverter))] 
        public string Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DataValueType ValueType { get; set; }

    }
}
