namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BluePrism.Api.BpLibAdapters.Mappers;
    using BluePrism.Api.CommonTestClasses;
    using NUnit.Framework;

    [TestFixture]
    #pragma warning disable S2699 // Tests should include assertions
    class WorkQueueWithGroupMapperTests
    {
        [Test]
        public void ToWorkQueueWithGroupObject_WithTestDomainWorkQueue_ReturnsCorrectlyMappedResult()
        {
            var domainWorkQueue = WorkQueuesHelper.GetTestDomainWorkQueue();
            var workQueueWithGroup = domainWorkQueue.ToBluePrismWorkQueueWithGroup();

            WorkQueuesHelper.ValidateModelsAreEqual(workQueueWithGroup, domainWorkQueue);
        }

        [Test]
        public void ToDomainObject_WithTestWorkQueueWithGroup_ReturnsCorrectlyMappedResult()
        {
            var workQueueWithGroup = WorkQueuesHelper.GetTestWorkQueueWithGroup();
            var domainWorkQueue = workQueueWithGroup.ToDomainObject();

            WorkQueuesHelper.ValidateModelsAreEqual(workQueueWithGroup, domainWorkQueue);
        }
    }
}
