using BluePrism.CharMatching.Properties;
using BluePrism.Server.Domain.Models;
using System;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Exception raised when a font is found to be empty when it is intended for use
    /// in a charmatching operation.
    /// </summary>
    [Serializable]
    public class EmptyFontException : EmptyException
    {
        /// <summary>
        /// Creates a new exception regarding the font with the given name
        /// </summary>
        /// <param name="fontName">The name of the empty font to report</param>
        public EmptyFontException(string fontName)
            : base(Resources.TheFont0ContainsNoCharacters, fontName) { }
    }
}
