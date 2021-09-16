using System;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Placeholder class for schedule events allowing us to change
    /// the data passed as part of an event without changing the public
    /// interface to the events.
    /// </summary>
    public class ScheduleEventArgs : EventArgs
    {
        /// <summary>
        /// The schedule which is the source of the event.
        /// </summary>
        public ISchedule SourceSchedule { get; }

        /// <summary>
        /// Creates a new ScheduleEventArgs object using the given 
        /// schedule as its source.
        /// </summary>
        /// <param name="src">The source of the event to use.
        /// </param>
        public ScheduleEventArgs(ISchedule src) => SourceSchedule = src;
    }
}
