using System.Collections.Generic;
using System.Text;

namespace BluePrism.Core.Expressions
{
    /// <summary>
    /// A list of lexemes
    /// </summary>
    internal class LexemeList : List<Lexeme>
    {
        /// <summary>
        /// Creates a new empty lexeme list
        /// </summary>
        public LexemeList() : base() { }

        /// <summary>
        /// Creates a new lexeme list containing the given lexemes
        /// </summary>
        /// <param name="lexemes">The lexemes to set into the new lexeme list</param>
        public LexemeList(IEnumerable<Lexeme> lexemes) : base(lexemes) { }

        /// <summary>
        /// Creates a new lexeme list containing the given lexemes
        /// </summary>
        /// <param name="els">The lexemes to set into the new lexeme list</param>
        public LexemeList(params Lexeme[] els) : this((IEnumerable<Lexeme>)els) { }

        /// <summary>
        /// The tokens which make up the lexemes in this list combined into a single
        /// expression
        /// </summary>
        public string Text
        {
            get 
            {
                StringBuilder sb = new StringBuilder();
                foreach (Lexeme lex in this)
                {
                    sb.Append(lex.Token);
                }
                return sb.ToString();
            }
        }
    }
}
