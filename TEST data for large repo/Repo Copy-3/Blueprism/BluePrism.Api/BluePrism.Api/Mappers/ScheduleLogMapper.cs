namespace BluePrism.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Func;
    using Models;

    public static class ScheduleLogMapper
    {
        public static ScheduleLogModel ToModelObject(this ScheduleLog @this) =>
            new ScheduleLogModel
            {
                ScheduleLogId = @this.ScheduleLogId,

                StartTime = @this.StartTime is Some<DateTime> startTime ? DateTime.SpecifyKind(startTime.Value, DateTimeKind.Utc) : (DateTime?)null,
                EndTime = @this.EndTime is Some<DateTime> endTime ? DateTime.SpecifyKind(endTime.Value, DateTimeKind.Utc) : (DateTime?)null,
                Status = @this.Status.ToModel(),
                ServerName = @this.ServerName,
                ScheduleId = @this.ScheduleId,
                ScheduleName = @this.ScheduleName,
            };

        public static IEnumerable<ScheduleLogModel> ToModel(this IEnumerable<ScheduleLog> @this) =>
            @this.Select(ToModelObject);

        private static Models.ScheduleLogStatus ToModel(this Domain.ScheduleLogStatus scheduleLogStatus)
        {
            switch (scheduleLogStatus)
            {
                case Domain.ScheduleLogStatus.PartExceptioned:
                    return Models.ScheduleLogStatus.PartExceptioned;
                case Domain.ScheduleLogStatus.Terminated:
                    return Models.ScheduleLogStatus.Terminated;
                case Domain.ScheduleLogStatus.Completed:
                    return Models.ScheduleLogStatus.Completed;
                case Domain.ScheduleLogStatus.Pending:
                    return Models.ScheduleLogStatus.Pending;
                case Domain.ScheduleLogStatus.Running:
                    return Models.ScheduleLogStatus.Running;
                default:
                    throw new ArgumentException("Unexpected schedule log status ", nameof(scheduleLogStatus));
            }
        }
    }
}
