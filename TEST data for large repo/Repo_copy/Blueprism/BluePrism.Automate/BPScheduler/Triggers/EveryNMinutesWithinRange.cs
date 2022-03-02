using System;
using System.Runtime.Serialization;
using BluePrism.BPCoreLib;

namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Trigger which activates every n minutes within a certain time range.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true)]
    public class EveryNMinutesWithinRange : EveryNMinutes
    {
        #region Member variables

        /// <summary>
        /// The (inclusive) range of times within which this trigger can
        /// be activated.
        /// </summary>
        [DataMember]
        private clsTimeRange _range;

        /// <summary>
        /// The ID of the calendar for this trigger. Zero if no calendar is
        /// in use for the trigger.
        /// </summary>
        [DataMember]
        private int _calendarId;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="minutes"/> minutes with no time limitations.
        /// </summary>
        /// <param name="minutes">The number of minutes between activations
        /// </param>
        public EveryNMinutesWithinRange(int minutes)
            : this(minutes, new clsTimeRange(TimeSpan.MinValue, TimeSpan.MaxValue), 0) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="minutes"/> minutes with no time limitations.
        /// </summary>
        /// <param name="minutes">The number of minutes between activations
        /// </param>
        public EveryNMinutesWithinRange(int minutes, int calendarId)
            : this(minutes, new clsTimeRange(TimeSpan.MinValue, TimeSpan.MaxValue), calendarId) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="minutes"/> minutes, if that time falls between the given
        /// <paramref name="startTime"/> and <paramref name="endTime"/>.
        /// </summary>
        /// <param name="minutes">The number of minutes between activations
        /// </param>
        /// <param name="startTime">The start time at which an activation
        /// should be valid - this is inclusive, ie. the start time itself
        /// would be a valid activation time.</param>
        /// <param name="endTime">The end time at which an activation
        /// should be valid - this is inclusive, ie. the end time itself
        /// would be a valid activation time.</param>
        public EveryNMinutesWithinRange(int minutes, TimeSpan startTime, TimeSpan endTime)
            : this(minutes, new clsTimeRange(startTime, endTime), 0) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="minutes"/> minutes, if that time falls between the given
        /// <paramref name="startTime"/> and <paramref name="endTime"/>.
        /// </summary>
        /// <param name="minutes">The number of minutes between activations
        /// </param>
        /// <param name="startTime">The start time at which an activation
        /// should be valid - this is inclusive, ie. the start time itself
        /// would be a valid activation time.</param>
        /// <param name="endTime">The end time at which an activation
        /// should be valid - this is inclusive, ie. the end time itself
        /// would be a valid activation time.</param>
        public EveryNMinutesWithinRange(int minutes, TimeSpan start, TimeSpan end, int calendarId)
            : this(minutes, new clsTimeRange(start, end), calendarId) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="minutes"/> minutes, if that time falls within the given
        /// <paramref name="range"/>.
        /// </summary>
        /// <param name="minutes">The number of minutes between activations
        /// </param>
        /// <param name="range">The time range within which an activation
        /// should be valid.</param>
        public EveryNMinutesWithinRange(int minutes, clsTimeRange range)
            : this(minutes, range, 0) { }

        /// <summary>
        /// Creates a new trigger which activates every <paramref 
        /// name="minutes"/> minutes, if that time falls within the given
        /// <paramref name="range"/>.
        /// </summary>
        /// <param name="minutes">The number of minutes between activations
        /// </param>
        /// <param name="range">The time range within which an activation
        /// should be valid.</param>
        /// <param name="calendarId">The ID of the calendar which dictates
        /// when this trigger should be enabled.</param>
        public EveryNMinutesWithinRange(int minutes, clsTimeRange range, int calendarId)
            : base(minutes)
        {
            _range = range;
            _calendarId = calendarId;
        }

        #endregion

        /// <summary>
        /// Gets the calendar that this trigger operates on, or null if
        /// it contains no calendar reference.
        /// </summary>
        protected virtual ICalendar Calendar
        {
            get { return _calendarId == 0 ? null : GetCalendarWithId(_calendarId); }
        }

        /// <summary>
        /// The meta data describing this trigger.
        /// </summary>
        public override TriggerMetaData PrimaryMetaData
        {
            get
            {
                TriggerMetaData meta = base.PrimaryMetaData;
                meta.Interval = IntervalType.Minute;
                meta.Period = this.Minutes;
                meta.AllowedHours = _range;
                ICalendar cal = this.Calendar;
                if (cal != null)
                    cal.UpdateMetaData(meta);
                return meta;
            }
        }

        /// <summary>
        /// The start time - ie. the first time at which an activation
        /// of this trigger would be valid.
        /// </summary>
        public TimeSpan StartTime
        {
            get { return _range.StartTime; }
        }

        /// <summary>
        /// The end time, ie. the last time at which an activation of
        /// this trigger would be valid.
        /// </summary>
        public TimeSpan EndTime
        {
            get { return _range.EndTime; }
        }

        /// <summary>
        /// The time range within which this trigger can activate.
        /// </summary>
        public clsTimeRange Range
        {
            get { return _range; }
            set { _range = value; }
        }

        /// <summary>
        /// Checks whether this subclass allows the given point in time
        /// to work as an activation time.
        /// </summary>
        /// <param name="date">The point in time to check to see if it
        /// should be allowed as an activation time.</param>
        /// <returns>true if the time on the given datefalls within the
        /// start and end time defined in this object.</returns>
        protected override ActivationAllowed Allows(DateTime date)
        {
            ICalendar cal = this.Calendar;

            if (Calendar != null && !Calendar.HasAnyWorkingDays())
            {
                return ActivationAllowed.NeverAllowed;
            }

            if (base.Allows(date) == ActivationAllowed.Allowed &&
                Range.Contains(date.TimeOfDay) &&
                (cal == null || cal.CanRun(date)))
            {
                return ActivationAllowed.Allowed;
            }

            return ActivationAllowed.NotAllowed;
        }
    }
}
