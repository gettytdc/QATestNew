namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using BpLibAdapters.Mappers;
    using CommonTestClasses.Extensions;
    using Domain;
    using Func;
    using NUnit.Framework;
    using Server.Domain.Models;

    [TestFixture(Category = "Unit Test")]
    public class ScheduleLogMapperTests
    {
        [Test]
        public void ToToDomainObject_WithTestDomainWorkQueue_ReturnsCorrectlyMappedResult()
        {
            var startTime = new DateTime(2021, 02, 01, 14, 25, 12, 245);
            var endTime = new DateTime(2021, 02, 03, 18, 18, 34, 85);

            var bluePrismScheduleLog = new Server.Domain.Models.ScheduleLog
            {
                ScheduleId = 41,
                ScheduleName = "Test Schedule Name",
                ServerName = "Test Server Name",
                ScheduleLogId = 76,
                Status = ItemStatus.Running,
                StartTime = OptionHelper.Some(startTime),
                EndTime = OptionHelper.Some(endTime),
            };

            var expected = new Domain.ScheduleLog
            {
                ScheduleId = 41,
                ScheduleName = "Test Schedule Name",
                ServerName = "Test Server Name",
                ScheduleLogId = 76,
                Status = ScheduleLogStatus.Running,
                StartTime = OptionHelper.Some(startTime),
                EndTime = OptionHelper.Some(endTime),
            };

            var mappedDomainScheduleLog = bluePrismScheduleLog.ToDomainObject();

            mappedDomainScheduleLog.ShouldRuntimeTypesBeEquivalentTo(expected);
        }
    }
}
