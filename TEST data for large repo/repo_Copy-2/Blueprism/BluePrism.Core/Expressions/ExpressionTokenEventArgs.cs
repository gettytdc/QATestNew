using System;

namespace BluePrism.Core.Expressions
{
    /// <summary>
    /// Delegate describing an event handler which handles ExpressionToken events
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The args detailing the event</param>
    public delegate void ExpressionTokenEventHandler(
        object sender, ExpressionTokenEventArgs e);

    /// <summary>
    /// Event args detailing an expression token event
    /// </summary>
    public class ExpressionTokenEventArgs : EventArgs
    {
        // The position within the text being lexed of the token
        private int _posn;

        // The style of the lexed token
        private ExpressionStyle _style;

        // The token that forms this event
        private string _token;

        /// <summary>
        /// Creates a new event args object for a lexeme
        /// </summary>
        /// <param name="lex">The lexeme containing the data that was found.</param>
        internal ExpressionTokenEventArgs(Lexeme lex)
            : this(lex.StartPosition, lex.Style, lex.Token) { }

        /// <summary>
        /// Creates a new event args object for a token
        /// </summary>
        /// <param name="position">The position within a larger body of text that
        /// the token was found.</param>
        /// <param name="token">The token that was found.</param>
        public ExpressionTokenEventArgs(
            int position, ExpressionStyle style, string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            _posn = position;
            _style = style;
            _token = token;
        }

        /// <summary>
        /// The position within a larger body of text that the token was found.
        /// The relative location of this token is dependent on the context in which
        /// it was discovered. Typically, for an ExpressionLexer for instance, it
        /// is the position of the token within the text which is being lexed.
        /// </summary>
        public int Position { get { return _posn; } }

        /// <summary>
        /// The token that forms this event
        /// </summary>
        public string Token { get { return _token; } }

        /// <summary>
        /// The style which describes the type of the found token
        /// </summary>
        public ExpressionStyle Style { get { return _style; } }
    }

}
