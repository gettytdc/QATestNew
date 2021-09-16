namespace BluePrism.Api.Models
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class CustomCreateWorkQueueItemValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
                

            if (reader.TokenType == JsonToken.StartObject)
            {
                var jObject = JObject.Load(reader, new JsonLoadSettings());
                return jObject.ToString();
            }

            return reader.Value?.ToString();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => serializer.Serialize(writer, value);
    }
}
