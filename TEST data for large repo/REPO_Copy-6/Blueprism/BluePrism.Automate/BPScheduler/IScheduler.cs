namespace BluePrism.Scheduling
{
    /// <summary>
    /// Delegate used to write trace log entries for the scheduler.
    /// This will receive status messages from the scheduler as it
    /// goes about its execution.
    /// </summary>
    /// <param name="message">The message </param>
    public delegate void LogStatus(string message);

    /// <summary>
    /// Interface describing the scheduler.
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// Event fired when the status of the scheduler has been updated and
        /// it wishes to inform any interested parties.
        /// </summary>
        event LogStatus StatusUpdated;

        /// <summary>
        /// Event fired when status of scheduler has been updated and
        /// info log is required.
        /// </summary>
        event LogStatus AddInfoLog;

        /// <summary>
        /// The name of this scheduler.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Starts the scheduler's background thread which awaits triggers being
        /// fired and executes the resultant jobs.
        /// </summary>
        /// <param name="millisToCheck">The number of milliseconds back to check
        /// to see if any schedules which should have executed have not.</param>
        void Start(int millisToCheck);

        /// <summary>
        /// Suspends the background thread without closing it down. It can be
        /// resumed at any time by calling Resume()
        /// </summary>
        void Suspend();

        /// <summary>
        /// Checks if the scheduler is currently suspended or not.
        /// </summary>
        /// <returns>true if the scheduler's background thread is currently
        /// suspended; false if it is either unstarted, stopped or running.
        /// </returns>
        bool IsSuspended();

        /// <summary>
        /// Resumes the suspended scheduler
        /// </summary>
        void Resume();

        /// <summary>
        /// Stops the background thread and closes it down. The scheduler must
        /// be 'Start()'ed in order to re-enable the background thread.
        /// </summary>
        void Stop();

        /// <summary>
        /// Checks if the scheduler's thread is running or not
        /// </summary>
        /// <returns>true if the background thread managed by this scheduler
        /// is running; false otherwise.</returns>
        bool IsRunning();

        /// <summary>
        /// The store used to retrieve and save information for this scheduler.
        /// </summary>
        IScheduleStore Store { get; set; }

        /// <summary>
        /// The job diary indicating when jobs registered with this schedule
        /// are due to run.
        /// </summary>
        IDiary Diary { get; }

    }
}
