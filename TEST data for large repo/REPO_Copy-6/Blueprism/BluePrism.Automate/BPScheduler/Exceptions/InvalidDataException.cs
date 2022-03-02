using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown if data which is to be interpreted is invalid and
    /// thus cannot be interpreted correctly.
    /// </summary>
    [Serializable]
    public class InvalidDataException : ScheduleException
    {
        /// <summary>
        /// Creates a new exception with the given message.
        /// </summary>
        /// <param name="msg">The message explaining the exception.</param>
        public InvalidDataException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new invalid data exception with the given explanation
        /// </summary>
        /// <param name="msg">The message detailing the invalid data which
        /// caused this exception, with formatting markers as necessary
        /// </param>
        /// <param name="args">The arguments for the formatted message of
        /// this exception.</param>
        public InvalidDataException(string msg, params object[] args) : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes a name exists exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected InvalidDataException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion
    }
}
