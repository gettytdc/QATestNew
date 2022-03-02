using System;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Interface describing the instance of a trigger
    /// </summary>
    public interface ITriggerInstance
    {
        /// <summary>
        /// Gets the trigger that this is an instance of
        /// </summary>
        ITrigger Trigger { get; }

        /// <summary>
        /// When this trigger instance is fired
        /// </summary>
        DateTime When { get; }

        /// <summary>
        /// The mode of this trigger - ie. whether it is firing the
        /// job or suppressing it.
        /// </summary>
        TriggerMode Mode { get; }

        /// <summary>
        /// Flag indicating whether this instance has been activated or not.
        /// This can usually be ascertained by checking the log in the store
        /// that the scheduler which ultimately owns this thread is using
        /// as a backing store.
        /// </summary>
        bool HasActivated();

        /// <summary>
        /// Flag indicating whether this instance has a valid activation time.
        /// An activation time is deemed invalid when it falls between a time zone's
        /// spring forward gap, i.e. when its clock move forward into daylight
        /// savings time.
        /// </summary>
        bool IsTimeValid();
    }
}
