using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown when a schedule has finished and an operation has
    /// been attempted on it which requires a running schedule
    /// </summary>
    [Serializable]
    public class ScheduleFinishedException : ScheduleException
    {
        /// <summary>
        /// Creates a new exception with no further detail message.
        /// </summary>
        public ScheduleFinishedException()
            : this("This schedule has already finished") { }

        /// <summary>
        /// Creates a new exception with the provided extra detail.
        /// </summary>
        /// <param name="msg">The message indicating why this exception
        /// was thrown.</param>
        public ScheduleFinishedException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public ScheduleFinishedException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected ScheduleFinishedException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion
    }
}
