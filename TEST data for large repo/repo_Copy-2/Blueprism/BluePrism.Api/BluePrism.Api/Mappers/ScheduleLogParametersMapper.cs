namespace BluePrism.Api.Mappers
{
    using System;
    using System.Linq;
    using Domain.Filters;
    using FilterMappers;
    using Models;

    public static class ScheduleLogParametersMapper
    {
        private const int DefaultItemsPerPage = 10;

        public static Domain.ScheduleLogParameters ToDomainObject(this ScheduleLogParameters @this, int? scheduleId = null) =>
            new Domain.ScheduleLogParameters
            {
                ScheduleId = GetScheduleIdFilter(scheduleId),
                ItemsPerPage = @this.ItemsPerPage ?? DefaultItemsPerPage,
                StartTime = @this.StartTime.ToDomain(x => x ?? DateTimeOffset.MinValue),
                EndTime = @this.EndTime.ToDomain(x => x ?? DateTimeOffset.MaxValue),
                ScheduleLogStatus = @this.ScheduleLogStatus?.Select(x => x.ToDomain()).ToMultiValueFilter()
                                    ?? new MultiValueFilter<Domain.ScheduleLogStatus>(Array.Empty<Filter<Domain.ScheduleLogStatus>>()),
                PagingToken = @this.PagingToken.ToDomainPagingToken(),
            };

        private static Domain.ScheduleLogStatus ToDomain(this ScheduleLogStatus scheduleLogStatus)
        {
            switch (scheduleLogStatus)
            {
                case ScheduleLogStatus.Completed:
                    return Domain.ScheduleLogStatus.Completed;
                case ScheduleLogStatus.PartExceptioned:
                    return Domain.ScheduleLogStatus.PartExceptioned;
                case ScheduleLogStatus.Pending:
                    return Domain.ScheduleLogStatus.Pending;
                case ScheduleLogStatus.Running:
                    return Domain.ScheduleLogStatus.Running;
                case ScheduleLogStatus.Terminated:
                    return Domain.ScheduleLogStatus.Terminated;
                default:
                    throw new ArgumentException($"Unable to map schedule log status value {scheduleLogStatus}");
            }
        }

        private static Filter<int> GetScheduleIdFilter(int? scheduleId) =>
            scheduleId == null ? (Filter<int>)new NullFilter<int>() : new EqualsFilter<int>(scheduleId.Value);
    }
}
