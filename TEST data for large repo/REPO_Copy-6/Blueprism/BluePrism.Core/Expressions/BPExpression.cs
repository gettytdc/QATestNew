using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using BluePrism.BPCoreLib.Collections;

namespace BluePrism.Core.Expressions
{
    /// <summary>
    /// Class which represents a Blue Prism expression, with support for lexing it
    /// and providing a normal form and local form of the lexed expression.
    /// </summary>
    /// <remarks>Note that this class is (and should remain) semantically immutable,
    /// ie. once it is created, it should not be possible to modify it (at least, not
    /// without the use of reflection anyway).</remarks>
    [Serializable, DataContract(Namespace = "bp")]
    public class BPExpression
    {
        #region - Class Scope Declarations -

        /// <summary>
        /// An empty expression which never changes its value
        /// </summary>
        public static readonly BPExpression Empty = new BPExpression("", true);

        /// <summary>
        /// Converts an expression from a culture-specific localised form to the
        /// common normal form
        /// </summary>
        /// <param name="expr">The expression to normalise</param>
        /// <param name="from">The culture which the expression is formatted using.
        /// </param>
        /// <returns>The given expression in normal form, using the common culture
        /// used throughout Blue Prism for normalised data.</returns>
        private static string Normalise(string expr, CultureInfo from)
        {
            return Lexeme.Combine(
                new ExpressionLexer(expr, from).PerformNormalised());
        }

        /// <summary>
        /// Creates a Blue Prism expression object from the given normalised
        /// expression string.
        /// </summary>
        /// <param name="expr">The expression, in normal form</param>
        /// <returns>An expression object which represents the given expression.
        /// </returns>
        public static BPExpression FromNormalised(string expr)
        {
            return new BPExpression(expr, true);
        }

        /// <summary>
        /// Creates a Blue Prism expression object from the given local expression
        /// string.
        /// </summary>
        /// <param name="expr">The expression in localised form, using the formatting
        /// as defined by the <see cref="CultureInfo.CurrentCulture">Current Culture
        /// </see></param>
        /// <returns>An expression object which represents the given expression.
        /// </returns>
        public static BPExpression FromLocalised(string expr)
        {
            return new BPExpression(expr, false);
        }

        /// <summary>
        /// Tests if the two given expressions are equal
        /// </summary>
        /// <param name="one">The first expression to test</param>
        /// <param name="two">The second expression to test</param>
        /// <returns>True if the two expressions are considered equal. Note that, for
        /// the purposes of this operator, a null expression is considered equal to
        /// an empty expression such that <c>BPExpression.FromNormal("") == null</c>
        /// would return true.</returns>
        public static bool operator ==(BPExpression one, BPExpression two)
        {
            if (((object)one) == null) one = BPExpression.Empty;
            if (((object)two) == null) two = BPExpression.Empty;
            return CollectionUtil.AreEqual(one._lexemes, two._lexemes);
        }

        /// <summary>
        /// Tests if the two given expressions are unequal
        /// </summary>
        /// <param name="one">The first expression to test</param>
        /// <param name="two">The second expression to test</param>
        /// <returns>True if the two expressions are considered not equal. Note that,
        /// for the purposes of this operator, a null expression is considered equal
        /// to an empty expression such that
        /// <c>BPExpression.FromNormal("") != null</c> would return true.</returns>
        public static bool operator !=(BPExpression one, BPExpression two)
        {
            return !(one == two);
        }

        #endregion

        #region - Member Variables -

