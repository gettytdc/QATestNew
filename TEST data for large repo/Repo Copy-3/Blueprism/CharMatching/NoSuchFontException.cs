using BluePrism.CharMatching.Properties;
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
    public class NoSuchFontException : NoSuchElementException
    {
        /// <summary>
        /// Creates a new exception with no message
        /// </summary>
        public NoSuchFontException() : base() { }

        /// <summary>
        /// Creates a new exception with the given message.
        /// </summary>
        /// <param name="fontName">The name of the font that was not found.</param>
        public NoSuchFontException(string fontName)
            : base(Resources.NoFontWithTheName0WasFound, fontName) { }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given serialization
        /// objects.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data
        /// </param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected NoSuchFontException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }

        #endregion
    }

}
