using System;
using BluePrism.Server.Domain.Models;
using System.Runtime.Serialization;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Exception indicating that a font which doesn't exist in this environment has
    /// been requested.
    /// </summary>
    [Serializable]
    public class CharMatchingException : BluePrismException
    {
        /// <summary>
        /// Creates a new exception with no message
        /// </summary>
        public CharMatchingException() : base() { }

        /// <summary>
        /// Creates a new exception with the given message.
        /// </summary>
        /// <param name="formattedMsg">The message giving detail about the exception,
        /// with formatting placeholders</param>
        /// <param name="args">The arguments for the format string</param>
        public CharMatchingException(string formattedMsg, params string[] args)
            : base(formattedMsg, args) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given serialization
        /// objects.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data
        /// </param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected CharMatchingException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion
    }

}
