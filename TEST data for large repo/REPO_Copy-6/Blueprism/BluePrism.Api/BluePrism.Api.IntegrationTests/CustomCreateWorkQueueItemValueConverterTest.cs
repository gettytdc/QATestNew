namespace BluePrism.Api.IntegrationTests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using FluentAssertions;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class CustomCreateWorkQueueItemValueConverterTest : UnitTestBase<CustomCreateWorkQueueItemValueConverter>
    {
        [Test]
        public void ReadJson_ShouldReturnJObjectValue_WhenGivenCollection()
        {
            var itemValue = new DataValueModel
            {
                Value = "testString",
                ValueType = DataValueType.Text
            };

            var expectedRowsData = new List<IReadOnlyDictionary<string, DataValueModel>>();
            var row = new Dictionary<string, DataValueModel>()
            {
                { "TestKey", itemValue }
            };

            expectedRowsData.Add(row);
            IReadOnlyCollection<IReadOnlyDictionary<string, DataValueModel>> dictionary = new ReadOnlyCollection<IReadOnlyDictionary<string, DataValueModel>>(expectedRowsData);

            var expectedDataCollectionModel = new DataCollectionModel
            {
                Rows = dictionary
            };

            var json = JsonConvert.SerializeObject(expectedDataCollectionModel);
            JsonReader reader = new JsonTextReader(new StringReader(json));
            while (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                {
                    break;
                }
            }

            var expectedJObject = JObject.FromObject(expectedDataCollectionModel);
            var result = ClassUnderTest.ReadJson(reader, typeof(DataCollectionModel), null, JsonSerializer.CreateDefault());
            result.ShouldBeEquivalentTo(expectedJObject.ToString());
        }

        [Test]
        public void ReadJson_ShouldReturn_String_WhenGivenDataValueModelValue()
        {
            var itemValue = new DataValueModel
            {
                Value = "testString",
                ValueType = DataValueType.Text
            };

            var json = JsonConvert.SerializeObject(itemValue.Value);
            JsonReader reader = new JsonTextReader(new StringReader(json));
            while (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                {
                    break;
                }
            }

            var result = ClassUnderTest.ReadJson(reader, typeof(DataValueModel), null, JsonSerializer.CreateDefault());
            result.ShouldBeEquivalentTo(reader.Value?.ToString());
        }
    }
}
