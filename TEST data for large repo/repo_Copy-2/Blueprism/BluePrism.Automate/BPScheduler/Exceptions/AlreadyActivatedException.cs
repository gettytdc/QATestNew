using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown when a schedule instance has already been activated
    /// </summary>
    [Serializable]
    public class AlreadyActivatedException : ScheduleException
    {
        /// <summary>
        /// Creates a new already activated exception with no further
        /// detail message.
        /// </summary>
        public AlreadyActivatedException() : base() { }

        /// <summary>
        /// Creates a new already activated exception with the provided
        /// extra detail.
        /// </summary>
        /// <param name="msg">The message indicating why this exception
        /// was thrown.</param>
        public AlreadyActivatedException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public AlreadyActivatedException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected AlreadyActivatedException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion

    }
}
