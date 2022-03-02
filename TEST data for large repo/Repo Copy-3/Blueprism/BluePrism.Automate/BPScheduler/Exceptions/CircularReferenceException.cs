using System;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Exception thrown if the adding of an element would create an
    /// unresolvable circular reference. eg. if attempting to add a 
    /// trigger group 'G1' to a group 'G2' when G2 is a member of the
    /// G1 group, this exception will be thrown.
    /// </summary>
    [Serializable]
    public class CircularReferenceException : ScheduleException
    {
        /// <summary>
        /// Creates a new circular reference exception with no further
        /// detail message.
        /// </summary>
        public CircularReferenceException() : base() { }

        /// <summary>
        /// Creates a new circular reference exception with the given
        /// detail message.
        /// </summary>
        /// <param name="msg">The message indicating why this exception
        /// has been thrown.</param>
        public CircularReferenceException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with the given formatted message.
        /// </summary>
        /// <param name="msg">The format string for the message. For the
        /// formatting placeholders see <see cref="String.Format"/></param>
        /// <param name="args">The arguments for the specified message.
        /// </param>
        public CircularReferenceException(string msg, params object[] args)
            : base(msg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given
        /// serialization objects.
        /// </summary>
        /// <param name="info">The serialized object data</param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected CircularReferenceException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion

    }
}
