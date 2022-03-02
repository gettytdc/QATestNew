using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BluePrism.Scheduling.Triggers;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Class to define a basic schedule.
    /// This provides the work for holding the scheduler owner of the schedule
    /// and the group of triggers which act on this schedule.
    /// Also, the NameChanging and TriggersUpdated events are handled from here.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "bp", IsReference = true)]
    public abstract class BaseSchedule : DescribedNamedObject, ISchedule
    {
        // The scheduler which operates as the owner of this schedule
        [NonSerialized]
        private IScheduler _owner;

        // The group of triggers defined for this schedule.
        [DataMember]
        private TriggerGroup _triggerGroup;

        /// <summary>
        /// Creates a new base schedule with the given owner.
        /// </summary>
        /// <param name="owner"></param>
        public BaseSchedule(IScheduler owner)
        {
            _owner = owner;
            _triggerGroup = new TriggerGroup();
            _triggerGroup.Schedule = this;

            _triggerGroup.TriggerTimingUpdated += HandleTriggerGroupTimingUpdated;
        }

        /// <summary>
        /// Handles the timing of the trigger group being updated.
        /// </summary>
        /// <param name="group">The trigger group whose timing is being updated.
        /// </param>
        private void HandleTriggerGroupTimingUpdated(ITrigger group)
        {
            MarkDataChanged("Timing", null, group);
            OnTriggersUpdated(new ScheduleEventArgs(this));
        }

        /// <summary>
        /// Raises the <see cref="TriggersUpdated"/> event
        /// </summary>
        /// <param name="e">The args detailing the event.</param>
        protected virtual void OnTriggersUpdated(ScheduleEventArgs e)
        {
            ScheduleEventHandler handler = this.TriggersUpdated;
            if (handler != null)
                handler(e);
        }

        /// <summary>
        /// Raises the <see cref="NameChanging"/> event
        /// </summary>
        /// <param name="e">The args detailing the event</param>
        protected virtual void OnNameChanging(ScheduleRenameEventArgs e)
        {
            ScheduleRenameEventHandler handler = this.NameChanging;
            if (handler != null)
                handler(e);
        }
        
        /// <summary>
        /// Gets a copy of this schedule, with the same owner and a copy
        /// of all the triggers on this schedule.
        /// </summary>
        /// <returns>A copy of this schedule.</returns>
        public new virtual ISchedule Copy()
        {
            BaseSchedule bs = (BaseSchedule)base.Copy();

            // Remove any event handlers in the schedule.
            bs.NameChanging = null;
            bs.TriggersUpdated = null;

            // New trigger group please...
            bs._triggerGroup = (TriggerGroup)_triggerGroup.Copy();

            // Set the owner schedule into it.
            bs._triggerGroup.Schedule = bs;

            // We want to ensure that the copy is listening for trigger group
            // timing updates - this is the state at creation of a schedule /
            // group relationship so that's where we need to be now.
            bs._triggerGroup.TriggerTimingUpdated += bs.HandleTriggerGroupTimingUpdated;

            return bs;
        }

        /// <summary>
        /// Overrides the Name in DescribedNamedObject to enable the appropriate
        /// event to be fired.
        /// </summary>
        public override string Name
        {
            get { return base.Name; }
            set
            {
                if (Object.Equals(Name, value))
                    return;
                OnNameChanging(new ScheduleRenameEventArgs(this, this.Name, value));
                base.Name = value;
            }
        }

        /// <summary>
        /// Event fired immediately before the name of this schedule 
        /// is changed.
        /// </summary>
        [field: NonSerialized]
        public event ScheduleRenameEventHandler NameChanging;

        /// <summary>
        /// Event fired when the trigger/s associated with this schedule
        /// have changed their timing data.
        /// </summary>
        [field: NonSerialized]
        public event ScheduleEventHandler TriggersUpdated;

        /// <summary>
        /// Executes this schedule.
        /// </summary>
        /// <param name="log">The log to use to log events occurring in the execution
        /// of the schedule.</param>
        public abstract void Execute(IScheduleLog log);

        /// <summary>
        /// Aborts this schedule if it is currently executing. Has no effect otherwise.
        /// </summary>
        /// <param name="reason">The reason that this schedule is being aborted.</param>
        public abstract void Abort(string reason);

        /// <summary>
        /// Handles a scheduler misfire, indicating to the caller what to do about
        /// the misfire in this instance.
        /// </summary>
        /// <param name="instance">The trigger instance which has misfired.</param>
        /// <param name="reason">The reason for the misfire.</param>
        /// <returns>The action which should be taken with regards to this misfire.
        /// </returns>
        /// <exception cref="InvalidOperationException">If the misfire reason was
        /// unrecognised.</exception>
        public abstract TriggerMisfireAction Misfire(
            ITriggerInstance instance, TriggerMisfireReason reason);

        /// <summary>
        /// The group of triggers registered with this schedule.
        /// </summary>
        public virtual ITriggerGroup Triggers
        {
            get { return _triggerGroup; }
        }

        /// <summary>
        /// The owner of this schedule.
        /// </summary>
        public virtual IScheduler Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        /// <summary>
        /// Sets the only trigger on this schedule to the given value.
        /// </summary>
        /// <param name="trigger">The trigger to set in this schedule.
        /// </param>
        /// <returns>true if this method has changed the triggers associated
        /// with this schedule; false if the given trigger was already the
        /// only trigger associated with this schedule, and this schedule has
        /// therefore not changed.</returns>
        public virtual bool SetTrigger(ITrigger trigger)
        {
            if (_triggerGroup.Count == 1 && _triggerGroup.Contains(trigger))
                return false;
            _triggerGroup.Clear();
            return AddTrigger(trigger);
        }

        /// <summary>
        /// Adds the given trigger to the triggers already associated with this schedule,
        /// if it does not already exist on the trigger.
        /// </summary>
        /// <param name="trigger">The trigger to add to this schedule.</param>
        /// <returns>true if the trigger setup has changed as a result of adding the
        /// given trigger to the schedule; false if the given trigger was already
        /// associated with this schedule and this schedule has thus not changed as a
        /// result.</returns>
        public virtual bool AddTrigger(ITrigger trigger)
        {
            if (_triggerGroup.Add(trigger))
            {
                trigger.Schedule = this;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the given trigger to the triggers associated with this schedule.
        /// </summary>
        /// <param name="trigger">The trigger to remove from this schedule if it is
        /// currently associated with it.</param>
        /// <returns>true if the given trigger was associated with this schedule and has
        /// thus been removed; false if it was not originally associated with it and this
        /// schedule has thus not changed.</returns>
        public virtual bool RemoveTrigger(ITrigger trigger)
        {
            if (_triggerGroup.Remove(trigger))
            {
                trigger.Schedule = null;
                return true;
            }
            return false;
        }

        public abstract ICollection<IScheduleLog> GetRunningInstances();
    }
}
