namespace BluePrism.Scheduling.Events
{
    /// <summary>
    /// The arguments which detail a 'triggered' event - fired when a
    /// schedule is triggered.
    /// </summary>
    public class TriggeredEventArgs : ScheduleEventArgs
    {
        // The trigger instance which caused the event
        private ITriggerInstance _instance;

        // The type of execution mode that the scheduler was in when
        // the instance event was fired.
        private ExecutionMode _mode;

        /// <summary>
        /// The trigger instance which caused the event to be fired.
        /// </summary>
        public ITriggerInstance Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// The execution mode of the scheduler when the triggered
        /// event was fired.
        /// </summary>
        public ExecutionMode Mode
        {
            get { return _mode; }
        }

        /// <summary>
        /// Creates a new event args object detailing the triggered
        /// event for the given trigger instance.
        /// </summary>
        /// <param name="inst">The trigger instance which caused the
        /// event detailed by this args object to be fired.</param>
        public TriggeredEventArgs(ITriggerInstance inst, ExecutionMode mode)
            : base(inst.Trigger.Schedule)
        {
            _instance = inst;
            _mode = mode;
        }
    }
}
