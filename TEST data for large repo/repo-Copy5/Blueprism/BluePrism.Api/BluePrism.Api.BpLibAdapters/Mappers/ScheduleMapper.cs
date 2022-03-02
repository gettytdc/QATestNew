namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Internal;
    using Domain;
    using Domain.PagingTokens;
    using Func;

    using static Func.OptionHelper;

    public static class ScheduleMapper
    {
        public static Domain.Schedule ToDomainObject(this Scheduling.ScheduleSummary @this) =>
            new Domain.Schedule
            {
                Id = @this.Id,
                Name = @this.Name,
                Description = @this.Description,
                InitialTaskId = @this.InitialTaskId,
                IsRetired = @this.IsRetired,
                TasksCount = @this.TasksCount,
                IntervalType = (Domain.IntervalType)@this.IntervalType,
                TimePeriod = @this.TimePeriod,
                StartPoint = @this.StartPoint,
                EndPoint = @this.EndPoint,
                DayOfWeek = @this.DayOfWeek.IsNullOrEmpty() ? None<DayOfWeek>() : Some(@this.DayOfWeek.First()),
                DayOfMonth = (Domain.NthOfMonth)@this.DayOfMonth,
                StartDate = @this.StartDate == DateTimeOffset.MinValue ? None<DateTimeOffset>() : Some(@this.StartDate),
                EndDate = @this.EndDate == DateTimeOffset.MaxValue ? None<DateTimeOffset>() : Some(@this.EndDate),
                CalendarId = @this.CalendarId,
                CalendarName = @this.CalendarName
            };

        public static ItemsPage<Schedule> ToItemsPage(this IReadOnlyList<Schedule> schedules, ScheduleParameters scheduleParameters) =>
            new ItemsPage<Schedule>()
            {
                Items = schedules,
                PagingToken = GetPagingToken(schedules, scheduleParameters)
            };

        private static Option<string> GetPagingToken(IReadOnlyList<Schedule> items, ScheduleParameters parameters)
        {
            if (items.Count < parameters.ItemsPerPage)
                return None<string>();

            var lastItem = items[items.Count - 1];

            var pagingToken = new PagingToken<string>
            {
                PreviousIdValue = lastItem.Name,
                DataType = lastItem.StartDate.GetTypeName(),
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };
            return Some(pagingToken.ToString());
        }
    }
}
