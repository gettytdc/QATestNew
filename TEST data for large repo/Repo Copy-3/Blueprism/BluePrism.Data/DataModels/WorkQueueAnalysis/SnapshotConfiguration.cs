using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NodaTime;

namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    [KnownType(typeof(TimeZoneInfo.AdjustmentRule))]
    [KnownType(typeof(TimeZoneInfo.AdjustmentRule[]))]
    [KnownType(typeof(TimeZoneInfo.TransitionTime))]
    [KnownType(typeof(DayOfWeek))]
    public class SnapshotConfiguration
    {
        private const int NameLimit = 255;

        [DataMember]
        private int _id;
        [DataMember]
        private bool _enabled;
        [DataMember]
        private string _name;
        [DataMember]
        private SnapshotInterval _interval;
        [DataMember]
        private TimeZoneInfo _timezone;
        [DataMember]
        private LocalTime _startTime;
        [DataMember]
        private LocalTime _endTime;
        [DataMember]
        private SnapshotDayConfiguration _daysOfTheWeek;

        private const int _oneDayInSeconds = 60 * 60 * 24;
        private const int _oneWeekInSeconds = 60 * 60 * 24 * 7;

        private static readonly int _trendCalculationTime = (int) (new LocalTime(00,15).TickOfDay / NodaConstants.TicksPerSecond);

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public SnapshotInterval Interval
        {
            get => _interval;
            set => _interval = value;
        }

        public TimeZoneInfo Timezone
        {
            get => _timezone;
            set => _timezone = value;
        }

        public LocalTime StartTime
        {
            get => _startTime;
            set => _startTime = value;
        }

        public LocalTime EndTime
        {
            get => _endTime;
            set => _endTime = value;
        }

        public SnapshotDayConfiguration DaysOfTheWeek
        {
            get => _daysOfTheWeek;
            set => _daysOfTheWeek = value;
        }

        public int ConfiguredSnapshotRowsPerQueue()
        {
            return GetSnapshotTimes().Count * _daysOfTheWeek.GetDaysMap().Count;
        }

        public bool NameIsTooLong => _name.Length > NameLimit;
        public bool NameIsNullEmptyOrWhitespace => string.IsNullOrWhiteSpace(_name);
        public bool EndDateIsBeforeStartDate => _endTime.CompareTo(_startTime) < 0;

        public SnapshotConfiguration(int id, bool enabled, string name, SnapshotInterval interval,
            TimeZoneInfo timezone, LocalTime startTime, LocalTime endTime, SnapshotDayConfiguration daysOfTheWeek)
        {
            _id = id;
            _enabled = enabled;
            _name = name;
            _interval = interval;
            _timezone = timezone ?? throw new ArgumentNullException(nameof(timezone));
            _startTime = startTime;
            _endTime = endTime;
            _daysOfTheWeek = daysOfTheWeek ?? throw new ArgumentNullException(nameof(daysOfTheWeek));
        }

        public List<int> GetSnapshotTimes()
        {
            var result = new List<int>();

            var snapshotStart = _startTime == new NodaTime.LocalTime() ? new LocalTime(0, 0) : _startTime;
            var snapshotEnd = _endTime == new NodaTime.LocalTime() ? new LocalTime(23, 59) : _endTime;
            var intervalAsMinutes = _interval.AsMinutes();

            LocalTime snapshotTime = snapshotStart;
            do
            {
                result.Add((int)(snapshotTime.TickOfDay / NodaConstants.TicksPerSecond));
                snapshotTime = snapshotTime.PlusMinutes(intervalAsMinutes);
            } while (snapshotStart < snapshotTime && snapshotTime <= snapshotEnd);

            return result;
        }

        public List<int[]> GetInterimSnapshotDaysAndTimes()
        {
            List<int[]> interimSnapshotTimes = new List<int[]> { };
            var configuredSnapshotTimesInSecs = GetSnapshotTimes();
            var configuredSnapshotDays = _daysOfTheWeek.GetDaysMap();

            int previousSnapshotTime = 0;
            int previousSnapshotDay = 0;

            // first find any gaps requiring an interim snapshot in between the configured snapshots
            foreach (int currentSnapshotDay in configuredSnapshotDays)
            {
                foreach (int currentSnapshotTime in configuredSnapshotTimesInSecs)
                {
                    if (previousSnapshotDay == 0)
                    {
                        previousSnapshotDay = currentSnapshotDay;
                        previousSnapshotTime = currentSnapshotTime;
                    }
                    else
                    {
                        if (DayAndTimeDifferenceInSeconds(previousSnapshotDay, previousSnapshotTime, currentSnapshotDay, currentSnapshotTime)
                            > _oneDayInSeconds)
                        {
                            while (DayAndTimeDifferenceInSeconds(previousSnapshotDay, previousSnapshotTime, currentSnapshotDay, currentSnapshotTime)
                                  > _oneDayInSeconds)
                            {
                                interimSnapshotTimes.Add(new int[] { previousSnapshotDay + 1, previousSnapshotTime });
                                previousSnapshotDay = previousSnapshotDay + 1;
                            }
                        }
                        previousSnapshotDay = currentSnapshotDay;
                        previousSnapshotTime = currentSnapshotTime;
                    }
                }
            }

            // now find any gaps requiring an interim snapshot from this point until the first configured snapshot
            int lastSnapshotTime = previousSnapshotTime;
            int lastSnapshotDay = previousSnapshotDay;
            int firstConfiguredSnapshotTime = configuredSnapshotTimesInSecs[0];
            int firstConfiguredSnapshotDay = configuredSnapshotDays[0];

            // first check if there is only one snapshot in the week and if so add the first interim snapshot
            if (lastSnapshotDay == firstConfiguredSnapshotDay && lastSnapshotTime == firstConfiguredSnapshotTime)
            {
                interimSnapshotTimes.Add(new int[] { lastSnapshotDay + 1, lastSnapshotTime });
                lastSnapshotDay = lastSnapshotDay + 1;
            }
            // now keep adding interim snapshots until we get back to the first configured snapshot
            while (lastSnapshotDay < firstConfiguredSnapshotDay + 6 &&
                   DayAndTimeDifferenceInSeconds(lastSnapshotDay, lastSnapshotTime,
                       firstConfiguredSnapshotDay, firstConfiguredSnapshotTime) > _oneDayInSeconds)
            {
                int snapshotDayMod7 = (lastSnapshotDay + 1) % 7;
                int nextInterimSnapshotDay = snapshotDayMod7 == 0 ? 7 : snapshotDayMod7;
                interimSnapshotTimes.Add(new int[] { nextInterimSnapshotDay, lastSnapshotTime });
                lastSnapshotDay = lastSnapshotDay + 1;

            }
            return interimSnapshotTimes;
        }

        public static int DayAndTimeDifferenceInSeconds(int day1, int time1, int day2, int time2)
        {
            if (day2 - day1 < 0)
            {
                return (time2 - time1) + (7 + day2 - day1) * _oneDayInSeconds;
            }

            if (day2 == day1 && time2 - time1 < 0)
            {
                return _oneWeekInSeconds + (time2 - time1);
            }
            return (time2 - time1) + (day2 - day1) * _oneDayInSeconds;
        }

        public List<int[]> GetTrendCalculationDaysAndTimes()
        {
            List<int[]> trendCalculationDaysAndTimes = new List<int[]> { };
            foreach (int day in _daysOfTheWeek.GetDaysMap())
            {
                if (day == 7)
                {
                    trendCalculationDaysAndTimes.Add(new int[] { 1, _trendCalculationTime });
                }
                else
                {
                    trendCalculationDaysAndTimes.Add(new int[] { day + 1, _trendCalculationTime });
                }
            }
            return trendCalculationDaysAndTimes;
        }

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public bool IsValidConfiguration()
        {
            return !(NameIsTooLong || NameIsNullEmptyOrWhitespace || EndDateIsBeforeStartDate);
        }
    }
}
