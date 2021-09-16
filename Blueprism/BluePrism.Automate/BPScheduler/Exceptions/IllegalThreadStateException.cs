using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// The exception thrown when an operation on a thread is invalid, due
    /// to the current state of the thread.
    /// eg. if a thread is not running and a call is made to pause it, this
    /// exception may be thrown.
    /// </summary>
    [Serializable]
    public class IllegalThreadStateException : ScheduleException
    {
        /// <summary>
        /// Creates a new illegal thread state exception with no detail
        /// </summary>
        public IllegalThreadStateException() : base() { }

        /// <summary>
        /// Creates a new illegal thread state exception with the given
        /// detail message.
        /// </summary>
        /// <param name="msg">The message detailing the exception</param>
        public IllegalThreadStateException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public IllegalThreadStateException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected IllegalThreadStateException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion

    }
}
