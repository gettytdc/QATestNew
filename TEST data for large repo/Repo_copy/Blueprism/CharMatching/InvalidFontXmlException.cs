using System;
using System.Runtime.Serialization;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Exception indicating that the XML given to load a font was invalid.
    /// </summary>
    [Serializable]
    public class InvalidFontXmlException : CharMatchingException
    {
        /// <summary>
        /// Creates a new invalid font xml exception with no message
        /// </summary>
        public InvalidFontXmlException() : base() { }

        /// <summary>
        /// Creates a new exception with the given message.
        /// </summary>
        /// <param name="formattedMsg">The message giving detail about the exception,
        /// with formatting placeholders</param>
        /// <param name="args">The arguments for the format string</param>
        public InvalidFontXmlException(string formattedMsg, params string[] args)
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
        protected InvalidFontXmlException(
            SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }

        #endregion
    }
}
