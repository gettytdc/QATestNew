using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Calendar
{
    /// <summary>
    /// Calendar which only allows jobs to run on specified days of the week.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class DaySetCalendar : ICalendar
    {
        /// <summary>
        /// The days on which this calendar allows jobs to run
        /// </summary>
        [DataMember]
        private DaySet _days;

        /// <summary>
        /// Creates a new calendar which allows jobs to run on the specified
        /// days of the week.
        /// </summary>
        /// <param name="days">The days on which jobs are allows to run.</param>
        public DaySetCalendar(DaySet days)
        {
            _days = new DaySet();
            _days.SetTo(days);
        }

        /// <summary>
        /// A copy of the day set that this calendar is using.
        /// </summary>
        public DaySet Days
        {
            get { return new DaySet(_days); }
        }

        /// <summary>
        /// Checks if this calendar allows job execution at the specified
        /// point in time.
        /// </summary>
        /// <param name="dt">The date/time to check to see if this calendar
        /// allows jobs to run.</param>
        /// <returns>true if the given date/time falls on a day of the week
        /// which is allowed by this object.</returns>
        public bool CanRun(DateTime dt)
        {
            return _days.Contains(dt.DayOfWeek);
        }

        public bool HasAnyWorkingDays()
        {
            return _days.Count > 0;
        }

        /// <summary>
        /// Updates the given metadata with the data on this calendar.
        /// This calendar uses a day set, and thus sets the 'Days'
        /// property on the metadata.
        /// </summary>
        /// <param name="meta">The metadata to update.</param>
        public void UpdateMetaData(TriggerMetaData meta)
        {
            meta.Days = Days;
        }
    }
}
