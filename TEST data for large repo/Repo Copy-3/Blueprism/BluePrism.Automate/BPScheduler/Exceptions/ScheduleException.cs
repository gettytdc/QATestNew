using System;
using System.Runtime.Serialization;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Base class for BPSchedule-specific exceptions
    /// </summary>
    [Serializable]
    public class ScheduleException : BluePrismException
    {
        /// <summary>
        /// Creates a new schedule exception with no message
        /// </summary>
        public ScheduleException() : base() { }

        /// <summary>
        /// Creates a new schedule exception with the given message.
        /// </summary>
        /// <param name="msg"></param>
        public ScheduleException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new schedule exception with the given message.
        /// </summary>
        /// <param name="msg">The message detailing the cause of this exception,
        /// with formatting markers as necessary
        /// </param>
        /// <param name="args">The arguments for the formatted message of
        /// this exception.</param>
        public ScheduleException(string msg, params object[] args) : base(msg, args) { }

        /// <summary>
        /// Creates a new schedule exception caused by the given exception,
        /// detailed by the given message.
        /// </summary>
        /// <param name="msg">The message detailing the cause of this exception,
        /// with formatting markers as necessary
        /// </param>
        /// <param name="cause">The root cause of this exception</param>
        /// <param name="args">The arguments for the formatted message of
        /// this exception.</param>
        public ScheduleException(string msg, Exception cause, params object[] args)
            : base(msg, cause, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes a schedule exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data
        /// </param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected ScheduleException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion
    }
}
