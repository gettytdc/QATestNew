namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Api.Mappers;
    using Domain;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class CreateWorkQueueItemModelMapperTest
    {
        [Test]
        public void CreateWorkQueueItem_ToDomainModel_ShouldReturnCorrectlyMappedModels()
        {
            var createWorkQueueItemModel = new CreateWorkQueueItemModel
            {
                Data = CreateDataCollectionModel(),
                DeferredDate = new DateTimeOffset(2020, 02, 5, 12, 12, 21, TimeSpan.FromHours(3)),
                Priority = 1,
                Tags = new List<string>() {"tag1"},
                Status = "testStatus"
            };

            var expectedCreateWorkQueueItemModel = new Domain.CreateWorkQueueItem
            {
                Data = CreateDomainDataCollection(),
                DeferredDate = new DateTimeOffset(2020, 02, 5, 12, 12, 21, TimeSpan.FromHours(3)),
                Priority = 1,
                Tags = new List<string>() { "tag1" },
                Status = "testStatus"
            };

            expectedCreateWorkQueueItemModel.ShouldBeEquivalentTo(createWorkQueueItemModel.ToDomainModel());
        }

        [Test]
        public void CreateWorkQueueItem_ToDomainModelWithNullTags_ShouldReturnCorrectlyMappedModels()
        {
            var createWorkQueueItemModel = new CreateWorkQueueItemModel
            {
                Tags = null,
                Data = CreateDataCollectionModel(),
                DeferredDate = new DateTimeOffset(2020, 02, 5, 12, 12, 21, TimeSpan.FromHours(3)),
                Priority = 1,
                Status = "testStatus"
            };
            
            var result = createWorkQueueItemModel.ToDomainModel();
            result.Tags.Count.Should().Be(0);
        }
        
        private static DataCollectionModel CreateDataCollectionModel()
        {
            var dictionaryValues = new Dictionary<string, DataValueModel>
            {
                {"Key1", new DataValueModel {Value = 1, ValueType = Models.DataValueType.Number}},
                {"Key2", new DataValueModel {Value = 1, ValueType = Models.DataValueType.Number}},
                {"Key3", new DataValueModel {Value = 1, ValueType = Models.DataValueType.Number}}
            };
            var list = new List<IReadOnlyDictionary<string, DataValueModel>> { dictionaryValues };
            IReadOnlyCollection<IReadOnlyDictionary<string, DataValueModel>> dictionary = new ReadOnlyCollection<IReadOnlyDictionary<string, DataValueModel>>(list);

            var dataCollection = new DataCollectionModel
            {
                Rows = dictionary
            };
            return dataCollection;
        }

        private static DataCollection CreateDomainDataCollection()
        {
            var dictionaryValues = new Dictionary<string, DataValue>
            {
                {"Key1", new DataValue {Value = 1, ValueType = Domain.DataValueType.Number}},
                {"Key2", new DataValue {Value = 1, ValueType = Domain.DataValueType.Number}},
                {"Key3", new DataValue {Value = 1, ValueType = Domain.DataValueType.Number}}
            };
            var list = new List<IReadOnlyDictionary<string, DataValue>> { dictionaryValues };
            IReadOnlyCollection<IReadOnlyDictionary<string, DataValue>> dictionary = new ReadOnlyCollection<IReadOnlyDictionary<string, DataValue>>(list);

            var dataCollection = new DataCollection
            {
                Rows = dictionary
            };
            return dataCollection;
        }
    }
}
