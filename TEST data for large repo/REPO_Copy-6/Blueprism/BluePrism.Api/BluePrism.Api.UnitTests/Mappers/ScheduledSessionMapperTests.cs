namespace BluePrism.Api.UnitTests.Mappers
{
    using Api.Mappers;
    using Domain;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class ScheduledSessionMapperTests
    {
        [Test]
        public void ToModelObject_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var scheduledSession = new ScheduledSession { ProcessName = "name", ResourceName = "name" };
            var expectedModel = new ScheduledSessionModel { ProcessName = "name", ResourceName = "name" };
            var result = scheduledSession.ToModelObject();

            result.ShouldBeEquivalentTo(expectedModel);
        }
    }
}
