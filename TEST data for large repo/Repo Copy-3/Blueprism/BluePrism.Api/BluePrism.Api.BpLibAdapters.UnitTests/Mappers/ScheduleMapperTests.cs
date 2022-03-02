namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class ScheduleMapperTests
    {
        [Test]
        public void ToDomainObject_WithTestScheduleSummary_ReturnsCorrectlyMappedResult()
        {
            var serverSchedule = SchedulesHelper.GetTestBluePrismScheduleSummary();
            var domainSchedule = serverSchedule.ToDomainObject();

            SchedulesHelper.ValidateModelsAreEqual(serverSchedule, domainSchedule);
        }
    }
}
