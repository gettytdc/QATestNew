using System;
using System.Linq;
using System.Collections.Generic;
using BluePrism.BPCoreLib;
using BluePrism.BPCoreLib.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using BluePrism.Scheduling.Properties;


namespace BluePrism.Scheduling.Triggers
{
    /// <summary>
    /// Base trigger for implementations to extend which allows for
    /// modifiable modes and priorities.
    /// It also provides a basic GetTimes() implementation.
    /// Any subclasses will have to implement <see cref="GetNextTime"/>
    /// indicating when the trigger is next active.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true), KnownType("GetKnownTriggerTypes")]
    public abstract class BaseTrigger : ITrigger
    {

        #region - Class-scope Declarations -

        /// <summary>
        /// Gets all known triggers in this assembly.
        /// </summary>
        /// <returns>An enumerable over the triggers defined in this assembly.
        /// </returns>
        protected static internal IEnumerable<Type> GetKnownTriggerTypes()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetConcreteImplementations<ITrigger>()
                .Where((tp) => tp.Namespace != "BluePrism.Scheduling._UnitTests");
        }

        #endregion

        #region Member Variables


        // Whether this trigger is the 'user trigger' or not.
        [DataMember]
        private bool _userTrigger;

        // The start date/time for this trigger.
        [DataMember]
        private DateTime _start;

        // The end date/time for this trigger.
        [DataMember]
        private DateTime _end;

        // The mode of this trigger.
        [DataMember]
        private TriggerMode _mode;

        // The priority of this trigger.
        [DataMember]
        private int _priority;

        // The trigger group that this trigger belongs to.
        [DataMember]
        private ITriggerGroup _group;

        // The schedule that this trigger is assigned to
        [DataMember]
        private ISchedule _sched;

        [DataMember]
        private string _timeZoneId;

        [DataMember]
        private TimeSpan? _utcOffset;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new "Fire" mode base trigger with a priority of 1, which
        /// has a start date of now and an end date of eternity (or MaxValue)
        /// </summary>
        protected BaseTrigger()
            : this(TriggerMode.Fire, DEFAULT_PRIORITY, DateTime.Now, DateTime.MaxValue) { }

        /// <summary>
        /// Creates a new base trigger with the given mode and priority.
        /// It will be valid from the current date/time and have no end date.
        /// </summary>
        /// <param name="mode">The base mode that this trigger should be
        /// set to - either "Fire" or "Suppress"</param>
        /// <param name="priority">The priority to set within this
        /// trigger. The higher the number, the higher the priority.
        /// </param>
        protected BaseTrigger(TriggerMode mode, int priority)
            : this(mode, priority, DateTime.Now, DateTime.MaxValue) { }

        /// <summary>
        /// Creates a new base trigger with the given mode, priority and start
        /// date. It will have no end date.
        /// </summary>
        /// <param name="mode">The base mode that this trigger should be
        /// set to - either "Fire" or "Suppress"</param>
        /// <param name="priority">The priority to set within this
        /// trigger. The higher the number, the higher the priority.
        /// </param>
        /// <param name="start">The start date from which this trigger should
        /// be considered active.</param>
        protected BaseTrigger(TriggerMode mode, int priority, DateTime start)
            : this(mode, priority, start, DateTime.MaxValue) { }

        /// <summary>
        /// Creates a new trigger with the given mode, priority, start and 
        /// end dates.
        /// </summary>
        /// <param name="mode">The base mode that this trigger should be
        /// set to - either "Fire" or "Suppress"</param>
        /// <param name="priority">The priority to set within this
        /// trigger. The higher the number, the higher the priority.
        /// </param>
        /// <param name="start">The start date from which this trigger should
        /// be considered active.</param>
        /// <param name="end">The last date that this trigger is valid - not
        /// that this is <em>inclusive</em> meaning that the time given will
        /// be considered a valid date if a trigger would fire at that time.
        /// </param>
        protected BaseTrigger(TriggerMode mode, int priority, DateTime start, DateTime end)
        {
            _priority = priority;
            _mode = mode;
            _start = ResolveTriggerDate(start);
            _end = ResolveTriggerDate(end);
        }

        #endregion

        #region ITrigger properties

        /// <summary>
        /// The inclusive start date/time for this trigger
        /// Note that when setting, unless the given date is DateTime.MinValue,
        /// any time component more specific than seconds is stripped out of
        /// the given value before setting into this trigger.
        /// </summary>
        public virtual DateTime Start
        {
            get { return _start; }
            set
            {
                if (_start == value)
                    return;
                _start = ResolveTriggerDate(value);
                FireTimingUpdatedEvent();
            }
        }

