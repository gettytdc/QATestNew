using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which is activated every n weeks on the nth day of the week, on working 
    /// days defined in a particular known calendar.
    /// </summary>
    /// <remarks>
    /// This trigger should be used if the calendar will not be affected by 
    /// external forces within the lifetime of this trigger. If it is possible
    /// that the calendar may be altered elsewhere before this trigger is
    /// disposed of (and potentially reloaded) it may be more appropriate to
    /// use the <see cref="EveryNWeeksWithinIdentifiedCalendar"/> trigger.
    /// NOTE TO DEVELOPERS: The parameter nth which takes a value from the 
    /// enum NthOfMonth is actually referring to the nth day of the configured working 
    /// week for the respective calendar. So in a standard UK Mon-Fri working week 
    /// NthOfMonth.First = Monday</remarks>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNWeeksOnNthDayInKnownCalendar : EveryNWeeksOnNthDayInCalendar
    {
        /// <summary>
        /// The calendar to consult to see if a job can run or not.
        /// </summary>
        [DataMember]
        private ICalendar _calendar;

        /// <summary>
        /// Creates a new trigger which activates on specific days within a
        /// calendar every week.
        /// </summary>
        /// <param name="calendar">The calendar which should be consulted to 
        /// see if the trigger can activate.</param>
        public EveryNWeeksOnNthDayInKnownCalendar(ICalendar calendar)
            : this(1, NthOfWeek.First, calendar) { }

        /// <summary>
        /// Creates a new trigger which activates on specific days within a
        /// calendar every <paramref name="period"/> week.
        /// </summary>
        /// <param name="period">The number of weeks between trigger
        /// activations for this trigger.</param>
        /// <param name="calendar">The calendar which should be consulted to 
        /// see if the trigger can activate.</param>
        public EveryNWeeksOnNthDayInKnownCalendar(int period, ICalendar calendar)
            : this(period, NthOfWeek.First, calendar) { }

        /// <summary>
        /// Creates a new trigger which activates on specific days within a
        /// calender every n weeks.
        /// </summary>
        /// <param name="period">The number of weeks between trigger
        /// activations for this trigger.</param>
        /// <param name="calendar">The calendar which should be consulted to 
        /// see if the trigger can activate.</param>
        public EveryNWeeksOnNthDayInKnownCalendar(int period, NthOfWeek nth, ICalendar calendar)
            : base(period, nth)
        {
            _calendar = calendar;
        }

        /// <summary>
        /// The calendar which is used to determine activation times
        /// for this trigger.
        /// </summary>
        protected override ICalendar Calendar
        {
            get { return _calendar; }
        }
    }
}
