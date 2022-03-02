namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers.Dashboard
{
    using BpLibAdapters.Mappers.Dashboard;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class WorkQueueCompositionMapperTests
    {
        [Test]
        public void ToDomainObject_WhenCorrectBluePrismObject_ShouldMapCorrectly()
        {
            var domainWorkQueueComposition = new Server.Domain.Models.Dashboard.WorkQueueComposition
            {
                Completed = 5,
                Locked = 7,
                Deferred = 2,
                Exceptioned = 14,
                Pending = 9,
                Name = "TEST_NAME",
            };

            var expected = new Domain.Dashboard.WorkQueueComposition
            {
                Completed = 5,
                Locked = 7,
                Deferred = 2,
                Exceptioned = 14,
                Pending = 9,
                Name = "TEST_NAME",
            };

            var actual = domainWorkQueueComposition.ToDomainObject();

            actual.ShouldBeEquivalentTo(expected);
        }
    }
}
