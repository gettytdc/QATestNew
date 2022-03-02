using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown when a trigger to be activated is found to be
    /// indeterminate at runtime, and thus the mode that the trigger is
    /// activated in cannot be determined.
    /// </summary>
    [Serializable]
    public class IndeterminateTriggerException : ScheduleException
    {
        /// <summary>
        /// Creates a new exception indicating an indeterminate-mode
        /// trigger with no message.
        /// </summary>
        public IndeterminateTriggerException() : base() { }

        /// <summary>
        /// Creates a new exception indicating an indeterminate-mode
        /// trigger with the specified message.
        /// </summary>
        /// <param name="msg">The message containing further details
        /// regarding this exception</param>
        public IndeterminateTriggerException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public IndeterminateTriggerException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected IndeterminateTriggerException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion
    }
}
