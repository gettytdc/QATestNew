namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using Api.Mappers;
    using CommonTestClasses.Extensions;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;

    [TestFixture]
    public class ScheduleLogMapperTests
    {
        [TestCaseSource(nameof(ScheduleLogStatusCases))]
        public void ToModelObject_ShouldReturnCorrectlyMappedModel_WhenCalled(Domain.ScheduleLogStatus domainScheduleLogStatus, Models.ScheduleLogStatus modelsScheduleLogStatus)
        {
            var startTime = new DateTime(2021, 02, 01, 14, 25, 35, DateTimeKind.Unspecified);
            var endTime = new DateTime(2021, 02, 01, 16, 15, 48, DateTimeKind.Unspecified);

            var scheduleLog = new Domain.ScheduleLog
            {
                ScheduleId = 11,
                ScheduleName = "Test Schedule Name",
                ServerName = "Test Server Name",
                ScheduleLogId = 55,
                Status = domainScheduleLogStatus,
                StartTime = OptionHelper.Some(startTime),
                EndTime = OptionHelper.Some(endTime)
            };

            var scheduleLogModel = scheduleLog.ToModelObject();

            scheduleLogModel.ShouldBeEquivalentTo(new Models.ScheduleLogModel()
            {
                ScheduleId = 11,
                ScheduleName = "Test Schedule Name",
                ServerName = "Test Server Name",
                ScheduleLogId = 55,
                Status = modelsScheduleLogStatus,
                StartTime = new DateTimeOffset(startTime, TimeSpan.Zero),
                EndTime = new DateTimeOffset(endTime,TimeSpan.Zero),
            });

            scheduleLogModel.StartTime.Value.Offset.Should().Be(TimeSpan.Zero);
            scheduleLogModel.EndTime.Value.Offset.Should().Be(TimeSpan.Zero);
        }

        private static IEnumerable<TestCaseData> ScheduleLogStatusCases() => new[]
        {
            (Domain.ScheduleLogStatus.PartExceptioned, Models.ScheduleLogStatus.PartExceptioned),
            (Domain.ScheduleLogStatus.Terminated, Models.ScheduleLogStatus.Terminated),
            (Domain.ScheduleLogStatus.Completed, Models.ScheduleLogStatus.Completed),
            (Domain.ScheduleLogStatus.Running, Models.ScheduleLogStatus.Running),
            (Domain.ScheduleLogStatus.Pending, Models.ScheduleLogStatus.Pending),
        }
        .ToTestCaseData();
    }
}
