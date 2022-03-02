namespace BluePrism.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class CustomDataCollectionModelConverter : JsonConverter
    {
        private static readonly
            Dictionary<DataValueType,
                Func<KeyValuePair<string, WriteDataValueModel>, KeyValuePair<string, DataValueModel>>>
            MapToDictionaryKeyAndDataValueModel =
                new Dictionary<DataValueType,
                    Func<KeyValuePair<string, WriteDataValueModel>, KeyValuePair<string, DataValueModel>>>()
        {
            {DataValueType.Date, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToDate())},
            {DataValueType.DateTime, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToDateTime())},
            {DataValueType.Time, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToTime())},
            {DataValueType.Flag, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToBoolean())},
            {DataValueType.Binary, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToByteArray())},
            {DataValueType.Number, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToDecimal())},
            {DataValueType.Text, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToText())},
            {DataValueType.Password, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToPassword())},
            {DataValueType.TimeSpan, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToTimeSpan())},
            {DataValueType.Image, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToBitMap())},
            {DataValueType.Collection, x => CreateMappedKeyValuePair(x,(writeDataValueModel) => writeDataValueModel.MapToCollection())},
        };

        private static KeyValuePair<string, DataValueModel> CreateMappedKeyValuePair(
            KeyValuePair<string, WriteDataValueModel> keyValuePairToMap,
            Func<WriteDataValueModel, DataValueModel> func) =>
            new KeyValuePair<string, DataValueModel>(keyValuePairToMap.Key, func(keyValuePairToMap.Value));

        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader, new JsonLoadSettings());

            if (!jObject.HasValues)
            {
                return new DataCollectionModel();
            }
            
            var property = jObject.Properties().SingleOrDefault();

            if (property == null)
            {
                return new DataCollectionModel();
            }

            return MapToDataCollectionModel(existingValue, property);
        }

        private static object MapToDataCollectionModel(object existingValue, JProperty property)
        {
            var collectionOfDictionariesWithValuesAsTypeOfString = DeserializeJson(property);

            if (collectionOfDictionariesWithValuesAsTypeOfString == null)
            {
                return new DataCollectionModel();
            }

            var mappedDictionary = MapToDictionaries(collectionOfDictionariesWithValuesAsTypeOfString);
            var dataCollectionModel = existingValue as DataCollectionModel ?? new DataCollectionModel();
            dataCollectionModel.Rows = mappedDictionary;
            return dataCollectionModel;
        }

        private static IReadOnlyCollection<IReadOnlyDictionary<string, WriteDataValueModel>> DeserializeJson(JProperty property) => JsonConvert.DeserializeObject<IReadOnlyCollection<IReadOnlyDictionary<string, WriteDataValueModel>>>(property.Value.ToString());

        private static List<Dictionary<string, DataValueModel>> MapToDictionaries(IReadOnlyCollection<IReadOnlyDictionary<string, WriteDataValueModel>> collectionOfDictionariesWithValuesAsTypeOfString)
        {
            var result = new List<Dictionary<string, DataValueModel>>();
            foreach (var row in collectionOfDictionariesWithValuesAsTypeOfString)
            {
                var dictionary = new Dictionary<string, DataValueModel>();

                foreach (var writeDataValueModel in row)
                {
                    if (!MapToDictionaryKeyAndDataValueModel.ContainsKey(writeDataValueModel.Value.ValueType))
                    {
                        throw new ArgumentException("Value was not of a recognized type");
                    }

                    var mappedKeyValuePair = MapToDictionaryKeyAndDataValueModel[writeDataValueModel.Value.ValueType](writeDataValueModel);
                    dictionary.Add(mappedKeyValuePair.Key, mappedKeyValuePair.Value);
                }

                result.Add(dictionary);
            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => serializer.Serialize(writer, value);
    }
}
