using System.Collections.Generic;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Delegate to handle the name of a schedule changing. Note
    /// that this event is called <em>before</em> the name is actually
    /// changed, so that the old name can be picked up from the schedule
    /// as it is currently.
    /// </summary>
    /// <param name="source">The source of the name change - ie.
    /// the schedule whose name is being changed.</param>
    /// <param name="name">The new name for the schedule.</param>
    public delegate void ScheduleRenameEventHandler(ScheduleRenameEventArgs args);

    /// <summary>
    /// Delegate to handle a schedule event
    /// </summary>
    /// <param name="source">The source of the event - the schedule on 
    /// which the trigger has changed.</param>
    /// <param name="args">The arguments providing further detail of
    /// the event.</param>
    public delegate void ScheduleEventHandler(ScheduleEventArgs args);

    /// <summary>
    /// Interface describing a schedule - loosely a job associated with
    /// a collection of triggers.
    /// A schedule has a name which must be unique within the scheduler that
    /// it is registered with.
    /// </summary>
    public interface ISchedule
    {
        /// <summary>
        /// Event fired when this schedule's name is changing. This must
        /// be called so that any schedulers with this schedule registered
        /// can update their data with the new name.
        /// </summary>
        event ScheduleRenameEventHandler NameChanging;

        /// <summary>
        /// Event fired when a trigger handling this schedule is changed
        /// such that the timing for the schedule is now different.
        /// This occurs if a trigger is added / removed to / from the
        /// trigger group, or if a trigger is modified such that its timing
        /// calculation changes.
        /// </summary>
        event ScheduleEventHandler TriggersUpdated;

        /// <summary>
        /// The name of the schedule - this should be unique within the context
        /// of the scheduler that it is registered with.
        /// </summary>
        /// <remarks>Note that the 'set' is only here so that implementations
        /// written in VB can have a setter - it's not mandatory in order to
        /// implement this interface, but necessary due to the restrictions
        /// in place within VB.</remarks>
        string Name { get; set; }

        /// <summary>
        /// A short description of this schedule.
        /// </summary>
        /// <remarks>Note that the 'set' is only here so that implementations
        /// written in VB can have a setter - it's not mandatory in order to
        /// implement this interface, but necessary due to the restrictions
        /// in place within VB.</remarks>
        string Description { get; set; }

        /// <summary>
        /// Executes this schedule
        /// </summary>
        /// <param name="log">The log on which the schedule execution should
        /// be recorded.</param>
        void Execute(IScheduleLog log);

        /// <summary>
        /// Gets a collection of logs representing this schedule's running
        /// instances in this schedule's owner scheduler, or in any other
        /// scheduler that this schedule may be aware of.
        /// Note that this log need not be updatable (ie. all mutator
        /// methods may throw an exception).
        /// </summary>
        /// <returns>A collection of bare bones schedule logs representing
        /// the running instances.</returns>
        ICollection<IScheduleLog> GetRunningInstances();

        /// <summary>
        /// Aborts the schedule if it is executing; has no effect if the 
        /// schedule is not currently executing.
        /// </summary>
        /// <param name="reason">The reason that the schedule is being aborted.
        /// </param>
        void Abort(string reason);

        /// <summary>
        /// Creates a copy of this schedule of the same runtime type, with the
        /// same value for its triggers and any further implementation-specific
        /// values, but that is not assigned to any scheduler - ie. the
        /// <see cref="Owner"/> property of the copy should be null, and its
        /// events are refreshed - ie. any listeners listening to events on
        /// the source schedule are <em>not</em> registered with the events
        /// on the copied schedule.
        /// </summary>
        /// <returns>An unassigned copy of this schedule with all data intact
        /// and separated from this schedule such that changes made in one are
        /// not reflected in the other (ie. a deep clone).</returns>
        ISchedule Copy();

        /// <summary>
        /// <para>
        /// Tells the schedule that a misfire of a trigger has occurred, giving
        /// the reason for the misfire, and seeking guidance about what to do
        /// with the instance which misfired.
        /// </para><para>
        /// This may be called multiple times for the same instance if, for
        /// example, the misfire reason is <c>ModeWasIndeterminate</c>, and this
        /// method returns <c>SuppressIndeterminateInstance</c>, it will be called
        /// again with the misfire reason: <c>ModeWasSuppress</c>.
        /// </para>
        /// </summary>
        /// <param name="instance">The instance which was the target of the misfire.
        /// </param>
        /// <param name="reason">The reason for the misfire.</param>
        /// <returns>The action to perform as a result of the misfire.</returns>
        TriggerMisfireAction Misfire(ITriggerInstance instance, TriggerMisfireReason reason);

        /// <summary>
        /// The trigger group containing all the triggers which are responsible
        /// for determining when this schedule is fired.
        /// </summary>
        ITriggerGroup Triggers { get; }

        /// <summary>
        /// The scheduler which operates as the owner of this schedule.
        /// </summary>
        IScheduler Owner { get; set; }

        /// <summary>
        /// Sets the trigger on this schedule to the given trigger. Note that
        /// this will remove any existing triggers and replace them with the
        /// given one.
        /// </summary>
        /// <param name="trigger">The trigger to use on this schedule.</param>
        /// <returns>true if the setting of the given trigger represents a
        /// change to this schedule; false if the trigger being set is the
        /// only one currently set in the schedule, and thus this schedule
        /// has not changed.</returns>
        /// <remarks>This will fire a <see cref="TriggerUpdated"/> event if
        /// the timing data for this schedule has been changed as a result
        /// of this method being called.</remarks>
        bool SetTrigger(ITrigger trigger);

        /// <summary>
        /// Adds the given trigger to this schedule, if it is not already
        /// held by it.
        /// </summary>
        /// <param name="trigger">The trigger to add to this schedule.</param>
        /// <returns>true if this schedule was changed as a result of adding
        /// the given trigger. false if the schedule already held the given
        /// trigger, and was thus not changed by this method.</returns>
        /// <remarks>This will fire a <see cref="TriggerUpdated"/> event if
        /// the timing data for this schedule has been changed as a result
        /// of this method being called.</remarks>
        bool AddTrigger(ITrigger trigger);

        /// <summary>
        /// Removes the given trigger from this schedule, if it is currently
        /// assigned to it.
        /// </summary>
        /// <param name="trigger">The trigger to remove from this schedule.
        /// </param>
        /// <returns>true if the specified trigger was found and removed from
        /// this schedule.</returns>
        /// <remarks>This will fire a <see cref="TriggerUpdated"/> event if
        /// the timing data for this schedule has been changed as a result
        /// of this method being called.</remarks>
        bool RemoveTrigger(ITrigger trigger);

    }
}
