using System;

namespace BluePrism.Scheduling.Calendar
{
    /// <summary>
    /// A calendar which will allow any job to run at any time.
    /// </summary>
    public class LiberalCalendar : ICalendar
    {
        /// <summary>
        /// Checks if this calendar allows running at the given point in time.
        /// It does.
        /// </summary>
        /// <param name="dt">The date/time to check</param>
        /// <returns>true - this implementation always allows jobs to run.</returns>
        public bool CanRun(DateTime dt)
        {
            return true;
        }

        public bool HasAnyWorkingDays()
        {
            return true;
        }

        /// <summary>
        /// Updates the given metadata with the data on this calendar.
        /// This calendar works every day, and is not identifiable on the
        /// database, so the easiest way to represent it is to set all days on.
        /// </summary>
        /// <param name="meta">The metadata to update.</param>
        public void UpdateMetaData(TriggerMetaData metadata)
        {
            metadata.Days = new DaySet(DaySet.ALL_DAYS);
        }
    }
}
