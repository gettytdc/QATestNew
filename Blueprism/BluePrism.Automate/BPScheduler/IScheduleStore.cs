using System;
using System.Collections.Generic;
using BluePrism.BPCoreLib.Collections;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Interface describing a backing store for a scheduler
    /// </summary>
    public interface IScheduleStore : IScheduleCalendarStore
    {
        /// <summary>
        /// Event fired when the backing store used by this object has been
        /// updated. The store makes no distinction between pure data
        /// changes and timing changes - just a blanket 'store updated'.
        /// </summary>
        event StoreDataChanged SchedulesUpdated;

        /// <summary>
        /// The scheduler which owns this store, if this store is currently
        /// registered with a scheduler.
        /// </summary>
        IScheduler Owner { get; set; }

        /// <summary>
        /// Gets the active schedules from the store.
        /// </summary>
        /// <returns>A collection of schedules which represent all
        /// the active schedules held in this store.</returns>
        ICollection<ISchedule> GetActiveSchedules();

        /// <summary>
        /// Gets the schedule with the given ID from the store.
        /// </summary>
        /// <param name="name">The name of the required schedule.</param>
        /// <returns>The schedule with the given name.</returns>
        ISchedule GetSchedule(string name);

        /// <summary>
        /// Saves the given schedule to this store.
        /// </summary>
        /// <param name="schedule">The schedule to save.</param>
        /// <remarks>
        /// This will either create or update the schedule depending on whether it
        /// is currently saved to the database or not.
        /// </remarks>
        void SaveSchedule(ISchedule schedule);

        /// <summary>
        /// Deletes the given schedule from the store.
        /// </summary>
        /// <param name="schedule">The schedule which should be deleted.</param>
        void DeleteSchedule(ISchedule schedule);

        /// <summary>
        /// Retires the given schedule from the store.
        /// </summary>
        /// <param name="schedule">The schedule which should be retired.</param>
        void RetireSchedule(ISchedule schedule);

        /// <summary>
        /// Unretires the given schedule from the store.
        /// </summary>
        /// <param name="schedule">The schedule which should be unretired.</param>
        void UnretireSchedule(ISchedule schedule);

        /// <summary>
        /// Gets a historical log for the given schedule at the given instant
        /// in time.
        /// </summary>
        /// <param name="schedule">The schedule for which the log is required.
        /// </param>
        /// <param name="instant">The moment in time for which the log is
        /// required.</param>
        /// <returns>A schedule log representing the execution of the given
        /// schedule, triggered at the given instant in time... or null if
        /// the schedule was not executed at the given instant.</returns>
        /// <remarks>The log returned by this method should not allow the log
        /// to be modified by any calling classes - eg. Start() and Stop()
        /// should throw exceptions.</remarks>
        IScheduleLog GetLog(ISchedule schedule, DateTime instant);

        /// <summary>
        /// The server time zone ID from the TimeZoneInfo library.
        /// </summary>
        /// <returns>
        /// The string ID of the server's time zone.
        /// </returns>
        TimeZoneInfo GetServerTimeZone();

        /// <summary>
        /// Gets the historical logs representing the given schedule within
        /// the given points in time (exclusive).
        /// </summary>
        /// <param name="schedule">The schedule whose logs are required.</param>
        /// <param name="after">The date/time after which the logs are 
        /// required.</param>
        /// <param name="before">The date/time before which the logs are
        /// required.</param>
        /// <returns>A non-null set of schedule logs which represent the
        /// logs for the given schedule's executions triggered between the
        /// specified dates.</returns>
        IBPSet<IScheduleLog> GetLogs(ISchedule schedule, DateTime after, DateTime before);

        /// <summary>
        /// Gets an open (and modifiable) log for the given trigger instance
        /// </summary>
        /// <returns>An open schedule log (which allows modification) representing
        /// the schedule execution initiated by the given trigger instance.
        /// </returns>
        /// <exception cref="AlreadyActivatedException">If a log for the given
        /// trigger instance has already been created, indicating that the schedule
        /// has already been activated.</exception>
        IScheduleLog CreateLog(ITriggerInstance instance);

        DateTime TriggerSchedule(ISchedule schedule);
        DateTime TriggerSchedule(ISchedule schedule, DateTime when);
    }
}
