namespace BluePrism.Api.Models
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class PagingTokenModelJsonConverter<T> : JsonConverter<PagingTokenModel<T>>
    {
        private static readonly IContractResolver Resolver = new NoTypeConverterContractResolver<T>();

        public override PagingTokenModel<T> ReadJson(JsonReader reader, Type objectType, PagingTokenModel<T> existingValue, bool hasExistingValue, JsonSerializer serializer) =>
            JsonSerializer.CreateDefault(new JsonSerializerSettings { ContractResolver = Resolver }).Deserialize(reader, objectType) as PagingTokenModel<T>;

        public override void WriteJson(JsonWriter writer, PagingTokenModel<T> value, JsonSerializer serializer) =>
            JsonSerializer.CreateDefault(new JsonSerializerSettings { ContractResolver = Resolver }).Serialize(writer, value);
    }

    public class NoTypeConverterContractResolver<T> : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(PagingTokenModel<T>).IsAssignableFrom(objectType))
            {
                var contract = CreateObjectContract(objectType);
                contract.Converter = null;
                return contract;
            }

            return base.CreateContract(objectType);
        }
    }
}
