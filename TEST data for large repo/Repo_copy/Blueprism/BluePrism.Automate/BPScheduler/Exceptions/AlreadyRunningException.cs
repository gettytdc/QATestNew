using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown when a schedule is executed while it is currently
    /// already running, whether in the same scheduler or otherwise depending
    /// on the exclusivity rules of the scheduler / schedule itself.
    /// </summary>
    [Serializable]
    public class AlreadyRunningException : ScheduleException
    {
        /// <summary>
        /// Creates a new exception with no message.
        /// </summary>
        public AlreadyRunningException() : base() { }

        /// <summary>
        /// Creates a new exception with the given message.
        /// </summary>
        /// <param name="msg">The message detailing the exception</param>
        public AlreadyRunningException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public AlreadyRunningException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected AlreadyRunningException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion


    }
}
