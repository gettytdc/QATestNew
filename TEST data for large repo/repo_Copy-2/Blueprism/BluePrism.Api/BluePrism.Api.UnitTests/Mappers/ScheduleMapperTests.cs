namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using Api.Mappers;
    using CommonTestClasses.Extensions;
    using Domain;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class ScheduleMapperTests
    {
        [Test]
        public void ToModelObject_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var schedule = new Schedule
            {
                Id = 1,
                EndDate = OptionHelper.Some(DateTimeOffset.UtcNow),
                StartDate = OptionHelper.Some(DateTimeOffset.UtcNow),
                Description = "some desrc",
                IsRetired = false,
                TasksCount = 1,
                IntervalType = Domain.IntervalType.Day,
                TimePeriod = 1,
                StartPoint = DateTimeOffset.UtcNow,
                EndPoint = DateTimeOffset.UtcNow,
                DayOfWeek = OptionHelper.Some(DayOfWeek.Friday),
                DayOfMonth = Domain.NthOfMonth.Fifth,
                Name = "some name"
            };

            schedule.ToModelObject().ShouldBeEquivalentTo(new ScheduleModel
            {
                Id = schedule.Id,
                EndDate = schedule.EndDate is Some<DateTimeOffset> endDate ? endDate.Value : (DateTimeOffset?)null,
                StartDate = schedule.StartDate is Some<DateTimeOffset> startDate ? startDate.Value : (DateTimeOffset?)null,
                Description = schedule.Description,
                InitialTaskId = schedule.InitialTaskId,
                IsRetired = schedule.IsRetired,
                TasksCount = schedule.TasksCount,
                IntervalType = Models.IntervalType.Day,
                TimePeriod = schedule.TimePeriod,
                StartPoint = schedule.StartPoint,
                EndPoint = schedule.EndPoint,
                DayOfWeek = schedule.DayOfWeek is Some<DayOfWeek> daySet ? daySet.Value : (DayOfWeek?)null,
                DayOfMonth = Models.NthOfMonth.Fifth,
                Name = schedule.Name
            });
        }

        [TestCaseSource(nameof(MappedNthOfMonths))]
        public void ToModelObject_ShouldReturnCorrectNthOfMonth_WhenCalled(Domain.NthOfMonth domainNthOfMonth, Models.NthOfMonth modelNthOfMonth) =>
            new Schedule { DayOfMonth = domainNthOfMonth }
                    .ToModelObject()
                    .DayOfMonth
                    .Should()
                    .Be(modelNthOfMonth);

        [Test]
        public void ToModelObject_ShouldThrowArgumentException_WhenInvalidNthOfMonthSupplied()
        {
            Action action = () => new Schedule { DayOfMonth = (Domain.NthOfMonth)33 }.ToModelObject();
            action.ShouldThrow<ArgumentException>();
        }

        [TestCaseSource(nameof(MappedIntervalType))]
        public void ToModelObject_ShouldReturnCorrectIntervalType_WhenCalled(Domain.IntervalType domainIntervalType, Models.IntervalType modelIntervalType) =>
            new Schedule { IntervalType = domainIntervalType }
                .ToModelObject()
                .IntervalType
                .Should()
                .Be(modelIntervalType);

        [Test]
        public void ToModelObject_ShouldThrowArgumentException_WhenInvalidIntervalTypeSupplied()
        {
            Action action = () => new Schedule { IntervalType = (Domain.IntervalType)33 }.ToModelObject();
            action.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void ToDomain_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            bool isRetired = true;
            var scheduleChanges = new UpdateScheduleModel
            {
                IsRetired = isRetired
            };
            scheduleChanges.ToDomain().ShouldBeEquivalentTo(new Schedule { IsRetired = isRetired });
        }

        private static IEnumerable<TestCaseData> MappedIntervalType() =>
            new[]
                {
                    (Domain.IntervalType.Day, Models.IntervalType.Day),
                    (Domain.IntervalType.Hour, Models.IntervalType.Hour),
                    (Domain.IntervalType.Minute, Models.IntervalType.Minute),
                    (Domain.IntervalType.Month, Models.IntervalType.Month),
                    (Domain.IntervalType.Never, Models.IntervalType.Never),
                    (Domain.IntervalType.Once, Models.IntervalType.Once),
                    (Domain.IntervalType.Second, Models.IntervalType.Second),
                    (Domain.IntervalType.Week, Models.IntervalType.Week),
                    (Domain.IntervalType.Year, Models.IntervalType.Year)
                }
                .ToTestCaseData();

        private static IEnumerable<TestCaseData> MappedNthOfMonths() =>
            new[]
                {
                    (Domain.NthOfMonth.First, Models.NthOfMonth.First),
                    (Domain.NthOfMonth.Second, Models.NthOfMonth.Second),
                    (Domain.NthOfMonth.Third, Models.NthOfMonth.Third),
                    (Domain.NthOfMonth.Fourth, Models.NthOfMonth.Fourth),
                    (Domain.NthOfMonth.Fifth, Models.NthOfMonth.Fifth),
                    (Domain.NthOfMonth.Last, Models.NthOfMonth.Last),
                    (Domain.NthOfMonth.None, Models.NthOfMonth.None)
                }
                .ToTestCaseData();
    }
}
