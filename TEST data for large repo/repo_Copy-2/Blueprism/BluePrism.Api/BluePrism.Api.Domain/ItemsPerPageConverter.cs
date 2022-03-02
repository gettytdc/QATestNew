namespace BluePrism.Api.Domain
{
    using System;
    using Newtonsoft.Json;

    internal class ItemsPerPageConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue((int)((ItemsPerPage)value));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();

        public override bool CanConvert(Type objectType) => true;
    }
}
