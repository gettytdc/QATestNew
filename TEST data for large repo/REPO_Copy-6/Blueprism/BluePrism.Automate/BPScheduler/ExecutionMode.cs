namespace BluePrism.Scheduling
{
    /// <summary>
    /// Execution mode detailing which mode to execute schedules in.
    /// </summary>
    public enum ExecutionMode
    {
        /// <summary>
        /// Startup mode - ensures that any trigger events indicate to
        /// listeners that any fired triggers are for schedules in the
        /// past, executed as a result of checking back for missed schedules
        /// </summary>
        Startup,

        /// <summary>
        /// Resume mode - ensures that any trigger events indicate to
        /// listeners that fired triggers are for schedules in the
        /// past, executed as a result of checking back for missed schedules
        /// </summary>
        Resume,

        /// <summary>
        /// Normal mode - ensures that any triggered event listeners know
        /// that any fired triggers occurred while the thread was running
        /// normally
        /// </summary>
        Normal
    }

}
