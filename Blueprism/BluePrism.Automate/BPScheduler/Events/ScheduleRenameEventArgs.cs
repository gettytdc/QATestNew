namespace BluePrism.Scheduling
{
    /// <summary>
    /// Event args for the renaming of a schedule.
    /// </summary>
    public class ScheduleRenameEventArgs : ScheduleEventArgs
    {
        // The old and new names for the schedule.
        private string _old;
        private string _new;

        /// <summary>
        /// Creates a new event args object for a schedule rename event.
        /// </summary>
        /// <param name="sched">The schedule whose name has changed.
        /// </param>
        /// <param name="oldName">The old name for the schedule.</param>
        /// <param name="newName">The new name for the schedule.</param>
        public ScheduleRenameEventArgs(ISchedule sched, string oldName, string newName)
            : base(sched)
        {
            _old = oldName;
            _new = newName;
        }

        /// <summary>
        /// The old name for this schedule.
        /// </summary>
        public string OldName
        {
            get { return _old; }
        }

        /// <summary>
        /// The new name for this schedule.
        /// </summary>
        public string NewName
        {
            get { return _new; }
        }
    }
}