        [DataMember]
        private IList<Lexeme> _lexemes;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new expression from the given string
        /// </summary>
        /// <param name="expr">The expression for which an expression object is
        /// required. A null value will be treated the same as an empty string,
        /// creating an empty expression</param>
        /// <param name="normalised">Flag indicating if <paramref name="expr"/>
        /// represents a normalised expression</param>
        private BPExpression(string expr, bool normalised)
        {
            // Lex the expression using either the 'normalising' culture or the
            // current thread's culture; Normalise the resultant lexemes using the
            // 'normal' culture.
            ExpressionLexer lexer = new ExpressionLexer(expr ?? "",
                normalised ? InternalCulture.Instance : CultureInfo.CurrentCulture);
            _lexemes = new List<Lexeme>(lexer.PerformNormalised());
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets a string representation of this expression in normal form
        /// </summary>
        public string NormalForm
        {
            get { return Lexeme.Combine(_lexemes); }
        }

        /// <summary>
        /// Gets a string representation of this expression in local form, according
        /// to the <see cref="CultureInfo.CurrentCulture">CurrentCulture</see>
        /// </summary>
        public string LocalForm
        {
            get
            {
                // Convert the current collection of lexemes into the local form
                // from the normal form and combine the results
                return Lexeme.Combine(CollectionUtil.Convert<Lexeme>(_lexemes,
                    delegate(Lexeme lex) {
                        return lex.Clone(
                            InternalCulture.Instance, CultureInfo.CurrentCulture);
                    }
                ));
            }
        }

        /// <summary>
        /// Gets whether this expression is empty or not
        /// </summary>
        public bool IsEmpty { get { return (_lexemes.Count == 0); } }

        #endregion

        #region - Methods -

        /// <summary>
        /// Clones this expression, but blanks out the list of lexemes as it does so,
        /// leaving them there ready to be populated by cloned lexemes or by filtered
        /// lexemes obeying some arbitrary rule
        /// </summary>
        /// <returns>A BPExpression cloned from this with a new empty collection in
        /// place of the lexemes</returns>
        private BPExpression BlankClone()
        {
            BPExpression copy = (BPExpression)MemberwiseClone();
            copy._lexemes = new List<Lexeme>();
            return copy;
        }

        /// <summary>
        /// Deep clones this expression and returns the result
        /// </summary>
        /// <returns>A full clone of this expression object</returns>
        public BPExpression Clone()
        {
            BPExpression copy = BlankClone();
            CollectionUtil.CloneInto(_lexemes, copy._lexemes);
            return copy;
        }

        /// <summary>
        /// Renames all references to a data item contained in this expression with
        /// references to a different item, returning the resultant expression.
        /// Scans this expression and creates a cloned expression with a data item
        /// reference renamed.
        /// </summary>
        /// <param name="oldName">The old data item name which is to be replaced
        /// </param>
        /// <param name="newName">The new name for the data item to be replace with.
        /// </param>
        /// <returns>An expression containing the new data item name in place of the
        /// old one</returns>
        public BPExpression ReplaceDataItemName(string oldName, string newName)
        {
            BPExpression copy = BlankClone();
            int offset = 0;

            // Go through each of the lexemes in this expression; if they are data
            // items with the name to be replaced, replace them with the new name.
            // Otherwise, just translate them from this expression to the new
            // expression (ensuring that their start position is updated if it has
            // changed due to the data item rename operation).
            foreach (Lexeme lex in _lexemes)
            {
                Lexeme newLex;
                // we're only interested in data items, specifically those with the
                // old name
                if (lex.Style == ExpressionStyle.DataItem
                    && lex.Token == "[" + oldName + "]")
                {
                    newLex = new Lexeme(
                        lex.Style, "[" + newName + "]", lex.StartPosition + offset);

                    // We need to bump the offset on by the difference between the
                    // two names so that subsequent lexemes are starting from the
                    // newly correct position
                    offset += (newName.Length - oldName.Length);
                }
                else
                {
                    newLex =
                        new Lexeme(lex.Style, lex.Token, lex.StartPosition + offset);
                }
                copy._lexemes.Add(newLex);
            }
            return copy;

        }

        /// <summary>
        /// Returns all data items referenced by the expression (duplicates are
        /// removed).
        /// </summary>
        /// <returns>List of data items</returns>
        public ICollection<string> GetDataItems()
        {
            List<string> items = new List<string>();
            foreach (Lexeme lex in _lexemes)
            {
                if (lex.Style == ExpressionStyle.DataItem)
                {
                    string name = lex.Token.Replace("[", "").Replace("]", "");
                    if (!items.Contains(name)) items.Add(name);
                }
            }
            return items;
        }

        /// <summary>
        /// Checks if this expression is equal to the given object.
        /// </summary>
        /// <param name="obj">The object to test for equality against.</param>
        /// <returns>True if the given object is a non-null expression with the same
        /// value as this expression; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            BPExpression expr = obj as BPExpression;

            // we need to avoid 'expr != null' here, since a blank expression does
            // equal null according to the operator, whereas it doesn't according
            // to this method (just to ensure reciprocity
            return (!ReferenceEquals(expr, null) && this == expr);
        }

        /// <summary>
        /// Gets a hash of this expression as an integer
        /// </summary>
        /// <returns>An integer hash based on the normal form of this expression.
        /// </returns>
        public override int GetHashCode()
        {
            return (NormalForm.GetHashCode() ^ 0xcaca);
        }

        /// <summary>
        /// Gets a string representation of this expression - this is equivalent to
        /// the <see cref="LocalForm"/> of this expression. As such, the standard
        /// string representation of an expression can be used in user-facing
        /// interfaces. (eg. <c>String.Format("Expression: {0}", expr)</c> will
        /// output the expression in the form dictated by user preferences rather
        /// than the normal form used by the back end)
        /// </summary>
        /// <returns>A (local) string representation of this expression.</returns>
        public override string ToString()
        {
            return LocalForm;
        }

        #endregion

    }
}
