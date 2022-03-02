using System;

namespace BluePrism.Core.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Finds the time from the DateTime until the next given hour.
        /// </summary>
        /// <param name="hour">Hour of the day to calculate upto. If this hour has already passed
        /// the timespan will be until this hour tomorrow</param>
        public static TimeSpan TimeUntilNextHour(this DateTime dt, int hour)
        {
            if (hour < 0 || hour > 24)
                throw new ArgumentException($"{nameof(hour)} has to be within 0-24", nameof(hour));


            var dayStart = new DateTime(dt.Year, dt.Month, dt.Day);
            var target = dayStart + TimeSpan.FromHours(hour);

            return dt.Hour < hour ?
                target - dt :
                target.AddDays(1) - dt;
        }
    }
}
