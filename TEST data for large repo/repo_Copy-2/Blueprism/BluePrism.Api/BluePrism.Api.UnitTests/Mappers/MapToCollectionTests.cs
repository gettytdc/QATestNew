namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Models;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class MapToCollectionTests : UnitTestBase<WriteDataValueModel>
    {
        [TestCaseSource(nameof(NestedCollectionTestCaseSource))]
        public void MapToCollection_WithNestedCollection_ShouldMapAsExpected(DataValueType dataValueType, string key, object dataValue)
        {
            
            var dataCollectionModel = CreateDataCollectionModel(dataValueType, key, dataValue);

            ClassUnderTest.Value = JsonConvert.SerializeObject(dataCollectionModel);
            ClassUnderTest.ValueType = DataValueType.Collection;
            
            var result = ClassUnderTest.MapToCollection();

            result.ValueType.Should().Be(DataValueType.Collection);
            var resultDataCollectionModel = (DataCollectionModel)result.Value;

            var firstCollection = resultDataCollectionModel.Rows.First();
            firstCollection.Keys.First().Should().Be("Key1");
            firstCollection.Values.First().ValueType.Should().Be(DataValueType.Collection);

            var secondCollection = ((DataCollectionModel)firstCollection.Values.First().Value).Rows.First();
            secondCollection.Keys.First().Should().Be(key);
            secondCollection.Values.First().ValueType.Should().Be(dataValueType);
            secondCollection.Values.First().Value.Should().Be(dataValue);
            

        }

        private static DataCollectionModel CreateDataCollectionModel(DataValueType dataValueType, string key, object dataValue)
        {
            var nestedCollectionRows = new List<IReadOnlyDictionary<string, DataValueModel>>();
            var nestedDictionary = new Dictionary<string, DataValueModel>
            {
                {key, new DataValueModel() {Value = dataValue, ValueType = dataValueType}}
            };
            nestedCollectionRows.Add(nestedDictionary);
            var nestedCollectionModel = new DataCollectionModel()
            {
                Rows = nestedCollectionRows
            };

            var dictionary = new List<IReadOnlyDictionary<string, DataValueModel>>
            {
                new Dictionary<string, DataValueModel>()
                {
                    {"Key1", new DataValueModel() {Value = nestedCollectionModel, ValueType = DataValueType.Collection}},
                }
            };

            var dataCollectionModel = new DataCollectionModel {Rows = dictionary};
            return dataCollectionModel;
        }

        private static IEnumerable<TestCaseData> NestedCollectionTestCaseSource
        {
            get
            {
                yield return new TestCaseData(DataValueType.Number, "Number", 38M);
                yield return new TestCaseData(DataValueType.Text, "Text", "Hello World");
                yield return new TestCaseData(DataValueType.DateTime, "DateTime", new DateTimeOffset(new DateTime(2021,02,01,15,45,12),TimeSpan.Zero));
                yield return new TestCaseData(DataValueType.Date, "Date", new DateTime(2021,02,01));
                yield return new TestCaseData(DataValueType.Time, "Time", new DateTimeOffset(2,1,1,10,0,0,TimeSpan.Zero));
                yield return new TestCaseData(DataValueType.Flag, "Flag", true);
            }
        }

    }
    
}
