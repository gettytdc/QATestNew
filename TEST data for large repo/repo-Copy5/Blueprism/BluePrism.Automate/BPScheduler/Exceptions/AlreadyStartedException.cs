using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Exceptions
{
    /// <summary>
    /// Exception thrown when something which can only be started once and
    /// has been started is asked to start again.
    /// eg. if Start() is called on an already started schedule log
    /// </summary>
    [Serializable]
    public class AlreadyStartedException : ScheduleException
    {
        /// <summary>
        /// Creates a new exception with no further detail message.
        /// </summary>
        public AlreadyStartedException() : base() { }

        /// <summary>
        /// Creates a new exception with the provided extra detail.
        /// </summary>
        /// <param name="msg">The message indicating why this exception
        /// was thrown.</param>
        public AlreadyStartedException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public AlreadyStartedException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected AlreadyStartedException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion
    }
}
