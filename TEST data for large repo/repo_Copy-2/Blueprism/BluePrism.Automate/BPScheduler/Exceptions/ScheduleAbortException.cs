using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception indicating a schedule being aborted
    /// </summary>
    [Serializable]
    public class ScheduleAbortException : ScheduleException
    {
        /// <summary>
        /// Creates a new exception with no message
        /// </summary>
        public ScheduleAbortException() : base() { }

        /// <summary>
        /// Creates a new exception with the given message.
        /// </summary>
        /// <param name="msg"></param>
        public ScheduleAbortException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given message.
        /// </summary>
        /// <param name="msg">The message detailing the invalid data which
        /// caused this exception, with formatting markers as necessary
        /// </param>
        /// <param name="args">The arguments for the formatted message of
        /// this exception.</param>
        public ScheduleAbortException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes a schedule exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data
        /// </param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected ScheduleAbortException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion

    }
}
