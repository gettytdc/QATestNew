using System;
using BluePrism.BPCoreLib.Data;

namespace BluePrism.Scheduling.ScheduleData
{
    public class ScheduleTriggerDatabaseData
    {
        public IntervalType UnitType { get; set; }
        public TriggerMode Mode { get; set; }
        public int Priority { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Period { get; set; }
        public int StartPoint { get; set; }
        public int EndPoint { get; set; }
        public DaySet DaySet { get; set; }
        public int CalendarId { get; set; }
        public NthOfMonth NthOfMonth { get; set; }
        public NonExistentDatePolicy MissingDatePolicy { get; set; }
        public bool UserTrigger { get; set; }
        public string TimeZoneId { get; set; }
        public TimeSpan? UtcOffset { get; set; }

        public ScheduleTriggerDatabaseData(IDataProvider dataProvider)
        {
            if (dataProvider == null)
                return;

            UnitType = dataProvider.GetValue(nameof(UnitType), IntervalType.Never);
            Mode = dataProvider.GetValue(nameof(Mode), TriggerMode.Indeterminate);
            Priority = dataProvider.GetInt(nameof(Priority), 1);
            StartDate = dataProvider.GetValue(nameof(StartDate), DateTime.MinValue);
            EndDate = dataProvider.GetValue(nameof(EndDate), DateTime.MaxValue);
            Period = dataProvider.GetInt(nameof(Period), 0);
            StartPoint = dataProvider.GetInt(nameof(StartPoint), -1);
            EndPoint = dataProvider.GetInt(nameof(EndPoint), -1);
            DaySet = new DaySet(dataProvider.GetInt(nameof(DaySet), 0));
            CalendarId = dataProvider.GetInt(nameof(CalendarId), 0);
            NthOfMonth = dataProvider.GetValue(nameof(NthOfMonth), NthOfMonth.None);
            MissingDatePolicy = dataProvider.GetValue(nameof(MissingDatePolicy), NonExistentDatePolicy.Skip);
            UserTrigger = dataProvider.GetBool(nameof(UserTrigger));
            TimeZoneId = dataProvider.GetString(nameof(TimeZoneId));
            var offset = dataProvider[nameof(UtcOffset)];
            UtcOffset = (offset != null) ? TimeSpan.FromMinutes((int)offset) : (TimeSpan?)null;
        }
    }
}
