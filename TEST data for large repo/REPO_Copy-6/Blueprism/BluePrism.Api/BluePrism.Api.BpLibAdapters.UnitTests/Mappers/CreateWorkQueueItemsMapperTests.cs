namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using BpLibAdapters.Mappers;
    using CommonTestClasses.Extensions;
    using Domain;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class CreateWorkQueueItemsMapperTests
    {
        [Test]
        public void ToBluePrismObject_WithValidParameters_ReturnsCorrectlyMappedResult()
        {
            var status = "Started";
            var priority = 1;
            
            var createWorkQueueItem = new CreateWorkQueueItem()
            {
                Status = status,
                Priority = priority,
                DeferredDate = new DateTimeOffset(new DateTime(2020,1,1)),
                Tags = null,
                Data = CreateDataCollection()
            };
            var result = createWorkQueueItem.ToBluePrismObject();
            result.Status.Should().Be(status);
            result.Priority.Should().Be(priority);
            result.Defer.Should().BeSameDateAs(createWorkQueueItem.DeferredDate.Value.DateTime);
            result.Tags.Should().Be(string.Empty);
            result.Data.Rows.Count.Should().Be(1);
        }

        [Test]
        public void ToBluePrismObject_WithNullDataCollection_ReturnsCorrectlyMappedResult()
        {
            var status = "Started";
            var priority = 1;

            var createWorkQueueItem = new CreateWorkQueueItem()
            {
                Status = status,
                Priority = priority,
                DeferredDate = new DateTimeOffset(new DateTime(2020, 1, 1)),
                Tags = null,
                Data = null
            };
            var result = createWorkQueueItem.ToBluePrismObject();
            result.Status.Should().Be(status);
            result.Priority.Should().Be(priority);
            result.Defer.Should().BeSameDateAs(createWorkQueueItem.DeferredDate.Value.DateTime);
            result.Tags.Should().Be(string.Empty);
            result.Data.Rows.Count.Should().Be(0);
        }

        [TestCaseSource(nameof(TagsTestSourceData))]
        public void ToBluePrismObject_WithValidTagParameter_ReturnsCorrectlyMappedResult(IReadOnlyCollection<string> tags, string expectedResult)
        {
            var createWorkQueueItem = new CreateWorkQueueItem()
            {
                Tags = tags,
            };
            var result = createWorkQueueItem.ToBluePrismObject();
            result.Tags.Should().Be(expectedResult);
        }

        [Test]
        public void ToBluePrismObject_WithNullDeferredDate_ReturnsCorrectlyMappedResult()
        {
            var createWorkQueueItem = new CreateWorkQueueItem()
            {
                DeferredDate = null
            };
            var result = createWorkQueueItem.ToBluePrismObject();
            result.Defer.Should().Be(DateTime.MinValue);
        }


        private static IEnumerable<TestCaseData> TagsTestSourceData => new[]
            {
                (new List<string>(), ""),
                (new List<string>(){"tag1"}, "tag1"),
                (new List<string>(){"tag1","tag2"}, "tag1;tag2"),
                (new List<string>(){"tag1","tag2","tag3"}, "tag1;tag2;tag3")
            }
            .ToTestCaseData();

        private DataCollection CreateDataCollection() 
        {
            var dictionaryValues = new Dictionary<string, DataValue>
            {
                {"Key1", new DataValue() {Value = 1, ValueType = DataValueType.Number}},
                {"Key2", new DataValue() {Value = 1, ValueType = DataValueType.Number}},
                {"Key3", new DataValue() {Value = 1, ValueType = DataValueType.Number}}
            };
            var list = new List<IReadOnlyDictionary<string, DataValue>> { dictionaryValues };
            IReadOnlyCollection<IReadOnlyDictionary<string, DataValue>> dictionary = new ReadOnlyCollection<IReadOnlyDictionary<string, DataValue>>(list);

            var dataCollection = new DataCollection()
            {
                Rows = dictionary
            };
            return dataCollection;
        }
    }
}
