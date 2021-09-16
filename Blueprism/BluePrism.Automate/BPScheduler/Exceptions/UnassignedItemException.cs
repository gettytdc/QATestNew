using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown when an item is asked to do something which it
    /// can only do when it is assigned to another object. This can be
    /// thrown, for example, if a trigger is asked to check a fire time
    /// for which it needs a calendar from its owner schedule, and it
    /// is not assigned to a schedule.
    /// </summary>
    [Serializable]
    public class UnassignedItemException : ScheduleException
    {
        /// <summary>
        /// Creates a new exception with no message
        /// </summary>
        public UnassignedItemException() : base() { }

        /// <summary>
        /// Creates a new exception with the given message
        /// </summary>
        /// <param name="msg"></param>
        public UnassignedItemException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The message with format placeholders</param>
        /// <param name="args">The arguments to populate the format message
        /// with.</param>
        public UnassignedItemException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes a name exists exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected UnassignedItemException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion


    }
}
