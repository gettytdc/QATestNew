namespace BluePrism.Api.Models
{
    using System;

    public class ScheduleLogModel
    {
        public int ScheduleLogId { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public ScheduleLogStatus Status { get; set; }
        public string ServerName { get; set; }
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; }
    }
}
