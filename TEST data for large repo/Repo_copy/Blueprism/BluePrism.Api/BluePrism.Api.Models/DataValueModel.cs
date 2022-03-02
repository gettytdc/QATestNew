namespace BluePrism.Api.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class DataValueModel
    {
        public object Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DataValueType ValueType { get; set; }

        [JsonIgnore]
        public bool HasBindError { get; set; }
    }
}
