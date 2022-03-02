namespace BluePrism.Api.Domain
{
    using System;
    using Func;

    public class ScheduleLog
    {
        public int ScheduleLogId { get; set; }
        public Option<DateTime> StartTime { get; set; }
        public Option<DateTime> EndTime { get; set; }
        public ScheduleLogStatus Status { get; set; }
        public string ServerName { get; set; }
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; }
    }
}
