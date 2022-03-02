using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which is activated every n days if the day falls on a day
    /// within an identified calendar (ie. a calendar with an ID retrievable
    /// via the store that this trigger is ultimately assigned to).
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNDaysWithinIdentifiedCalendar : EveryNDaysWithinCalendar
    {
        /// <summary>
        /// The dayset calendar containing the valid days on which trigger
        /// activation is allowed.
        /// </summary>
        [DataMember]
        private int _calendarId;

        /// <summary>
        /// Creates a new trigger which activates every day within the given
        /// day set.
        /// </summary>
        /// <param name="calendarId">The unique ID of the calendar to use for
        /// this trigger.</param>
        public EveryNDaysWithinIdentifiedCalendar(int calendarId)
            : this(1, calendarId) { }

        /// <summary>
        /// Creates a new trigger which activates every n days, if that falls
        /// on a day within the given day set.
        /// This will add <paramref name="period"/> days repeatedly until the
        /// date falls on a day within the given day set.
        /// </summary>
        /// <param name="period">The number of days between instances of this
        /// trigger.</param>
        /// <param name="calendarId">The unique ID of the calendar to use for
        /// this trigger.</param>
        public EveryNDaysWithinIdentifiedCalendar(int period, int calendarId)
            : base(period)
        {
            _calendarId = calendarId;
        }

        /// <summary>
        /// The calendar containing the valid days on which trigger activation
        /// is allowed.
        /// </summary>
        protected override ICalendar Calendar
        {
            get { return GetCalendarWithId(_calendarId); }
        }
    }
}