        /// <summary>
        /// The inclusive end date/time for this trigger, DateTime.MaxValue if
        /// this trigger is set to never end.
        /// Note that when setting, unless the given date is DateTime.MaxValue,
        /// any time component more specific than seconds is stripped out of
        /// the given value before setting into this trigger.
        /// </summary>
        public virtual DateTime End
        {
            get { return _end; }
            set
            {
                if (_end == value)
                    return;
                _end = ResolveTriggerDate(value);
                FireTimingUpdatedEvent();
            }
        }

        /// <summary>
        /// Base mode of this trigger. This is the default mode of this
        /// trigger without timing data. For trigger groups, this will
        /// be effectively invalid, since they require the timing data to
        /// see if the resultant mode is 'Fire' or 'Suppress'.
        /// </summary>
        public virtual TriggerMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        /// <summary>
        /// The priority that this trigger has.
        /// </summary>
        public virtual int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        /// <summary>
        /// The group that this trigger belongs to, or null if it is
        /// not a member of a group.
        /// </summary>
        public virtual ITriggerGroup Group
        {
            get { return _group; }
            set { _group = value; }
        }

        /// <summary>
        /// The job that this trigger is assigned to
        /// </summary>
        public virtual ISchedule Schedule
        {
            get { return _sched; }
            set { _sched = value; }
        }

        /// <summary>
        /// Flag to indicate whether this trigger is user-configurable or
        /// whether it is a system trigger, ie. not configurable by the user.
        /// </summary>
        public virtual bool IsUserTrigger
        {
            get { return _userTrigger; }
            set { _userTrigger = value; }
        }

        public virtual string TimeZoneId
        {
            get { return _timeZoneId; }
            set { _timeZoneId = value; }
        }

        public virtual TimeSpan? UtcOffset
        {
            get { return _utcOffset; }
            set { _utcOffset = value; }
        }

        /// <summary>

        #endregion

        #region ITrigger methods

        /// <summary>
        /// The meta data which describes this trigger. This assumes
        /// that this trigger can be represented by a single meta data
        /// object, which will be returned by the <see 
        /// cref="SingleMetaData"/> property. If that is not the case,
        /// this property should be overridden.
        /// </summary>
        public virtual ICollection<TriggerMetaData> MetaData
        {
            get
            {
                TriggerMetaData meta = PrimaryMetaData;
                if (meta != null)
                    return GetSingleton.ICollection(meta);

                throw new InvalidDataException(
                    Resources.CannotGenerateASingleMetadataAndNoMultipleMetadataAvailable);
            }
        }

        /// <summary>
        /// Provides a single trigger meta data object with the base trigger data
        /// already filled in.
        /// If a trigger cannot be represented by a single meta data object, then
        /// the <see cref="MetaData"/> property should be overridden and a
        /// collection of metadata objects returned from there.
        /// </summary>
        public virtual TriggerMetaData PrimaryMetaData
        {
            get
            {
                var meta = new TriggerMetaData
                {
                    Start = Start,
                    End = End,
                    Priority = Priority,
                    Mode = Mode,
                    IsUserTrigger = IsUserTrigger,
                    TimeZoneId = TimeZoneId,
                    UtcOffset = UtcOffset
                };
                
                return meta;
            }
        }

        /// <summary>
        /// Event fired whenever the trigger's timing data has been updated.
        /// This means that, for any implementations of ITrigger, if the
        /// trigger is modified in such a way that its timing has been
        /// altered (eg. its Start or End date has changed; an 
        /// implementation-specific timing-related member has been changed),
        /// this event will be fired.
        /// </summary>
        [field: NonSerialized]
        public event HandleTriggerTimingUpdated TriggerTimingUpdated;

        /// <summary>
        /// Gets the next point in time that this trigger is
        /// activated after the given date/time.
        /// </summary>
        /// <param name="cal">The calendar to use when determining
        /// the trigger's event prediction.</param>
        /// <param name="after">The datetime after which the next
        /// trigger activation time is required.</param>
        /// <returns>The next point in time that this trigger is
        /// activated, or DateTime.MaxValue if it is never again
        /// activated.</returns>
        public abstract ITriggerInstance GetNextInstance(DateTime after);

        #endregion

        #region BaseTrigger-specific methods & members

        /// <summary>
        /// The default priority to use if one is not provided
        /// </summary>
        protected const int DEFAULT_PRIORITY = 1;

