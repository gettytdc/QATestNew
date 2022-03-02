namespace BluePrism.Api.Models
{
    using System;

    public class ScheduleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int InitialTaskId { get; set; }
        public bool IsRetired { get; set; }
        public int TasksCount { get; set; }
        public IntervalType IntervalType { get; set; }
        public int TimePeriod { get; set; }
        public DateTimeOffset StartPoint { get; set; }
        public DateTimeOffset EndPoint { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public NthOfMonth DayOfMonth { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int CalendarId { get; set; }
        public string CalendarName { get; set; }
    }
}
