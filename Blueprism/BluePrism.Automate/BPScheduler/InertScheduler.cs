using BluePrism.Scheduling.Properties;
using System;
using System.Collections.Generic;
using BluePrism.BPCoreLib.Collections;

#pragma warning disable 67 // disable "this event is never used" warning...

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Inert scheduler - ie. a scheduler which acts as a holder for the store
    /// and a diary, but not for actually executing any schedules.
    /// </summary>
    /// <remarks>This should only be used when only interested in data and is
    /// intended as a lightweight replacement for the background thread scheduler
    /// in that instance only. Any attempt to run this as a scheduler will result
    /// in exceptions being thrown.</remarks>
    public class InertScheduler : IScheduler, IDiary
    {
        /// <summary>
        /// Creates a new InertScheduler with no store currently set.
        /// </summary>
        public InertScheduler() : this(null) { }

        /// <summary>
        /// Creates a new InertScheduler using the given store.
        /// </summary>
        /// <param name="store">The store from which to draw the scheduler data.
        /// </param>
        public InertScheduler(IScheduleStore store)
        {
            this.Store = store;
        }

        #region IScheduler Members

        /// <summary>
        /// Event never called...
        /// </summary>
        public event LogStatus StatusUpdated;

        public event LogStatus AddInfoLog;

        /// <summary>
        /// The name of the scheduler.
        /// </summary>
        public string Name
        {
            get { return "Inert Scheduler"; }
        }

        /// <summary>
        /// Throws an exception
        /// </summary>
        /// <param name="millisToCheck">ignored</param>
        /// <exception cref="InvalidOperationException">When called.</exception>
        public void Start(int millisToCheck)
        {
            throw new InvalidOperationException(Resources.ThisSchedulerIsInertItCannotBeOperated);
        }

        /// <summary>
        /// Throws an exception
        /// </summary>
        /// <param name="millisToCheck">ignored</param>
        /// <exception cref="InvalidOperationException">When called.</exception>
        public void Suspend()
        {
            throw new InvalidOperationException(Resources.ThisSchedulerIsInertItCannotBeOperated);
        }

        /// <summary>
        /// Checks if the scheduler is suspended. It is not.
        /// </summary>
        /// <returns>false</returns>
        public bool IsSuspended()
        {
            return false;
        }

        /// <summary>
        /// Throws an exception
        /// </summary>
        /// <param name="millisToCheck">ignored</param>
        /// <exception cref="InvalidOperationException">When called.</exception>
        public void Resume()
        {
            throw new InvalidOperationException(Resources.ThisSchedulerIsInertItCannotBeOperated);
        }

        /// <summary>
        /// Throws an exception
        /// </summary>
        /// <param name="millisToCheck">ignored</param>
        /// <exception cref="InvalidOperationException">When called.</exception>
        public void Stop()
        {
            throw new InvalidOperationException(Resources.ThisSchedulerIsInertItCannotBeOperated);
        }

        /// <summary>
        /// Checks if the scheduler is running. It is not.
        /// </summary>
        /// <returns>false</returns>
        public bool IsRunning()
        {
            return false;
        }

        private IScheduleStore _store;

        /// <summary>
        /// The schedule store used to hold schedules on this scheduler.
        /// </summary>
        public IScheduleStore Store
        {
            get { return _store; }
            set
            {
                _store = value;
                // Check that we're not already assigned to this store to
                // inhibit infinite loops
                if (value != null && !object.ReferenceEquals(this, value.Owner))
                    _store.Owner = this;
            }
        }

        /// <summary>
        /// The diary used by this scheduler.
        /// </summary>
        public IDiary Diary
        {
            get { return this; }
        }

        #endregion

        #region IDiary members

        /// <summary>
        /// Event never called.
        /// </summary>
        public event HandleDiaryUpdated DiaryUpdated;

        /// <summary>
        /// Gets the next activation time for any schedules held in this
        /// scheduler after the given date/time.
        /// </summary>
        /// <param name="after">The date/time after which the next activation
        /// time is required.</param>
        /// <returns>The next point in time at which an activation occurs on
        /// any schedule within this scheduler's store.</returns>
        public DateTime GetNextActivationTime(DateTime after)
        {
            DateTime earliest = DateTime.MaxValue;
            foreach (ISchedule sched in Store.GetActiveSchedules())
            {
                ITriggerInstance inst = sched.Triggers.GetNextInstance(after);
                if (inst != null && inst.When < earliest)
                    earliest = inst.When;
            }
            return earliest;
        }

        /// <summary>
        /// Gets all trigger instances which occur for the given date/time.
        /// </summary>
        /// <param name="date">The date/time for which instances are required.
        /// </param>
        /// <returns>The trigger instances which occur for the given date/time.
        /// </returns>
        public ICollection<ITriggerInstance> GetInstancesFor(DateTime date)
        {
            IBPSet<ITriggerInstance> instanceSet = new clsSet<ITriggerInstance>();

            // A trigger group can get all the instances for its contained
            // triggers between 2 exclusive datetime bounds... we want
            // all instances for a particular time... therefore...
            DateTime start = date.AddSeconds(-1);
            DateTime end = date.AddSeconds(1);
            foreach (ISchedule sched in Store.GetActiveSchedules())
                instanceSet.Union(sched.Triggers.GetInstances(start, end));

            return instanceSet;
        }

        #endregion

    }
}
