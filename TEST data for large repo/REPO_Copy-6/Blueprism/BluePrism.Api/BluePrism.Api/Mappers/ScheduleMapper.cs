namespace BluePrism.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Func;
    using Models;

    public static class ScheduleMapper
    {
        public static ScheduleModel ToModelObject(this Schedule @this) =>
            new ScheduleModel
            {
                Id = @this.Id,
                Name = @this.Name,
                Description = @this.Description,
                InitialTaskId = @this.InitialTaskId,
                IsRetired = @this.IsRetired,
                TasksCount = @this.TasksCount,
                IntervalType = @this.IntervalType.ToModel(),
                TimePeriod = @this.TimePeriod,
                StartPoint = @this.StartPoint,
                EndPoint = @this.EndPoint,
                DayOfWeek = @this.DayOfWeek is Some<DayOfWeek> daySet ? daySet.Value : (DayOfWeek?)null,
                DayOfMonth = @this.DayOfMonth.ToModel(),
                StartDate = @this.StartDate is Some<DateTimeOffset> startDate ? startDate.Value : (DateTimeOffset?)null,
                EndDate = @this.EndDate is Some<DateTimeOffset> endDate ? endDate.Value : (DateTimeOffset?)null,
                CalendarId = @this.CalendarId,
                CalendarName = @this.CalendarName
            };

        public static IEnumerable<ScheduleModel> ToModel(this IEnumerable<Schedule> @this) =>
            @this.Select(x => x.ToModelObject());

        public static Schedule ToDomain(this UpdateScheduleModel @this) =>
            new Schedule {
                IsRetired = @this.IsRetired
            };

        private static Models.IntervalType ToModel(this Domain.IntervalType dbStatus)
        {
            switch (dbStatus)
            {
                case Domain.IntervalType.Never:
                    return Models.IntervalType.Never;
                case Domain.IntervalType.Once:
                    return Models.IntervalType.Once;
                case Domain.IntervalType.Hour:
                    return Models.IntervalType.Hour;
                case Domain.IntervalType.Day:
                    return Models.IntervalType.Day;
                case Domain.IntervalType.Week:
                    return Models.IntervalType.Week;
                case Domain.IntervalType.Month:
                    return Models.IntervalType.Month;
                case Domain.IntervalType.Year:
                    return Models.IntervalType.Year;
                case Domain.IntervalType.Minute:
                    return Models.IntervalType.Minute;
                case Domain.IntervalType.Second:
                    return Models.IntervalType.Second;
                default:
                    throw new ArgumentException("Unexpected interval type", nameof(dbStatus));
            }
        }

        private static Models.NthOfMonth ToModel(this Domain.NthOfMonth dbStatus)
        {
            switch (dbStatus)
            {
                case Domain.NthOfMonth.Last:
                    return Models.NthOfMonth.Last;
                case Domain.NthOfMonth.None:
                    return Models.NthOfMonth.None;
                case Domain.NthOfMonth.First:
                    return Models.NthOfMonth.First;
                case Domain.NthOfMonth.Second:
                    return Models.NthOfMonth.Second;
                case Domain.NthOfMonth.Third:
                    return Models.NthOfMonth.Third;
                case Domain.NthOfMonth.Fourth:
                    return Models.NthOfMonth.Fourth;
                case Domain.NthOfMonth.Fifth:
                    return Models.NthOfMonth.Fifth;
                default:
                    throw new ArgumentException("Unexpected day of month", nameof(dbStatus));
            }
        }
    }
}
