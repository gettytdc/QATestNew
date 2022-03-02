namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using FluentAssertions;
    using NUnit.Framework;
    using ScheduledSession = Domain.ScheduledSession;

    [TestFixture(Category = "Unit Test")]
    public class ScheduleSessionDataMapperTests
    {
        [Test]
        public void ToDomainModel_ShouldReturnCorrectlyMappedData()
        {
            var serverModel = new Server.Domain.Models.ScheduledSession
            {
                ProcessName = "test",
                ResourceName = "res"
            };
            var domainModel = new ScheduledSession
            {
                ProcessName = "test",
                ResourceName = "res"
            };

            var result = serverModel.ToDomainModel();
            result.ShouldBeEquivalentTo(domainModel);
        }
    }
}
