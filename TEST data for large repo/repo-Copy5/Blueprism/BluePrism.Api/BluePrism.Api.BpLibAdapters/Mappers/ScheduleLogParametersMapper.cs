namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using Domain;
    using FilterMappers;
    using Func;
    using Server.Domain.Models;
    using Server.Domain.Models.Pagination;
    using ScheduleLogParameters = Domain.ScheduleLogParameters;

    using static Func.OptionHelper;

    public static class ScheduleLogParametersMapper
    {
        public static Server.Domain.Models.ScheduleLogParameters ToBluePrismObject(this ScheduleLogParameters @this) =>
            new Server.Domain.Models.ScheduleLogParameters
            {
                ScheduleId = @this.ScheduleId.ToBluePrismObject(),
                ItemsPerPage = @this.ItemsPerPage,
                StartTime = @this.StartTime.ToBluePrismObject(),
                EndTime = @this.EndTime.ToBluePrismObject(),
                ScheduleLogStatus = @this.ScheduleLogStatus.ToBluePrismObject(x => x.ToBluePrismObject()),
                PagingToken = @this.PagingToken.ToBluePrismPagingToken()
            };

        private static Option<ScheduleLogsPagingToken> ToBluePrismPagingToken(this Option<Domain.PagingTokens.PagingToken<int>> pagingToken)
        {
            switch (pagingToken)
            {
                case Some<Domain.PagingTokens.PagingToken<int>> t:
                    return Some(new ScheduleLogsPagingToken
                    {
                        PreviousIdValue = t.Value.PreviousIdValue,
                        DataType = t.Value.DataType,
                        PreviousSortColumnValue = t.Value.PreviousSortColumnValue,
                    });
                default:
                    return None<ScheduleLogsPagingToken>();
            }
        }

        private static ItemStatus ToBluePrismObject(this ScheduleLogStatus scheduleLogStatus)
        {
            switch (scheduleLogStatus)
            {
                case ScheduleLogStatus.All:
                    return ItemStatus.All;
                case ScheduleLogStatus.Unknown:
                    return ItemStatus.Unknown;
                case ScheduleLogStatus.Pending:
                    return ItemStatus.Pending;
                case ScheduleLogStatus.Running:
                    return ItemStatus.Running;
                case ScheduleLogStatus.Exceptioned:
                    return ItemStatus.Exceptioned;
                case ScheduleLogStatus.Stopped:
                    return ItemStatus.Stopped;
                case ScheduleLogStatus.Completed:
                    return ItemStatus.Completed;
                case ScheduleLogStatus.Debugging:
                    return ItemStatus.Debugging;
                case ScheduleLogStatus.Deferred:
                    return ItemStatus.Deferred;
                case ScheduleLogStatus.Locked:
                    return ItemStatus.Locked;
                case ScheduleLogStatus.Queried:
                    return ItemStatus.Queried;
                case ScheduleLogStatus.PartExceptioned:
                    return ItemStatus.PartExceptioned;
                default:
                    throw new ArgumentException($"Unable to map schedule log status: {scheduleLogStatus}");
            }
        }
    }
}
