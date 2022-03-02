namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using CommonTestClasses.Extensions;
    using FluentAssertions;
    using FluentAssertions.Common;
    using Models;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Utilities.Testing;

    public class CustomDataCollectionModelConverterTests : UnitTestBase<CustomDataCollectionModelConverter>
    {

        [Test]
        public void ReadJson_ShouldReturnNewDataCollectionModel_WhenGivenEmptyCollectionModel()
        {
            var dataCollectionModel = new DataCollectionModel();

            var json = JsonConvert.SerializeObject(dataCollectionModel);

            JsonReader reader = new JsonTextReader(new StringReader(json));
            while (reader.TokenType == JsonToken.None)
                if (!reader.Read())
                    break;

            var result = ClassUnderTest.ReadJson(reader, typeof(DataCollectionModel), null, JsonSerializer.CreateDefault());
            result.Should().BeOfType<DataCollectionModel>();
        }

        [TestCaseSource(nameof(DataValuesHasBindingErrorTestCases))]
        public void ReadJson_ShouldReturnExpectedDataCollectionModel_WhenGivenCollectionModel(DataValueTestModel testModel)
        {
            var expectedRowsData = new List<IReadOnlyDictionary<string, DataValueModel>>();
            var row = new Dictionary<string, DataValueModel>
            {
                { testModel.Key, new DataValueModel() { Value = testModel.Value, ValueType = testModel.ValueType }}
            };

            expectedRowsData.Add(row);
            var expectedRows = new ReadOnlyCollection<IReadOnlyDictionary<string, DataValueModel>>(expectedRowsData);

            var expectedDataCollectionModel = new DataCollectionModel
            {
                Rows = expectedRows
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

            var result = (DataCollectionModel) ClassUnderTest.ReadJson(reader, typeof(DataCollectionModel), null, JsonSerializer.CreateDefault());
            result.Rows.Single().Values.Single().HasBindError.Should().Be(testModel.ExpectedBindErrorResult);
            result.Rows.Single().Values.Single().Value.Should().IsSameOrEqualTo(testModel.ExpectedResultObject);
        }

        private static IEnumerable<TestCaseData> DataValuesHasBindingErrorTestCases() =>
            new[]
            {
                new DataValueTestModel("KeyTime", "2020-01-12T15:47:57", DataValueType.DateTime, false, new DateTimeOffset(2020,1,12,15,47,57,TimeSpan.Zero)),
                new DataValueTestModel("KeyTime", "200", DataValueType.DateTime, true, DateTimeOffset.MinValue),
                new DataValueTestModel("KeyTime", "2020-01-12T15:47:57+02:00", DataValueType.DateTime, false, new DateTimeOffset(2020,1,12,15,47,57,TimeSpan.FromHours(2))),
                new DataValueTestModel("KeyTime", "2020-01-12", DataValueType.Date, false, new DateTime(2020,1,12,0,0,0)),
                new DataValueTestModel("KeyTime", "2020-01-12T15:47:57", DataValueType.Date, false, new DateTime(2020,1,12,0,0,0)),
                new DataValueTestModel("KeyTime", "ddd", DataValueType.Date, true, DateTime.MinValue),
                new DataValueTestModel("KeyTime", "15:47:57+02:00", DataValueType.Time, false, new DateTimeOffset(1,1,1,15,47,57,TimeSpan.FromHours(2))),
                new DataValueTestModel("KeyTime", "15:47:57", DataValueType.Time, false, new DateTimeOffset(1,1,1,15,47,57,TimeSpan.Zero)),
                new DataValueTestModel("KeyTime", "ddd", DataValueType.Time, true, DateTimeOffset.MinValue),
                new DataValueTestModel("KeyBinary", Convert.ToBase64String(new byte[] {0x01, 0x02, 0x03}), DataValueType.Binary, false, new byte[] {0x01, 0x02, 0x03}),
                new DataValueTestModel("KeyBinary", "444", DataValueType.Binary, true, new byte[] {}),
                new DataValueTestModel("KeyFlag", "True", DataValueType.Flag, false, true),
                new DataValueTestModel("KeyFlag", "False", DataValueType.Flag, false, false),
                new DataValueTestModel("KeyFlag", "fail", DataValueType.Flag, true, false),
                new DataValueTestModel("KeyNumber", "20", DataValueType.Number, false, 20M),
                new DataValueTestModel("KeyNumber", "50.5", DataValueType.Number, false, 50.5M),
                new DataValueTestModel("KeyNumber", "decimal", DataValueType.Number, true, default(decimal)),
                new DataValueTestModel("KeyText", string.Empty, DataValueType.Text, false, string.Empty),
                new DataValueTestModel("KeyText", null, DataValueType.Text, false, string.Empty),
                new DataValueTestModel("KeyText", "testText", DataValueType.Text, false, "testText"),
                new DataValueTestModel("KeyTimeSpan", "6", DataValueType.TimeSpan, false, new TimeSpan(6,0,0,0)),
                new DataValueTestModel("KeyTimeSpan", "6:12:14", DataValueType.TimeSpan, false, new TimeSpan(0,6,12,14)),
                new DataValueTestModel("KeyTimeSpan", "6.12:14:56,322", DataValueType.TimeSpan, true, default(TimeSpan)),
                new DataValueTestModel("KeyPassword", string.Empty, DataValueType.Password, true, string.Empty),
                new DataValueTestModel("KeyPassword", null, DataValueType.Password, true, string.Empty),
                new DataValueTestModel("KeyPassword", "pass", DataValueType.Password, false, "pass"),
                new DataValueTestModel("KeyCollectionData", GetInputDataCollection(), DataValueType.Collection, false, GetInputDataCollection()),
            }.ToTestCaseData();


        private static DataCollectionModel GetInputDataCollection()
        {
            var nestedCollectionRows = new List<IReadOnlyDictionary<string, DataValueModel>>();
            var nestedDictionary = new Dictionary<string, DataValueModel>
            {
                {"Nested Key 1", new DataValueModel() {Value = 1, ValueType = DataValueType.Number}}
            };
            nestedCollectionRows.Add(nestedDictionary);
            var nestedCollectionModel = new DataCollectionModel() {Rows = nestedCollectionRows};

            var dictionary = new List<IReadOnlyDictionary<string, DataValueModel>>
            {
                new Dictionary<string, DataValueModel>()
                {
                    {
                        "Key1",
                        new DataValueModel() {Value = nestedCollectionModel, ValueType = DataValueType.Collection}
                    },
                }
            };

            return new DataCollectionModel {Rows = dictionary};
        }
    }
}
