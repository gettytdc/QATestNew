using System;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Interface describing a basic log for a schedule.
    /// This provides a point for implementations to capture and report on
    /// the running of a schedule. It is expected to be stored at least that
    /// a schedule has executed in order to allow the scheduler to discover
    /// whether to run a schedule or not.
    /// </summary>
    public interface IScheduleLog
    {
        /// <summary>
        /// The schedule that this log represents.
        /// </summary>
        ISchedule Schedule { get; }

        /// <summary>
        /// The name of the scheduler which executed or is executing this schedule
        /// instance, or an empty string if this instance is in the future or if
        /// the scheduler name is not known.
        /// </summary>
        string SchedulerName { get; }

        /// <summary>
        /// The instance time of this log - ie. the time at which the trigger
        /// was configured to activate... even if the start time was different.
        /// This should always return a value - a log should not exist without
        /// a valid instance time.
        /// </summary>
        DateTime InstanceTime { get; }

        /// <summary>
        /// The time that this log represented by this object was started,
        /// or DateTime.MaxValue if this log has not yet been started.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// The time that this log was stopped, or DateTime.MaxValue if this
        /// log has not yet been stopped.
        /// </summary>
        DateTime EndTime { get; }

        /// <summary>
        /// Gets the last update or pulse on this log. This DateTime should
        /// always be specified with UTC time.
        /// </summary>
        DateTime LastUpdated { get; }

        /// <summary>
        /// Checks if this log has already been 'completed' or 'terminated'
        /// </summary>
        /// <returns>true if this log has been completed or terminated,
        /// false if it is unstarted or still in progress.</returns>
        bool IsFinished();

        /// <summary>
        /// Method used to tell the log that the schedule represented by
        /// this log has started.
        /// </summary>
        /// <exception cref="AlreadyStartedException">If the schedule log
        /// could not be started since it has already been started.
        /// </exception>
        /// <exception cref="InvalidOperationException">If this log is
        /// not updatable</exception>
        void Start();

        /// <summary>
        /// Records on this log that the schedule has completed.
        /// </summary>
        /// <param name="success">True to indicate that the log is being
        /// closed because the execution was successful; false to indicate
        /// that the execution failed.</param>
        /// <exception cref="ScheduleFinishedException">If this log is 
        /// already closed.</exception>
        /// <exception cref="InvalidOperationException">If this log is
        /// not updatable</exception>
        void Complete();

        /// <summary>
        /// Records on this log that the schedule has terminated, giving a
        /// reason why it was terminated.
        /// No exception is reported in this log.
        /// </summary>
        /// <param name="reason">The user-presentable reason why the schedule
        /// terminated.</param>
        /// <exception cref="ScheduleFinishedException">If this log is 
        /// already closed.</exception>
        /// <exception cref="InvalidOperationException">If this log is
        /// not updatable</exception>
        void Terminate(string reason);

        /// <summary>
        /// Records on this log that the schedule has terminated, giving a
        /// reason why it was terminated and details of the exception which
        /// caused the termination.
        /// </summary>
        /// <param name="reason">The user-presentable reason why the schedule
        /// terminated.</param>
        /// <param name="ex">The exception which caused or provides more data
        /// on this termination, null if no exception is appropriate.</param>
        /// <exception cref="ScheduleFinishedException">If this log is 
        /// already closed.</exception>
        /// <exception cref="InvalidOperationException">If this log is
        /// not updatable</exception>
        void Terminate(string reason, Exception ex);

        /// <summary>
        /// Sends a heartbeat signal to the log indicating that it is still
        /// considered to be in a running state by the scheduler.
        /// </summary>
        /// <exception cref="ScheduleFinishedException">If the schedule has
        /// finished and therefore the heartbeat cannot be sent.</exception>
        /// <exception cref="InvalidOperationException">If this log is
        /// not updatable</exception>
        void Pulse();

    }
}
