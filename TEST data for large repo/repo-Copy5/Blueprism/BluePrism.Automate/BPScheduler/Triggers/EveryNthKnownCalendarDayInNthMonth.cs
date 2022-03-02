using System;
using System.Runtime.Serialization;
using BluePrism.Scheduling.Calendar;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// <para>
    /// Trigger used to activate on the last calendar-derived day every nth 
    /// month where the calendar is static - ie. it is known at creation
    /// time and it will not change in the interim due to outside forces.
    /// </para><para>
    /// If the calendar to be used is a linked calendar (ie. a trigger with
    /// a reference to a <see cref="ScheduleCalendar"/>, you should use the
    /// <see cref="EveryNthIdentifiedCalendarDayInNthMonth"/> trigger so
    /// that the trigger will pick the latest version of the calendar from
    /// its owning schedule.
    /// </para>
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true),
     KnownType(typeof(ScheduleCalendar)),
     KnownType(typeof(LiberalCalendar)),
     KnownType(typeof(DaySetCalendar))]
    public class EveryNthKnownCalendarDayInNthMonth : EveryNthCalendarDayInNthMonth
    {
        /// <summary>
        /// The calendar providing the valid days to allow this trigger
        /// to activate.
        /// </summary>
        [DataMember]
        private ICalendar _calendar;

        /// <summary>
        /// The calendar to use for this trigger
        /// </summary>
        protected override ICalendar Calendar
        {
            get { return _calendar; }
        }

        /// <summary>
        /// Creates a new trigger which activates on each first/second/last/etc
        /// calendar day of every month, according the given known calendar.
        /// </summary>
        /// <param name="period">The period of months between trigger activations
        /// for this trigger.</param>
        /// <param name="nth">The iteration within the month that the trigger
        /// should activate.</param>
        /// <param name="cal">The calendar which provides the valid working
        /// days on which to allow activation</param>
        /// <exception cref="ArgumentException">If the given period was negative
        /// or zero.</exception>
        public EveryNthKnownCalendarDayInNthMonth(NthOfMonth nth, ICalendar cal)
            : this(1, nth, cal) { }

        /// <summary>
        /// Creates a new trigger which activates on each first/second/last/etc
        /// calendar day of every nth month, according the given known calendar.
        /// </summary>
        /// <param name="period">The period of months between trigger activations
        /// for this trigger.</param>
        /// <param name="nth">The iteration within the month that the trigger
        /// should activate.</param>
        /// <param name="cal">The calendar which provides the valid working
        /// days on which to allow activation</param>
        /// <exception cref="ArgumentException">If the given period was negative
        /// or zero.</exception>
        public EveryNthKnownCalendarDayInNthMonth(int period, NthOfMonth nth, ICalendar cal)
            : base(period, nth)
        {
            _calendar = cal;
        }

    }
}
