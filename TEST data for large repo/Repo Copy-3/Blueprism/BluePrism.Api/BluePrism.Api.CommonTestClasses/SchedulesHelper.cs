namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BpLibAdapters.Mappers.FilterMappers;
    using Domain;
    using Domain.Filters;
    using FluentAssertions;
    using Func;
    using Scheduling;
    using IntervalType = Scheduling.IntervalType;
    using NthOfMonth = Scheduling.NthOfMonth;

    public static class SchedulesHelper
    {
        public static ScheduleSummary GetTestBluePrismScheduleSummary() =>
            new ScheduleSummary
            {
                Id = 1,
                EndDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                Description = "some desrc",
                InitialTaskId = 3,
                IsRetired = false,
                TasksCount = 1,
                IntervalType = IntervalType.Day,
                TimePeriod = 1,
                StartPoint = DateTimeOffset.UtcNow,
                EndPoint = DateTimeOffset.UtcNow,
                DayOfWeek = new DaySet(DayOfWeek.Tuesday),
                DayOfMonth = NthOfMonth.Fifth,
                Name = "some name",
                CalendarId = 1,
                CalendarName = "calendar name"
            };

        public static IEnumerable<ScheduleSummary> GetTestBluePrismScheduleSummary(
            int count,
            DateTimeOffset? offset = null)
        {
            var scheduleSummaryList = new List<ScheduleSummary>(count);
            for (var i = 0; i < count; i++)
            {
                scheduleSummaryList.Add(new ScheduleSummary
                {
                    Id = i,
                    EndDate = offset ?? DateTime.UtcNow.AddHours(i),
                    StartDate = offset ?? DateTime.UtcNow.AddHours(i),
                    Description = "some desrc",
                    InitialTaskId = 2,
                    IsRetired = false,
                    TasksCount = 1,
                    IntervalType = IntervalType.Day,
                    TimePeriod = 1,
                    StartPoint = offset ?? DateTimeOffset.UtcNow.AddHours(i),
                    EndPoint = offset ?? DateTimeOffset.UtcNow.AddHours(i),
                    DayOfWeek = new DaySet(DayOfWeek.Tuesday),
                    DayOfMonth = NthOfMonth.Fifth,
                    Name = $"some name {i}",
                    CalendarId = i,
                    CalendarName = $"calendar name {i}"
                });
            }

            return scheduleSummaryList;
        }

        public static void ValidateModelsAreEqual(ScheduleSummary[] bluePrism, Schedule[] domain)
        {
            bluePrism.Should().HaveCount(domain.Length);

            for (var i = 0; i < bluePrism.Length; i++)
                ValidateModelsAreEqual(bluePrism[i], domain[i]);
        }

        public static void ValidateModelsAreEqual(ScheduleSummary bluePrism, Schedule domain)
        {
            domain.Id.Should().Be(bluePrism.Id);
            ((Some<DateTimeOffset>)domain.EndDate).Value.Should().Be(bluePrism.EndDate);
            ((Some<DateTimeOffset>)domain.StartDate).Value.Should().Be(bluePrism.StartDate);
            ((Some<DayOfWeek>)domain.DayOfWeek).Value.Should().Be(bluePrism.DayOfWeek.First());
            domain.EndPoint.Should().Be(bluePrism.EndPoint);
            domain.StartPoint.Should().Be(bluePrism.StartPoint);
            domain.Name.Should().Be(bluePrism.Name);
            domain.Description.Should().Be(bluePrism.Description);
            domain.InitialTaskId = bluePrism.InitialTaskId;
            domain.IsRetired.Should().Be(bluePrism.IsRetired);
            domain.TasksCount.Should().Be(bluePrism.TasksCount);
            domain.TimePeriod.Should().Be(bluePrism.TimePeriod);
            IntervalTypesAreEqual(bluePrism.IntervalType, domain.IntervalType).Should().BeTrue();
            NthOfMonthsAreEqual(bluePrism.DayOfMonth, domain.DayOfMonth).Should().BeTrue();
            domain.CalendarId.Should().Be(bluePrism.CalendarId);
            domain.CalendarName.Should().Be(bluePrism.CalendarName);
        }

        public static void ValidateParametersModelsAreEqual(Server.Domain.Models.ScheduleParameters bluePrismParameters,
                                                            ScheduleParameters domainParameters)
        {
            bluePrismParameters.Name.Should().Be(domainParameters.Name.ToBluePrismObject());
            bluePrismParameters.RetirementStatus.Should()
                .Be(domainParameters.RetirementStatus.ToBluePrismObject(x => (Server.Domain.Models.RetirementStatus)x));
            bluePrismParameters.ItemsPerPage.Should().Be(domainParameters.ItemsPerPage);
        }

        public static bool IntervalTypesAreEqual(IntervalType bluePrismState, Domain.IntervalType domainState) =>
            (bluePrismState == IntervalType.Day && domainState == Domain.IntervalType.Day)
            || (bluePrismState == IntervalType.Hour && domainState == Domain.IntervalType.Hour)
            || (bluePrismState == IntervalType.Minute && domainState == Domain.IntervalType.Minute)
            || (bluePrismState == IntervalType.Month && domainState == Domain.IntervalType.Month)
            || (bluePrismState == IntervalType.Never && domainState == Domain.IntervalType.Never)
            || (bluePrismState == IntervalType.Once && domainState == Domain.IntervalType.Once)
            || (bluePrismState == IntervalType.Second && domainState == Domain.IntervalType.Second)
            || (bluePrismState == IntervalType.Week && domainState == Domain.IntervalType.Week)
            || (bluePrismState == IntervalType.Year && domainState == Domain.IntervalType.Year);

        private static bool NthOfMonthsAreEqual(NthOfMonth bluePrismState, Domain.NthOfMonth domainState) =>
            (bluePrismState == NthOfMonth.First && domainState == Domain.NthOfMonth.First)
            || (bluePrismState == NthOfMonth.Second && domainState == Domain.NthOfMonth.Second)
            || (bluePrismState == NthOfMonth.Third && domainState == Domain.NthOfMonth.Third)
            || (bluePrismState == NthOfMonth.Fourth && domainState == Domain.NthOfMonth.Fourth)
            || (bluePrismState == NthOfMonth.Fifth && domainState == Domain.NthOfMonth.Fifth)
            || (bluePrismState == NthOfMonth.Last && domainState == Domain.NthOfMonth.Last)
            || (bluePrismState == NthOfMonth.None && domainState == Domain.NthOfMonth.None);

        public static ScheduleParameters GetTestDomainScheduleParameters(int? itemsPerPage = null) =>
            new ScheduleParameters
            {
                Name = new StringStartsWithFilter("test"),
                ItemsPerPage = itemsPerPage ?? 10,
                RetirementStatus = new MultiValueFilter<RetirementStatus>(new[]
                {
                    new EqualsFilter<RetirementStatus>(RetirementStatus.Active),
                    new EqualsFilter<RetirementStatus>(RetirementStatus.Retired)
                })
            };
    }
}
