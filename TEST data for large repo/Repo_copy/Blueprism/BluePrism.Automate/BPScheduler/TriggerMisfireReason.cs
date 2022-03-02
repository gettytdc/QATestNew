namespace BluePrism.Scheduling
{
    /// <summary>
    /// Enumeration of the reasons why a trigger instance might misfire.
    /// </summary>
    public enum TriggerMisfireReason
    {
        /// <summary>
        ///  Zero-value indicates that there is no reason for a misfire.
        /// </summary>
        None = 0,

        /// <summary>
        /// The mode of the instance was indeterminate, meaning the scheduler
        /// was unable to ascertain whether to fire or suppress the instance
        /// </summary>
        ModeWasIndeterminate,

        /// <summary>
        /// The trigger instance suppressed the trigger, meaning that the
        /// trigger instance will not be fired.
        /// </summary>
        /// <remarks>Typically, the return value for a misfire in this mode
        /// is ignored - if the mode is set to suppress, the schedule will
        /// not be executed in this instance</remarks>
        ModeWasSuppress,

        /// <summary>
        /// The instance should have activated but the scheduler was paused
        /// at the time of its activation
        /// </summary>
        /// <remarks>Return values from this misfire are currently ignored -
        /// the schedule will not be executed again for the instance specified
        /// in the misfire.</remarks>
        ScheduleAlreadyExecutedDuringResume,

        /// <summary>
        /// The instance should have activated but the scheduler was either
        /// not loaded / initialised or just not started at the time of its activation
        /// </summary>
        /// <remarks>Return values from this misfire are currently ignored -
        /// the schedule will not be executed again for the instance specified
        /// in the misfire.</remarks>
        ScheduleAlreadyExecutedDuringStartup,

        /// <summary>
        /// An earlier instance of the schedule is still running, either in the
        /// same scheduler or in a different one which the schedule is aware of
        /// </summary>
        EarlierScheduleInstanceStillRunning,

        /// <summary>
        /// The schedule instance that was trigger has already been executed or
        /// is currently being executed by another scheduler.
        /// Again, the return value from this misfire is ignored - a schedule
        /// instance can only be run once within an environment.
        /// </summary>
        ScheduleInstanceAlreadyExecuted
    }

}
