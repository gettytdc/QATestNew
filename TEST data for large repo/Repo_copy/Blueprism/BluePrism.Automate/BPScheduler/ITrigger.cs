using System;
using System.Collections.Generic;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Delegate defining the method called when the trigger is modified
    /// in such a way that its timing data has changed.
    /// </summary>
    /// <param name="trigger">The trigger whose timing has been updated.
    /// </param>
    public delegate void HandleTriggerTimingUpdated(ITrigger trigger);

    /// <summary>
    /// The mode that a trigger can be set to.
    /// This will determine whether the trigger is running a scheduled
    /// job or suppressing it.
    /// </summary>
    public enum TriggerMode { Indeterminate = 0, Fire = 1, Suppress = 2, Stop = 3 }

    /// <summary>
    /// Class defining bounds for the triggers.
    /// </summary>
    public static class TriggerBounds
    {
        /// <summary>
        /// The latest supported date of this trigger.
        /// This is the date beyond which any trigger <em>doesn't have to</em> support.
        /// Note that triggers don't have to stop work at this date, but if their
        /// calculations are expensive, they must support at least up to this date.
        /// </summary>
        public static readonly DateTime LatestSupportedDate = new DateTime(2199, 12, 31, 23, 59, 59);
    }

    /// <summary>
    /// <para>
    /// Triggers are responsible for determining when a scheduled job should be fired / 
    /// suppressed.
    /// </para><para>
    /// A job can have many triggers assigned to it. If a number of triggers exist for
    /// a job at the same point in time, the priority of the trigger determines which
    /// one is 'active'.
    /// </para><para>
    /// If the active trigger is a 'Fire' trigger, then the job is executed. If it is
    /// a 'Suppress' trigger, then the job is skipped. This allows for a lot of
    /// flexibility in defining triggers.
    /// </para>
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// Event fired whenever the trigger's timing data has been updated.
        /// This means that, for any implementations of ITrigger, if the
        /// trigger is modified in such a way that its timing has been
        /// altered (eg. its Start or End date has changed; an 
        /// implementation-specific timing-related member has been changed),
        /// this event will be fired.
        /// </summary>
        event HandleTriggerTimingUpdated TriggerTimingUpdated;

        /// <summary>
        /// Flag to indicate whether this trigger is user-configurable or
        /// whether it is a system trigger, ie. not configurable by the user.
        /// </summary>
        bool IsUserTrigger { get; }

        /// <summary>
        /// The start date / time to initiate this trigger.
        /// Note that this only supports times down to second granularity.
        /// Any further detail (millisecond / Tick) will have been stripped
        /// by the trigger on setting of the value.
        /// </summary>
        DateTime Start { get; }

        /// <summary>
        /// The end date for this trigger - this will return
        /// DateTime.MaxValue if this trigger is never ending.
        /// Note that this end date is <em>inclusive</em>, meaning that
        /// if a trigger would fire on this date/time, then it will be
        /// fired. To disable this, the end date must be set to 1ms less
        /// than the required exclusive end date.
        /// Note that this only supports times down to second granularity.
        /// Any further detail (millisecond / Tick) will have been stripped
        /// by the trigger on setting of the value.
        /// </summary>
        DateTime End { get; }

        /// <summary>
        /// <para>
        /// The mode in which this trigger is set. This will determine
        /// whether this trigger fires a scheduled job, or suppresses
        /// the job from running.
        /// </para><para>
        /// Note that for a trigger group, the setting of this mode
        /// is ignored, since it must calculate the mode from its
        /// member triggers which are active at a given point in time.
        /// </para>
        /// </summary>
        TriggerMode Mode { get; }

        /// <summary>
        /// The priority of this trigger - only really has meaning
        /// when compared to other triggers. Note that a suppress
        /// trigger has a higher natural priority than a fire
        /// trigger - ie. if 2 triggers with the same priority exist
        /// for the same job at the same point in time, and one is
        /// a fire trigger and the other is a suppress trigger, the
        /// suppress trigger will be treated as if it had higher 
        /// priority.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// The collection of meta data which represents this trigger.
        /// Note that a trigger can be represented by several distinct
        /// pieces of metadata, this must return a collection, though
        /// it is unlikely that a single trigger will return more than
        /// once metadata object - certainly a group will be expected
        /// to do just that.
        /// </summary>
        ICollection<TriggerMetaData> MetaData { get; }

        /// <summary>
        /// Gets the primary meta data which represents this trigger.
        /// Note that if this trigger cannot be represented by a 
        /// single metadata object, this will return null, and the
        /// <see cref="MetaData"/> property should be used instead.
        /// </summary>
        TriggerMetaData PrimaryMetaData { get; }

        /// <summary>
        /// Gets the group that this trigger is a member of. A trigger
        /// can only be a direct member of at most a single group.
        /// This property will be null if a trigger is not a member of
        /// a group.
        /// </summary>
        ITriggerGroup Group { get; set; }

        /// <summary>
        /// The schedule that this trigger is assigned to. A trigger can only
        /// be assigned to a single schedule, although a schedule can be assigned
        /// many triggers.
        /// </summary>
        ISchedule Schedule { get; set; }

        /// <summary>
        /// Returns a copy of the <em>value</em> of this trigger, ensuring that
        /// the copy is not assigned to any schedule or group and that any 
        /// event listeners have been removed.
        /// If this trigger is a group itself, then its members will be copied
        /// and assigned to the newly copied group, but the group itself will
        /// not be assigned via this method.
        /// </summary>
        /// <returns>An unassigned copy of this trigger.</returns>
        ITrigger Copy();

        /// <summary>
        /// Gets the instance for the next point in time that this trigger
        /// is activated after the given date/time.
        /// </summary>
        /// <param name="after">The datetime after which the next
        /// trigger activation time is required.</param>
        /// <returns>The instance of this trigger detailing the next point in time
        /// that this trigger is activated, or null if no next instance of this
        /// trigger was found.</returns>
        /// <remarks>
        /// Usually, a return value of null indicates quite confidently that there
        /// are no more instances of this trigger. However, it may be that the
        /// calculation which determines the date within a particular period is
        /// expensive, and some arbitrary mechanism (eg. a calendar) is set up in
        /// such a way that it (a) cannot be predicted / algorithmified and
        /// (b) refuses several periods.
        /// As such, a return value of null indicates that there are no more 
        /// instances of the trigger until <em>at least</em> the date specified
        /// in <see cref="TriggerBounds.LatestSupportedDate"/>.
        /// </remarks>
        ITriggerInstance GetNextInstance(DateTime after);
    }
}
