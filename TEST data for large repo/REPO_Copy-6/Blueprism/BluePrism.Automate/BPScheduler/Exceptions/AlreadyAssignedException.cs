using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown when an object is assigned and an attempt is
    /// made to assign it elsewhere without first unassigning it from
    /// its existing assignation.
    /// </summary>
    [Serializable]
    public class AlreadyAssignedException : ScheduleException
    {
        /// <summary>
        /// Creates a new already assigned exception with no further
        /// detail message.
        /// </summary>
        public AlreadyAssignedException() : base() { }

        /// <summary>
        /// Creates a new already assigned exception with the provided
        /// extra detail.
        /// </summary>
        /// <param name="msg">The message indicating why this exception
        /// was thrown.</param>
        public AlreadyAssignedException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public AlreadyAssignedException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected AlreadyAssignedException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion

    }
}