        /// <summary>
        /// The latest time that should be checked in a repeating trigger.
        /// This is a placeholder to enable some optimisations to be fairly
        /// easily implemented in subclasses which support it.
        /// </summary>
        protected static readonly DateTime LATEST_TIME = DateTime.MaxValue;

        /// <summary>
        /// Resolves the given date time such that, if it is not a boundary
        /// date, it loses any component more specific than 'second'.
        /// </summary>
        /// <param name="date">The date to resolve</param>
        /// <returns>The trigger date with the millisecond and below time
        /// components filtered out, or an unmolested boundary date.</returns>
        private DateTime ResolveTriggerDate(DateTime date)
        {
            if (date == DateTime.MinValue || date == DateTime.MaxValue)
                return date;
            return new DateTime((date.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);
        }

        /// <summary>
        /// Fires an event to indicate that timing has been updated for this
        /// trigger... weirdly, there's no mechanism (bar something like this)
        /// to enable subclasses to fire events defined in a superclass.
        /// </summary>
        protected void FireTimingUpdatedEvent()
        {
            if (TriggerTimingUpdated != null)
                TriggerTimingUpdated(this);
        }

        /// <summary>
        /// Attempts to get the calendar with the given ID.
        /// This will fail if : <list>
        /// <item>This trigger is not assigned to a schedule -or-</item>
        /// <item>The owner schedule is not assigned to a scheduler -or-</item>
        /// <item>A calendar with the given ID could not be found on the
        /// IScheduleStore associated with the owning scheduler.</item>
        /// </list>
        /// </summary>
        /// <param name="id">The ID of the required calendar.</param>
        /// <returns>The calendar associated with the given ID in the
        /// schedule store associated with the owning scheduler.</returns>
        /// <exception cref="UnassignedItemException">If the trigger is not assigned
        /// to a schedule, or the owning schedule is not assigned to a scheduler.
        /// </exception>
        /// <exception cref="InvalidDataException">If no calendar could be found which
        /// corresponded to the ID set in this trigger.</exception>
        protected ICalendar GetCalendarWithId(int id)
        {
            ISchedule sched = this.Schedule;
            if (sched == null)
            {
                throw new UnassignedItemException(
                    Resources.ThisTriggerNeedsToBeAssignedToAScheduleBeforeUse);
            }
            IScheduler scheduler = sched.Owner;
            if (scheduler == null)
            {
                throw new UnassignedItemException(
                    string.Format(Resources.TheSchedule0IsNotAssignedToASchedulerThisIsRequiredBeforeItCanBeUsed, sched.Name));
            }
            ICalendar cal = scheduler.Store.GetCalendar(id);
            if (cal == null)
            {
                throw new InvalidDataException(string.Format(Resources.NoCalendarFoundWithTheID0, id));
            }
            return cal;
        }

        /// <summary>
        /// Creates a trigger instance for the given date time, using the base
        /// mode set in this trigger.
        /// </summary>
        /// <param name="when">The date/time for which the instance is 
        /// required.</param>
        /// <returns>A trigger instance referring to this trigger and set for
        /// the given time and the base mode currently set in this trigger.
        /// </returns>
        protected ITriggerInstance CreateInstance(DateTime when)
        {
            return CreateInstance(when, Mode);
        }

        /// <summary>
        /// Creates a trigger instance for the given date time, using the
        /// specified mode.
        /// </summary>
        /// <param name="when">The date/time for which the instance is 
        /// required.</param>
        /// <param name="mode">The required mode of the instance</param>
        /// <returns>A trigger instance referring to this trigger and set for
        /// the given time and mode.
        /// </returns>
        protected ITriggerInstance CreateInstance(DateTime when, TriggerMode mode)
        {
            return new BaseTriggerInstance(this, when, mode);
        }

        /// <summary>
        /// Gets a copy of this base trigger which has the same core values
        /// as this trigger but is not assigned to any schedule or trigger 
        /// group.
        /// </summary>
        /// <returns>An unassigned base trigger with the same values as this
        /// trigger.</returns>
        /// <remarks>The runtime type of the returned trigger will be the same
        /// as the runtime type of this trigger.</remarks>
        public virtual ITrigger Copy()
        {
            BaseTrigger bt = (BaseTrigger)base.MemberwiseClone();

            // de-assign from both schedule and group
            bt._sched = null;
            bt._group = null;

            // remove any event listeners.
            bt.TriggerTimingUpdated = null;

            return bt;
        }

        #endregion

    }
}
