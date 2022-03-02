using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown when a duplicate name within the context
    /// of the scheduler or its ancillary objects.
    /// </summary>
    /// <remarks>
    /// This would be named 'DuplicateNameException' if one with that
    /// name didn't already exist within System.Data specific to data
    /// table names.
    /// </remarks>
    [Serializable]
    public class NameAlreadyExistsException : ScheduleException
    {
        /// <summary>
        /// Creates a new exception with no specific message.
        /// </summary>
        public NameAlreadyExistsException() : base() { }

        /// <summary>
        /// Creates a new exception with the given message.
        /// </summary>
        /// <param name="msg"></param>
        public NameAlreadyExistsException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public NameAlreadyExistsException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes a name exists exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected NameAlreadyExistsException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion

    }
}
