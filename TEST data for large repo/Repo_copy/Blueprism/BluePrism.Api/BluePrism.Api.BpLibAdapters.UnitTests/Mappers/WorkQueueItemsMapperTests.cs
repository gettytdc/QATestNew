namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class WorkQueueItemsMapperTests
    {
        [Test]
        public void ToDomainObjectNoDataXml_WithTestClsWorkQueueItem_ReturnsCorrectlyMappedResult()
        {
            var clsWorkQueueItem= WorkQueuesHelper.GetTestBluePrismWorkQueueItem(Guid.NewGuid(), new DateTime(2020,1,1), 1,2);
            var domainWorkQueue = clsWorkQueueItem.ToDomainObjectNoDataXml();

            WorkQueuesHelper.ValidateModelsAreEqual(clsWorkQueueItem, domainWorkQueue);
        }
    }
}
