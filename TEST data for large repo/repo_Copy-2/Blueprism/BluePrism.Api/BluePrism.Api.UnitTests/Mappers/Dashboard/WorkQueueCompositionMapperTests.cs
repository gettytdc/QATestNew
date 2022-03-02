namespace BluePrism.Api.UnitTests.Mappers.Dashboard
{
    using System;
    using Api.Mappers.Dashboard;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class WorkQueueCompositionMapperTests
    {
        [Test]
        public void ToModel_WhenCorrectDomainObject_ShouldMapCorrectly()
        {
            var domainWorkQueueComposition = new Domain.Dashboard.WorkQueueComposition
            {
                Id = Guid.Parse("F3AFE477-17B2-4120-8619-203FDFCA9CC4"),
                Name = "TEST_NAME",
                Completed = 5,
                Locked = 7,
                Deferred = 2,
                Exceptioned = 14,
                Pending = 9,
            };

            var expected = new Models.Dashboard.WorkQueueCompositionModel
            {
                Id = Guid.Parse("F3AFE477-17B2-4120-8619-203FDFCA9CC4"),
                Name = "TEST_NAME",
                Completed = 5,
                Locked = 7,
                Deferred = 2,
                Exceptioned = 14,
                Pending = 9,
            };

            var actual = domainWorkQueueComposition.ToModel();

            actual.ShouldBeEquivalentTo(expected);
        }
    }
}
