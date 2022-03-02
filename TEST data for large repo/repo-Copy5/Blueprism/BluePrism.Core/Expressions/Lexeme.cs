using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;
using System.Linq.Expressions;

namespace BluePrism.Core.Expressions
{
    /// <summary>
    /// A single lexeme from the text being lexed.
    /// This effectively represents a single 'token' as recognised by the
    /// Expression lexer.
    /// </summary>
    [Serializable, DataContract(Namespace = "bp")]
    internal class Lexeme : ICloneable
    {
        /// <summary>
        /// Combines the tokens in a collection of lexemes into a single string
        /// </summary>
        /// <param name="lexemes">The lexemes to combine</param>
        /// <returns>A string containing all of the tokens in the specified
        /// collection of lexemes; an empty string if no lexemes were provided.
        /// </returns>
        public static string Combine(IEnumerable<Lexeme> lexemes)
        {
            if (lexemes == null)
                return "";

            StringBuilder sb = new StringBuilder();
            foreach (Lexeme lex in lexemes)
                sb.Append(lex.Token);
            return sb.ToString();
        }

        [DataMember]
        private readonly ExpressionStyle _style;

        [DataMember]
        private readonly string _token;

        [DataMember]
        private readonly int _startPos;

        /// <summary>
        /// Creates a new lexeme
        /// </summary>
        /// <param name="style">The style being applied to this lexeme</param>
        /// <param name="token">The string which makes up the lexeme</param>
        /// <param name="startPos">The start position within the text being
        /// lexed of this lexeme.</param>
        public Lexeme(ExpressionStyle style, string token, int startPos)
        {
            _style = style;
            _token = token;
            _startPos = startPos;
        }

        /// <summary>
        /// The style applied to this lexeme
        /// </summary>
        public ExpressionStyle Style { get { return _style; } }

        /// <summary>
        /// The string token which makes up this lexeme
        /// </summary>
        public string Token { get { return _token; } }

        /// <summary>
        /// The start position within the text being lexed of this lexeme. Note
        /// that this may be different to the position within the text of, say,
        /// a textbox if only a portion of that text is currently being lexed.
        /// </summary>
        public int StartPosition { get { return _startPos; } }

        /// <summary>
        /// Gets a string representation of this lexeme, wrapping the style,
        /// start position and text itself into the returned string.
        /// </summary>
        /// <returns>A string representation of this lexeme</returns>
        public override string ToString()
        {
            return string.Format("[{0}]-{1}: {2}", _startPos, _style, _token);
        }

        /// <summary>
        /// Checks if this lexeme is equal to the given object.
        /// </summary>
        /// <param name="obj">The object to test for equality against.</param>
        /// <returns>True if the given object is a non-null lexeme which
        /// represents the same text at the same location with the same style
        /// applied to it.</returns>
        public override bool Equals(object obj)
        {
            Lexeme tok = obj as Lexeme;
            return (
                tok != null &&
                _startPos == tok._startPos &&
                _style == tok._style &&
                _token == tok._token
            );
        }

        /// <summary>
        /// Gets an integer hash of this object's data.
        /// </summary>
        /// <returns>An integer which can be used as a hash of this lexeme.
        /// </returns>
        public override int GetHashCode()
        {
            return ((int)_style ^ _token.GetHashCode() ^ _startPos);
        }

        /// <summary>
        /// Clones this lexeme, returning an exact copy of it
        /// </summary>
        /// <returns>A copy of this lexeme</returns>
        public Lexeme Clone()
        {
            return (Lexeme)MemberwiseClone();
        }

        /// <summary>
        /// Clones this lexeme, normalising it to the specified culture along the way
        /// </summary>
        /// <param name="lexemeCulture">The culture which describes the format of
        /// this lexeme.</param>
        /// <param name="normalCulture">The culture which describes the format of the
        /// desired lexeme clone</param>
        /// <returns>A clone of this lexeme, normalised to the provided culture.
        /// </returns>
        internal Lexeme Clone(CultureInfo lexemeCulture, CultureInfo normalCulture)
        {
            var token = _token;
            switch (_style)
            {
                case ExpressionStyle.Literal:
                    if (decimal.TryParse(_token, NumberStyles.AllowDecimalPoint, lexemeCulture, out var digit))
                    {
                        token = digit.ToString(normalCulture);
                    }
                    break;
                case ExpressionStyle.ParamSeparator:
                    token = normalCulture.TextInfo.ListSeparator;
                    break;
            }

            return new Lexeme(_style, token, _startPos);
        }

        /// <summary>
        /// ICloneable implementation of the Clone() method
        /// </summary>
        /// <returns>Clones this lexeme, according to the ICloneable contract
        /// </returns>
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
