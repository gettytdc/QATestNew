using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which is activated every n weeks on the nth day of the week, on working 
    /// days defined in a particular identified calendar.
    /// Note that the calendar is drawn from the assigned owners of this
    /// trigger at the point of activation time calculation - meaning that this
    /// trigger must be assigned to a schedule, and that schedule to a scheduler
    /// before it can be calculated.
    /// </summary>
    /// <remarks>NOTE TO DEVELOPERS: The parameter nth which takes a value from the 
    /// enum NthOfMonth is actually referring to the nth day of the configured working 
    /// week for the respective calendar. So in a standard UK Mon-Fri working week 
    /// NthOfMonth.First = Monday</remarks>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNWeeksOnNthDayInIdentifiedCalendar : EveryNWeeksOnNthDayInCalendar
    {
        /// <summary>
        /// The ID of the calendar that this trigger uses.
        /// </summary>
        [DataMember]
        private int _calendarId;

        /// <summary>
        /// Creates a new trigger which activates on the first occurrence
        /// of any of the days specified as working days within an identified
        /// calendar.
        /// </summary>
        /// <param name="calendarId">The ID of the calendar to use to get the
        /// working days.</param>
        public EveryNWeeksOnNthDayInIdentifiedCalendar(int calendarId)
            : this(1, NthOfWeek.First, calendarId) { }

        /// <summary>
        /// Creates a new trigger which activates on the first occurrence of
        /// any of the days specified as working days within an identified
        /// calendar every <paramref name="period"/> weeks.
        /// </summary>
        /// <param name="calendarId">The ID of the calendar which should be
        /// used to identify the days on which this trigger should activate
        /// </param>
        public EveryNWeeksOnNthDayInIdentifiedCalendar(int period, int calendarId)
            : this(period, NthOfWeek.First, calendarId) { }

        /// <summary>
        /// Creates a new trigger which activates on specific days within a
        /// calender every n weeks.
        /// </summary>
        /// <param name="period">The number of weeks between trigger
        /// activations for this trigger.</param>
        /// <exception cref="ArgumentException">If the given period was
        /// negative or zero.</exception>
        public EveryNWeeksOnNthDayInIdentifiedCalendar(int period, NthOfWeek nth, int calendarId)
            : base(period, nth)
        {
            _calendarId = calendarId;
        }

        /// <summary>
        /// The calendar which is used to determine activation times
        /// for this trigger.
        /// </summary>
        protected override ICalendar Calendar
        {
            get { return GetCalendarWithId(_calendarId); }
        }
    }
}
