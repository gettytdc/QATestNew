using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Triggers
{
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNthIdentifiedCalendarDayInNthMonth : EveryNthCalendarDayInNthMonth
    {
        /// <summary>
        /// The id of the calendar that this trigger uses.
        /// </summary>
        [DataMember]
        private int _calendarId;

        /// <summary>
        /// The calendar which is used by this trigger to determine which days to run.
        /// </summary>
        /// <exception cref="UnassignedItemException">If the trigger is not assigned
        /// to a schedule, or the owning schedule is not assigned to a scheduler.
        /// </exception>
        /// <exception cref="InvalidDataException">If no calendar could be found which
        /// corresponded to the ID set in this trigger.</exception>
        protected override ICalendar Calendar
        {
            get { return GetCalendarWithId(_calendarId); }
        }

        /// <summary>
        /// Creates a new trigger to activate every month on the
        /// first/second/last/etc day within the calendar identified by the
        /// given ID. The calendar will be retrieved from the schedule's
        /// <see cref="ISchedule.Store">backing store</see> at the point of
        /// calculating the running time for this trigger.
        /// </summary>
        /// <param name="nth">The iteration within the month that the trigger
        /// should activate.</param>
        /// <param name="cal">The ID of the calendar which provides the valid
        /// working days on which to allow activation</param>
        /// <exception cref="ArgumentException">If the given period was negative
        /// or zero.</exception>
        public EveryNthIdentifiedCalendarDayInNthMonth(NthOfMonth nth, int calendarId)
            : this(1, nth, calendarId) { }

        /// <summary>
        /// Creates a new trigger to activate every nth month on the
        /// first/second/last/etc day within the calendar identified by the
        /// given ID. The calendar will be retrieved from the schedule's
        /// <see cref="ISchedule.Store">backing store</see> at the point of
        /// calculating the running time for this trigger.
        /// </summary>
        /// <param name="period">The period of months between trigger activations
        /// for this trigger.</param>
        /// <param name="nth">The iteration within the month that the trigger
        /// should activate.</param>
        /// <param name="cal">The ID of the calendar which provides the valid
        /// working days on which to allow activation</param>
        /// <exception cref="ArgumentException">If the given period was negative
        /// or zero.</exception>
        public EveryNthIdentifiedCalendarDayInNthMonth(int period, NthOfMonth nth, int calendarId)
            : base(period, nth)
        {
            _calendarId = calendarId;
        }

    }
}
