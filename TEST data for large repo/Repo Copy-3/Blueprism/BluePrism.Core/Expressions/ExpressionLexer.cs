using System.Collections.Generic;
using System.Text;
using BluePrism.BPCoreLib.Collections;
using System.Globalization;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Core.Expressions
{
    /// <summary>
    /// Delegate used to test if a name represents a function or not
    /// </summary>
    /// <param name="name">The name to test to see if it is a function</param>
    /// <returns>true if the name represents the name of a function; false otherwise.
    /// </returns>
    public delegate bool FunctionTester(string name);

    /// <summary>
    /// A class which will accept an expression and lex it into a series of
    /// <see cref="Lexeme">Lexemes</see>, effectively tokenising the expression and
    /// categorising the resultant tokens.
    /// </summary>
    /// <remarks>A lexing run should never actually error, even if, after lexing,
    /// some of the tokens are still of <see cref="ExpressionStyle.Default"/> style;
    /// it's up to the parser to contextualise and report those errors. This is not
    /// least because the lexer is planned to be used for the text editor in order to
    /// syntax colour expressions, and thus it must always return something.
    /// </remarks>
    public class ExpressionLexer
    {
        #region - Class-scope declarations -

        // Boolean operators for testing current tokens against
        private static readonly IBPSet<string> BoolOperators = GetReadOnly.ISetFrom(
            "AND", "OR", "and", "or"
        );

        /// <summary>
        /// Tests if the given name represents a boolean operation
        /// </summary>
        /// <param name="name">The name to test to see if it represents a boolean
        /// </param>
        /// <returns>True if the given name represents a boolean operation;
        /// false otherwise.</returns>
        public static bool IsBoolOp(string name)
        {
            return BoolOperators.Contains(name);
        }

        #endregion

        #region - Published Events -

        /// <summary>
        /// Raised in a lexing run after a token (lexeme) has been found and
        /// processed
        /// </summary>
        public event ExpressionTokenEventHandler FoundToken;

        #endregion

        #region - Member Variables -

        // The current token being constructed
        private StringBuilder _token = new StringBuilder();

        // The collection of tokens produced by this run
        private ICollection<Lexeme> _tokens;

        // The delegate used to test 
        private FunctionTester m_isFunction;

        // The text that is being lexed over
        private string m_text;

        // The base position within the text at which the current token started
        private int m_basePosn;

        // The current position that the lexer is at within the text
        private int m_posn;

        // The culture used to interpret the expression
        private CultureInfo m_culture;

        // The style of the current token being lexed.
        private ExpressionStyle m_style;

        // The potential style of the current token - this is set if a function or
        // boolean operator is detected and discarded if the next char invalidates it
        private ExpressionStyle m_potential;

        #region - Potentially Redundant Items - created for UI, may be dead -

        // The start position of the lexing run
        private int m_startPosn;

        // The current line number in the text that we are on
        private int m_line;

        // The current column number in the text that we are on
        private int m_col;

        #endregion

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new lexer to handle the given text using the current locale.
        /// </summary>
        /// <param name="txt">The text to perform the lexing operation upon</param>
        public ExpressionLexer(string txt)
            : this(txt, CultureInfo.CurrentCulture, ExpressionStyle.Default) { }

        /// <summary>
        /// Creates a new lexer to handle the given text using a specific locale.
        /// </summary>
        /// <param name="txt">The text to perform the lexing operation upon</param>
        public ExpressionLexer(string txt, CultureInfo cult)
            : this(txt, cult, ExpressionStyle.Default) { }

        /// <summary>
        /// Creates a new lexer to handle the given text with the state continuing
        /// from the previous line state given, using the current locale
        /// </summary>
        /// <param name="txt">The text to perform the lexing operation upon</param>
        /// <param name="initialState">The state inherited from the previous line
        /// which should be used in this lexing run</param>
        internal ExpressionLexer(string txt, ExpressionStyle initialState)
            : this(txt, CultureInfo.CurrentCulture, 0, initialState, 0, 0) { }

        /// <summary>
        /// Creates a new lexer to handle the given text with the state continuing
        /// from the previous line state given, using a specific locale
        /// </summary>
        /// <param name="txt">The text to perform the lexing operation upon</param>
        /// <param name="cult">The culture to use to interpret the expression</param>
        /// <param name="initialState">The state inherited from the previous line
        /// which should be used in this lexing run</param>
        internal ExpressionLexer(
            string txt, CultureInfo cult, ExpressionStyle initialState)
            : this(txt, cult, 0, initialState, 0, 0) { }

        /// <summary>
        /// Creates a new lexer with the given properties
        /// </summary>
        /// <param name="txt">The text to els over</param>
        /// <param name="cult">The culture to use to interpret the expression</param>
        /// <param name="startPosn">The start position within the larger document
        /// that the text starts at</param>
        /// <param name="inheritedStyle">The style inherited from a previous line to
        /// the text being lexed</param>
        /// <param name="lineNo">The line number within the larger document that this
        /// text starts at</param>
        /// <param name="colNo">The column number within the larger document that
        /// this text starts at</param>
        private ExpressionLexer(string txt, CultureInfo cult, int startPosn,
            ExpressionStyle inheritedStyle, int lineNo, int colNo)
        {
            m_text = txt;
            m_culture = cult;
            m_startPosn = startPosn;
            m_style = InitStyle(inheritedStyle);
            m_line = lineNo;
            m_col = colNo;
            m_posn = m_basePosn = 0;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The delegate used to test if an as yet uncategorised token represents
        /// a function name or not.
        /// </summary>
        /// <remarks>This will never be null - if it is explicitly set to null from
        /// outside of this class a default delegate which has no functions defined
        /// </remarks>
        public FunctionTester IsFunctionDelegate
        {
            get { return (m_isFunction ?? delegate(string nm) { return false; }); }
            set { m_isFunction = value; }
        }

        /// <summary>
        /// The text being lexed (a subset of the text within the textbox whose
        /// content is being lexed).
        /// </summary>
        protected string Text
        {
            get { return m_text; }
            private set { m_text = value; }
        }

        /// <summary>
        /// The string builder in which the current lexeme is being built
        /// </summary>
        protected StringBuilder Builder
        {
            get { return _token; }
        }

        /// <summary>
        /// The token currently being processed in the form of a string.
        /// </summary>
        protected string CurrentToken
        {
            get { return Builder.ToString(); }
        }

        /// <summary>
        /// The collection of tokens currently generated by this lexing run
        /// </summary>
        private ICollection<Lexeme> Tokens
        {
            get { return _tokens; }
            set { _tokens = value; }
        }

        /// <summary>
        /// Gets the last lexeme added to the collection of <see cref="Tokens"/> in
        /// this lexer, or null if there are no tokens currently set in this lexer
        /// </summary>
        private Lexeme Last
        {
            get
            {
                if (_tokens == null) return null;
                return CollectionUtil.Last(_tokens);
            }
        }

        /// <summary>
        /// Gets the style of the last lexeme, or <see cref="ExpressionStyle.None"/>
        /// if there are no lexemes currently set in this lexer.
        /// </summary>
        private ExpressionStyle LastStyle
        {
            get
            {
                Lexeme lex = Last;
                return (lex == null ? ExpressionStyle.None : lex.Style);
            }
        }

        /// <summary>
        /// The current character being considered within the lexing loop
        /// or a null char if there are no more characters to consider
        /// </summary>
        protected char CurrentChar
        {
            get
            {
                if (CurrentPosition < Text.Length)
                    return Text[CurrentPosition];
                return default(char);
            }
        }

        /// <summary>
        /// The next character which will be considered in the lexing loop
        /// or a null char if there are no more characters to consider
        /// </summary>
        protected char NextChar
        {
            get
            {
                if (CurrentPosition + 1 < Text.Length)
                    return Text[CurrentPosition + 1];
                return default(char);
            }
        }

        /// <summary>
        /// Flag indicating if the end of the text being styled has been reached
        /// </summary>
        protected bool IsEndOfText
        {
            get { return (CurrentPosition >= Text.Length); }
        }

        /// <summary>
        /// The current base position - ie. the position within the text of the
        /// lexeme which is currently being processed
        /// </summary>
        protected int CurrentBasePosition
        {
            get { return m_basePosn; }
            private set { m_basePosn = value; }
        }

        /// <summary>
        /// The current position within the text being considered
        /// </summary>
        protected int CurrentPosition
        {
            get { return m_posn; }
            private set { m_posn = value; }
        }

        /// <summary>
        /// The line number within the greater body of text held by the text box
        /// of the character being considered
        /// </summary>
        protected int CurrentLine
        {
            get { return m_line; }
            private set { m_line = value; }
        }

        /// <summary>
        /// The column number of the character being considered.
        /// </summary>
        protected int CurrentColumn
        {
            get { return m_col; }
            private set { m_col = value; }
        }

        /// <summary>
        /// The current style set within the state machine of this lexer
        /// </summary>
        internal ExpressionStyle CurrentStyle
        {
            get { return m_style; }
            set { m_style = value; }
        }

        private ExpressionStyle PotentialStyle
        {
            get { return m_potential; }
            set { m_potential = value; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Commits the current token using the current style and resets the
        /// token accumulator builder.
        /// Raises the <see cref="FoundToken"/> event with the token and current
        /// position within the text.
        /// </summary>
        private void Commit()
        {
            if (Builder.Length > 0)
            {
                string tok = Builder.ToString();
                ExpressionStyle style = CurrentStyle;
                // See if there is an outstanding 'potential' style in place - if
                // there is, it means that, at last check, that style should be
                // committed, not the current style
                if (PotentialStyle != ExpressionStyle.Default)
                {
                    style = PotentialStyle;
                    // Reset the potential style - we don't want it to carry on into
                    // the next token by mistake
                    PotentialStyle = ExpressionStyle.Default;
                }
                Lexeme lex = new Lexeme(style, tok, CurrentBasePosition);
                Tokens.Add(lex);
                OnFoundToken(new ExpressionTokenEventArgs(lex));
                Builder.Length = 0;
            }
            CurrentBasePosition = CurrentPosition;
        }

        /// <summary>
        /// Raises the <see cref="FoundToken"/> event with the given args.
        /// </summary>
        /// <param name="e">The event args to pass on to listeners to the event.
        /// </param>
        protected virtual void OnFoundToken(ExpressionTokenEventArgs e)
        {
            ExpressionTokenEventHandler handler = this.FoundToken;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Consumes the current character with the current style, then moves onto
        /// the next character
        /// </summary>
        /// <returns>true if there are any more characters to process; false if the
        /// end of the text has been reached</returns>
        private bool Consume()
        {
            return Consume(CurrentStyle);
        }

        /// <summary>
        /// Consumes the current character with the given style. If the current style
        /// differs from that given, the current token is committed and a new token
        /// accumulator is begun.
        /// Finally, the method moves onto the next character.
        /// </summary>
        /// <param name="style">The style to consume the current character in and set
        /// as the current style in this lexer</param>
        /// <returns>true if there are any more characters to process; false if the
        /// end of the text has been reached</returns>
        private bool Consume(ExpressionStyle style)
        {
            if (CurrentStyle != style)
            {
                Commit();
                CurrentStyle = style;
            }
            Builder.Append(CurrentChar);
            return MoveNext();
        }

        /// <summary>
        /// Consumes characters using the current style up until and including a
        /// terminating character.
        /// </summary>
        /// <param name="termChar">The terminating character to consume up until.
        /// </param>
        /// <returns>true if there are any more characters to process; false if the
        /// end of the text has been reached</returns>
        /// <remarks>When this method returns true (ie. there are more characters),
        /// the lexer's current character will be the one immediately beyond the
        /// terminating character.</remarks>
        private bool ConsumeUntil(char termChar)
        {
            return ConsumeUntil(CurrentStyle, termChar);
        }

        /// <summary>
        /// Consumes characters using up until and including a terminating character.
        /// </summary>
        /// <param name="state">The state to effect when consuming the characters
        /// </param>
        /// <param name="termChar">The terminating character to consume up until.
        /// </param>
        /// <returns>true if there are any more characters to process; false if the
        /// end of the text has been reached</returns>
        /// <remarks>When this method returns true (ie. there are more characters),
        /// the lexer's current character will be the one immediately beyond the
        /// terminating character.</remarks>
        private bool ConsumeUntil(ExpressionStyle state, char termChar)
        {
            while (Consume(state))
            {
                if (CurrentChar == termChar)
                {
                    // Include the term char in the token before committing it
                    bool more = Consume(state);
                    Commit();
                    CurrentStyle = ExpressionStyle.Default;
                    return more;
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to move to the next character in the string being processed.
        /// </summary>
        /// <returns>true if there is a 'next' character that was moved to; false if
        /// there were no more characters to process</returns>
        private bool MoveNext()
        {
            if (CurrentPosition >= Text.Length)
                return false;

            switch (CurrentChar)
            {
                // I'm not sure if we can work this really...
                // Strings certainly need the line terms appended to the token
                // since they can be multiline... needs a bit of a rethink.
                //case '\r':
                //    if (NextChar == '\n')
                //        CurrentPosition++;
                //    goto case '\n';
                case '\n':
                    CurrentLine++;
                    CurrentColumn = 0;
                    break;
                default:
                    CurrentColumn++;
                    break;
            }
            return (++CurrentPosition < Text.Length);
        }

        /// <summary>
        /// Performs a lexical analysis of the text, normalising it to a specific
        /// culture along the way.
        /// </summary>
        /// <param name="norm">The culture to which the expression should be
        /// normalised.</param>
        /// <returns></returns>
        internal ICollection<Lexeme> PerformNormalised()
        {
            // If we're normalising to the current culture in this lexer, just
            // perform a lexing run and return the results
            if (m_culture == InternalCulture.Instance)
                return Perform();

            // Otherwise, clone the lexemes, converting from the current culture
            // to the required 'normal' culture
            return CollectionUtil.Convert<Lexeme>(Perform(),
                delegate(Lexeme lex) {
                    return lex.Clone(m_culture, InternalCulture.Instance); 
                });
        }

        /// <summary>
        /// Performs the styling of the text set in this lexer.
        /// </summary>
        /// <returns>The collection of els found by lexing the text within
        /// this object.</returns>
        internal ICollection<Lexeme> Perform()
        {
            string decSep =
                m_culture.NumberFormat.NumberDecimalSeparator;
            string listSep =
                m_culture.TextInfo.ListSeparator;

            // Create a new list of els to work with
            Tokens = new List<Lexeme>();

            // Reset the accumulator
            Builder.Length = 0;

            while (!IsEndOfText)
            {
                switch (m_style)
                {
                    case ExpressionStyle.String: ConsumeUntil('"'); break;

                    case ExpressionStyle.DataItem: ConsumeUntil(']'); break;

                    case ExpressionStyle.Param: ConsumeUntil('}'); break;

                    case ExpressionStyle.OpenBracket:
                    case ExpressionStyle.CloseBracket:
                    case ExpressionStyle.ParamSeparator:
                        // Each bracket and param separator is in its own token,
                        // commit it and move to the standard handling.
                        Commit();
                        goto case ExpressionStyle.Default;

                    case ExpressionStyle.Whitespace:
                        if (char.IsWhiteSpace(CurrentChar))
                        {
                            Consume();
                        }
                        else
                        {
                            Commit();
                            goto case ExpressionStyle.Default;
                        }
                        break;

                    case ExpressionStyle.Operator:
                        // If we're dealing with the tail end of a "<>", ">=" or "<="
                        // Consume the current char (without committing)
                        if ("=>".IndexOf(CurrentChar) != -1 && Builder.Length == 1)
                        {
                            string tok = CurrentToken + CurrentChar;
                            if (tok == ">=" || tok == "<=" || tok == "<>")
                            {
                                Consume();
                                break;
                            }
                        }
                        Commit();
                        // If we're *not* dealing with a combined operator, commit
                        // the current operator and move onto the standard handling
                        goto case ExpressionStyle.Default;

                    case ExpressionStyle.Literal:
                        // We need to check what we have in the current token before
                        // we can sign this off as a literal number.
                        // First see if we've stopped being a number
                        if ((decSep + "0123456789").IndexOf(CurrentChar) == -1)
                        {
                            // So we're moving from number to 'something else'.
                            // Now check that what we have in the token at the
                            // moment is actually a valid number
                            if (CurrentToken == decSep)
                            {
                                // The only token we have is a decimal separator,
                                // which is not valid in and of itself
                                CurrentStyle = ExpressionStyle.Default;
                            }
                            // Anything else is valid (except overflow which is
                            // possibly a little beyond our ken), eg. "5." is 5;
                            // ".5" is 0.5 and is parsed as such by Decimal.Parse()
                        }
                        goto case ExpressionStyle.Default;

                    case ExpressionStyle.Default:
                        ExpressionStyle lastStyle = LastStyle;

                        switch (CurrentChar)
                        {
                            case '"':
                                ConsumeUntil(ExpressionStyle.String, '"');
                                break;

                            case '[':
                                ConsumeUntil(ExpressionStyle.DataItem, ']');
                                break;

                            case '{':
                                ConsumeUntil(ExpressionStyle.Param, '}');
                                break;

                            case '(':
                                Consume(ExpressionStyle.OpenBracket);
                                break;

                            case ')':
                                Consume(ExpressionStyle.CloseBracket);
                                break;

                            case '&':
                            case '+':
                            case '-':
                            case '/':
                            case '*':
                            case '<':
                            case '>':
                            case '=':
                            case '^':
                                Consume(ExpressionStyle.Operator);
                                break;

                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                // Note: decimal separator is not constant, and is
                                // therefore handled in the default case handler

                                // We want to allow numbers as part of other tokens
                                // (eg. function names), so we allow them within a
                                // 'default' token - the only time we change such a
                                // token is if it exists of only a decimal separator
                                // which we combine with the current digit and turn
                                // into a literal number token.
                                if (CurrentStyle == ExpressionStyle.Default &&
                                    CurrentToken == decSep)
                                {
                                    CurrentStyle = ExpressionStyle.Literal;
                                }

                                if (Builder.Length > 0
                                    && CurrentStyle == ExpressionStyle.Default)
                                {
                                    Consume();
                                }
                                else
                                {
                                    Consume(ExpressionStyle.Literal);
                                }
                                break;

                            default:
                                // Get the current token, both without and with the
                                // current character being handled.
                                string curr = this.CurrentToken;
                                string full = curr + CurrentChar;

                                // First deal with the decimal separator as if it was
                                // matched by the digits as above
                                if (decSep.IndexOf(CurrentChar) != -1)
                                {
                                    // If we already have a decimal separator in our
                                    // token or if this is the first char in a token,
                                    // this char is 'unknown'
                                    if (curr.Contains(decSep) || Builder.Length == 0)
                                    {
                                        Consume(ExpressionStyle.Default);
                                    }
                                    else
                                    {
                                        Consume(ExpressionStyle.Literal);
                                    }
                                }
                                // If not a decimal separator, check for being a
                                // list (ie. parameter) separator
                                else if (listSep.IndexOf(CurrentChar) != -1)
                                {
                                    Consume(ExpressionStyle.ParamSeparator);
                                }
                                // We treat whitespace separately (although it's
                                // typically styled just the same as Default), mainly
                                // so that we can easily look up the current word to
                                // see if it's a function/boolean operator
                                else if (char.IsWhiteSpace(CurrentChar))
                                {
                                    Consume(ExpressionStyle.Whitespace);
                                }
                                // We only test for functions & boolean ops if we are
                                // currently in default state
                                else if (CurrentStyle == ExpressionStyle.Default)
                                {
                                    ExpressionStyle newState = CurrentStyle;
                                    if (IsFunctionDelegate(full))
                                    {
                                        PotentialStyle = ExpressionStyle.Function;
                                    }
                                    else if (IsBoolOp(full))
                                    {
                                        PotentialStyle = ExpressionStyle.BooleanOperator;
                                    }
                                    else
                                    {
                                        PotentialStyle = ExpressionStyle.Default;
                                    }
                                    Consume();
                                }
                                // Anything else is now default (if previously it was
                                // functions or bool ops, it has gone beyond and is
                                // thus no longer a function or bool op).
                                else
                                {
                                    PotentialStyle = ExpressionStyle.Default;
                                    Consume(ExpressionStyle.Default);
                                }
                                break;

                        }
                        break;

                    default:
                        throw new InvalidStateException(
                            "Unhandled style: {0}", m_style);
                }
            }

            Commit();

            return GetReadOnly.ICollection(Tokens);
        }

        /// <summary>
        /// Initialises the style from the given style derived from the end of
        /// the previous line.
        /// For our purposes, only strings and parameters can continue onto the
        /// next line (though the latter is only an artificial construct anyway).
        /// </summary>
        /// <param name="prevLineStyle">The style which was in place at the end
        /// of the previous line to the one being lexed.</param>
        /// <returns>The style that this lexing run should be initialised to.
        /// </returns>
        private ExpressionStyle InitStyle(ExpressionStyle prevLineStyle)
        {
            switch (prevLineStyle)
            {
                case ExpressionStyle.String: return ExpressionStyle.String;
                case ExpressionStyle.Param: return ExpressionStyle.Param;
                default: return ExpressionStyle.Default;
            }
        }

        #endregion

    }
}
