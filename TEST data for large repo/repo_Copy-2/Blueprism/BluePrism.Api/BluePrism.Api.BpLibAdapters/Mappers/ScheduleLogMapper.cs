namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System.Collections.Generic;
    using Domain;
    using Domain.PagingTokens;
    using Func;
    using Server.Domain.Models.Pagination;
    using static Func.OptionHelper;

    public static class ScheduleLogMapper
    {
        public static Domain.ScheduleLog ToDomainObject(this BluePrism.Server.Domain.Models.ScheduleLog @this) =>
            new Domain.ScheduleLog
            {
                ScheduleLogId = @this.ScheduleLogId,
                Status = (Domain.ScheduleLogStatus)@this.Status,
                StartTime = @this.StartTime,
                EndTime = @this.EndTime,
                ServerName = @this.ServerName,
                ScheduleId = @this.ScheduleId,
                ScheduleName = @this.ScheduleName,
            };

        public static ItemsPage<ScheduleLog> ToItemsPage(this IReadOnlyList<ScheduleLog> schedulesLogs, ScheduleLogParameters scheduleLogParameters) =>
            new ItemsPage<ScheduleLog>
            {
                Items = schedulesLogs,
                PagingToken = GetPagingToken(schedulesLogs, scheduleLogParameters)
            };

        private static Option<string> GetPagingToken(IReadOnlyList<ScheduleLog> items, ScheduleLogParameters parameters)
        {
            if (items.Count < parameters.ItemsPerPage)
                return None<string>();

            var lastItem = items[items.Count - 1];
            var lastItemSortValue = lastItem.StartTime;

            var pagingToken = new PagingToken<int>
            {
                PreviousIdValue = lastItem.ScheduleLogId,
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItemSortValue),
                DataType = lastItemSortValue.GetTypeName(),
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };
            return Some(pagingToken.ToString());
        }
    }
}
