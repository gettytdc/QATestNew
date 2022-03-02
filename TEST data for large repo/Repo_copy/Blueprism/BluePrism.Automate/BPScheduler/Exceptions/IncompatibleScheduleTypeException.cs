using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown when a scheduler / schedule is asked to perform a
    /// task on an instance of a type it does not support.
    /// </summary>
    [Serializable]
    public class IncompatibleScheduleTypeException : ScheduleException
    {
        /// <summary>
        /// Creates an exception with no message.
        /// </summary>
        public IncompatibleScheduleTypeException() : base() { }

        /// <summary>
        /// Creates an exception with the given message.
        /// </summary>
        /// <param name="msg">The message detailing the exception.</param>
        public IncompatibleScheduleTypeException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public IncompatibleScheduleTypeException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected IncompatibleScheduleTypeException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion

    }
}
