using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown when an invalid name is passed as a public
    /// holiday group name to any of the relevant public holiday methods.
    /// </summary>
    [Serializable]
    public class InvalidGroupNameException : ScheduleException
    {
        /// <summary>
        /// Creates an invalid group name exception with no message.
        /// </summary>
        public InvalidGroupNameException() : base() { }

        /// <summary>
        /// Creates an invalid group name exception with the given message.
        /// </summary>
        /// <param name="msg">The message detailing the error.</param>
        public InvalidGroupNameException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public InvalidGroupNameException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected InvalidGroupNameException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion


    }
}
