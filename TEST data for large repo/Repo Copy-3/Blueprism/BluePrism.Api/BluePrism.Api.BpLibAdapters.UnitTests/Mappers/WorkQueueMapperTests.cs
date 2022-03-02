namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    #pragma warning disable S2699 // Tests should include assertions
    public class WorkQueueMapperTests
    {
        [Test]
        public void ToBluePrismObject_WithTestDomainWorkQueue_ReturnsCorrectlyMappedResult()
        {
            var domainWorkQueue = WorkQueuesHelper.GetTestDomainWorkQueue();
            var clsWorkQueue = domainWorkQueue.ToBluePrismObject();

            WorkQueuesHelper.ValidateModelsAreEqual(clsWorkQueue, domainWorkQueue);
        }

        [Test]
        public void ToDomainObject_WithTestClsWorkQueue_ReturnsCorrectlyMappedResult()
        {
            var clsWorkQueue = WorkQueuesHelper.GetTestBluePrismClsWorkQueue();
            var domainWorkQueue = clsWorkQueue.ToDomainObject();

            WorkQueuesHelper.ValidateModelsAreEqual(clsWorkQueue, domainWorkQueue);
        }
    }
}
