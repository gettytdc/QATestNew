using System;
using System.Runtime.Serialization;
using BluePrism.Scheduling.Calendar;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which is activated every n days if the day falls on a day
    /// within a predefined day set.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNDaysWithinDaySet : EveryNDaysWithinCalendar
    {
        /// <summary>
        /// The dayset calendar containing the valid days on which trigger
        /// activation is allowed.
        /// </summary>
        [DataMember]
        private DaySetCalendar _cal;

        /// <summary>
        /// Creates a new trigger which activates every day within the given
        /// day set.
        /// </summary>
        /// <param name="days">The day set containing the days on which this
        /// trigger can activate.</param>
        public EveryNDaysWithinDaySet(DaySet days)
            : this(1, days) { }

        /// <summary>
        /// Creates a new trigger which activates every n days, if that falls
        /// on a day within the given day set.
        /// This will add <paramref name="period"/> days repeatedly until the
        /// date falls on a day within the given day set.
        /// </summary>
        /// <param name="period">The number of days between instances of this
        /// trigger.</param>
        /// <param name="days">The days on which this trigger is permitted
        /// being activated.</param>
        public EveryNDaysWithinDaySet(int period, DaySet days)
            : base(period)
        {
            _cal = new DaySetCalendar(days);
        }

        /// <summary>
        /// The calendar containing the valid days on which trigger activation
        /// is allowed.
        /// </summary>
        protected override ICalendar Calendar
        {
            get { return _cal; }
        }

        /// <summary>
        /// Gets the next instance of this trigger which is activated after the
        /// given date. Overridden because there are a couple of shortcuts that
        /// can be performed for a daily trigger based on a DaySet.
        /// </summary>
        /// <param name="after">The date after which the next instance of this
        /// trigger is to be activated.</param>
        /// <returns>A trigger instance representing the next instance of this 
        /// trigger which is activated after the specified date, or null if
        /// there are no more activations of this trigger after the given date.
        /// </returns>
        public override ITriggerInstance GetNextInstance(DateTime after)
        {
            // quick check - if the dayset contains no valid days
            // then make sure we don't return an instance.
            if (_cal.Days.IsEmpty())
                return null;

            // Quick check number 2
            // if period == 7 and Start falls on a day which is not in the
            // contained DaySet, shortcut to no instance - a day within the
            // DaySet can never be reached.
            if (Ticks / TimeSpan.TicksPerDay == 7 && !_cal.Days.Contains(Start.DayOfWeek))
                return null;

            // otherwise delegate to the base instance.
            return base.GetNextInstance(after);
        }

    }
}
