using System;
using System.Runtime.Serialization;
using BluePrism.BPCoreLib.Data;
using BluePrism.Utilities.Functional;

namespace BluePrism.Scheduling
{
    [Serializable]
    [DataContract(Namespace = "bp", IsReference = true)]
    public class ScheduleSummary
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int InitialTaskId { get; set; }

        [DataMember]
        public bool IsRetired { get; set; }

        [DataMember]
        public int TasksCount { get; set; }

        [DataMember]
        public IntervalType IntervalType { get; set; }

        [DataMember]
        public int TimePeriod { get; set; }

        [DataMember]
        public DateTimeOffset StartPoint { get; set; }

        [DataMember]
        public DateTimeOffset EndPoint { get; set; }

        [DataMember]
        public DaySet DayOfWeek { get; set; }

        [DataMember]
        public NthOfMonth DayOfMonth { get; set; }

        [DataMember]
        public DateTimeOffset StartDate { get; set; }

        [DataMember]
        public DateTimeOffset EndDate { get; set; }

        [DataMember]
        public int CalendarId { get; set; }

        [DataMember]
        public string CalendarName { get; set; }

        public ScheduleSummary(IDataProvider provider = null)
        {
            if (provider == null)
                return;

            const int dbOffsetDefaultValue = -1;
            var timeZoneId = provider.GetString("timezoneid");
            var dbOffset = provider.GetInt("utcoffset", dbOffsetDefaultValue);

            Id = provider.GetInt("id");
            Name = provider.GetString("name");
            Description = provider.GetString("description");
            InitialTaskId = provider.GetInt("initialtaskid");
            IsRetired = provider.GetBool("retired");
            TasksCount = provider.GetInt("taskscount");
            IntervalType = provider.GetValue("unittype", IntervalType.Never);
            TimePeriod = provider.GetInt("period", 0);
            StartPoint = GetTimeOffsetWithUtcOffset(provider, "startpoint", dbOffset == dbOffsetDefaultValue ? GetUtcOffsetByTimeZoneId(timeZoneId, DateTime.UtcNow) : dbOffset);
            EndPoint = GetTimeOffsetWithUtcOffset(provider, "endpoint", dbOffset == dbOffsetDefaultValue ? GetUtcOffsetByTimeZoneId(timeZoneId, DateTime.UtcNow) : dbOffset);
            DayOfWeek = new DaySet(provider.GetInt("dayset", 0));
            DayOfMonth = provider.GetValue("nthofmonth", NthOfMonth.None);

            StartDate = provider.GetValue<DateTime?>("startdate", null)?.Map(date => new DateTimeOffset(date, TimeSpan.FromMinutes(dbOffset == dbOffsetDefaultValue ?
                GetUtcOffsetByTimeZoneId(timeZoneId, date) : dbOffset))) ?? DateTimeOffset.MinValue;

            EndDate = provider.GetValue<DateTime?>("enddate", null)?.Map(date => new DateTimeOffset(date, TimeSpan.FromMinutes(dbOffset == dbOffsetDefaultValue ?
                GetUtcOffsetByTimeZoneId(timeZoneId, date) : dbOffset))) ?? DateTimeOffset.MaxValue;

            CalendarId = provider.GetInt("calendarid");
            CalendarName = provider.GetString("calendarname");
        }

        private DateTimeOffset GetTimeOffsetWithUtcOffset(IDataProvider provider, string parameterName, int utcOffset)
        {
            var localDateTime = DateTime.SpecifyKind(DateTime.Now.Date.AddSeconds(provider.GetInt(parameterName, 0)),
                DateTimeKind.Unspecified);
            return new DateTimeOffset(localDateTime, TimeSpan.FromMinutes(utcOffset));
        }

        private int GetUtcOffsetByTimeZoneId(string timeZoneId, DateTime date) =>
            string.IsNullOrWhiteSpace(timeZoneId) ? 0 : (int)TimeZoneInfo.FindSystemTimeZoneById(timeZoneId).GetUtcOffset(date).TotalMinutes;
    }
}
