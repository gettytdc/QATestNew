using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using BluePrism.BPCoreLib;

namespace AutomateControls.Filters
{
    /// <summary>
    /// Filter definition which matches an integer.
    /// </summary>
    public partial class IntegerFilterDefinition: BaseFilterDefinition
    {
        #region - IntegerComparison class -

        /// <summary>
        /// Class to represent a single integer comparison, eg. "> 1", or "<= 5"
        /// </summary>
        private class IntegerComparison
        {
            /// <summary>
            /// The regex to use to identify a single integer comparison
            /// </summary>
            public readonly static Regex FilterTermRegex = new Regex(@"\s*?((?:<|>)?=?)?\s*(\d+)\s*?", RegexOptions.None, RegexTimeout.DefaultRegexTimeout);

            /// <summary>
            /// The type of a comparison
            /// </summary>
            public enum ComparisonType
            {
                None,
                LessThan,
                LessThanOrEqualTo,
                EqualTo,
                GreaterThanOrEqualTo,
                GreaterThan
            }

            // The integer value that is being compared against
            private int _value;

            // The comparison type
            private ComparisonType _comp;

            /// <summary>
            /// Creates a new integer comparison using the given comparison and
            /// integer value.
            /// </summary>
            /// <param name="comp">The type of comparison to store.</param>
            /// <param name="value">The value to compare against</param>
            public IntegerComparison(ComparisonType comp, int value)
            {
                _comp = comp;
                _value = value;
            }

            /// <summary>
            /// Creates a new integer comparison, using the given type and value.
            /// </summary>
            /// <param name="comp">The type of comparison.</param>
            /// <param name="value">The value to compare against.</param>
            public IntegerComparison(string comp, int value)
                : this(ParseComparator(comp), value) { }

            /// <summary>
            /// Parses the given comparison type.
            /// </summary>
            /// <param name="comp">The string representing the comparison type</param>
            /// <returns>The comparison type represented by the string. This will
            /// return <see cref="ComparisonType.EqualTo"/> if the string type was
            /// not recognised.</returns>
            private static ComparisonType ParseComparator(string comp)
            {
                switch (comp)
                {
                    case ">": return ComparisonType.GreaterThan;
                    case ">=": return ComparisonType.GreaterThanOrEqualTo;
                    case "<": return ComparisonType.LessThan;
                    case "<=": return ComparisonType.LessThanOrEqualTo;
                    default: return ComparisonType.EqualTo;
                }
            }

            /// <summary>
            /// Gets a string representation of this comparison.
            /// </summary>
            /// <returns>This comparator as a string.</returns>
            public override string ToString()
            {
                return ToBuilder(new StringBuilder(10)).ToString();
            }

            /// <summary>
            /// Appends a string representation of this comparison into the
            /// given string builder.
            /// </summary>
            /// <param name="sb">The string builder into which this comparison
            /// should be appended.</param>
            /// <returns>The given string builder with this comparison appended into
            /// it. If this comparison represents a
            /// <see cref="ComparisonType.None">None</see> type, then this appends
            /// nothing into the builder.</returns>
            public StringBuilder ToBuilder(StringBuilder sb)
            {
                switch (_comp)
                {
                    // '<' or '<='
                    case ComparisonType.LessThan: sb.Append("< "); break;
                    case ComparisonType.LessThanOrEqualTo: sb.Append("<= "); break;

                    // '>' or '>='
                    case ComparisonType.GreaterThan: sb.Append("> "); break;
                    case ComparisonType.GreaterThanOrEqualTo: sb.Append(">= "); break;

                    // For '=' we just show the value itself.
                    case ComparisonType.EqualTo: break;

                    // For 'None', there is no string, just return the null term.
                    default: return sb;

                }
                return sb.Append(_value);
            }
        }

        #endregion

        #region - IntegerFilterTerm class -

        /// <summary>
        /// Class to represent a filter term for integers, which may be made up of
        /// zero, one or two <see cref="IntegerComparison">integer comparisons</see>.
        /// </summary>
        private class IntegerFilterTerm
        {
            /// <summary>
            /// The collection of comparisons which make up this term.
            /// </summary>
            private ICollection<IntegerComparison> _comparators;

            /// <summary>
            /// Creates a new integer filter term representing the given comparisons.
            /// </summary>
            /// <param name="comparators"></param>
            public IntegerFilterTerm(ICollection<IntegerComparison> comparators)
            {
                _comparators = new List<IntegerComparison>(comparators);
            }

            /// <summary>
            /// Creates a new integer filter term object, parsed from the given
            /// string representation.
            /// </summary>
            /// <param name="term">The string representation of the filter term
            /// which is required.</param>
            public IntegerFilterTerm(string term) 
                : this(ParseComparators(term)) { }

            /// <summary>
            /// Parses the given string term into an integer filter term.
            /// </summary>
            /// <param name="term">The filter term as as string.</param>
            /// <returns>An Integer Filter Term made up from the given string.
            /// </returns>
            public static IntegerFilterTerm Parse(string term)
            {
                return new IntegerFilterTerm(ParseComparators(term));
            }

            /// <summary>
            /// Parses the given term into a collection of comparisons.
            /// </summary>
            /// <param name="term">The term to parse, with 0, 1 or 2 comma-separated
            /// comparisons. eg. "&gt;1", "&lt;=5", "&gt;10, &lt;=20", ""</param>
            /// <returns>The collection of comparisons which have been parsed from 
            /// the given term. This will return an empty collection if no terms
            /// were found in the string.</returns>
            private static ICollection<IntegerComparison> ParseComparators(string term)
            {
                ICollection<IntegerComparison> comps = new List<IntegerComparison>();
                // If there's nothing there, return an empty collection.
                if (string.IsNullOrEmpty(term) 
                    || term.Equals(FilterItem.Empty.FilterTerm))
                {
                    return comps;
                }

                foreach (Match m in IntegerComparison.FilterTermRegex.Matches(term))
                {
                    if (m.Captures.Count > 0)
                    {
                        string op = m.Groups[1].Value;
                        int val = int.Parse(m.Groups[2].Value);
                        comps.Add(new IntegerComparison(op, val));
                    }
                }
                return comps;
            }

            /// <summary>
            /// Gets a string representation of this filter term.
            /// </summary>
            /// <returns>This filter term as a string.</returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (IntegerComparison comp in _comparators)
                {
                    // Prepend a comma if this isn't the first entry into
                    // the string builder.
                    if (sb.Length > 0)
                    {
                        sb.Append(',');
                    }

                    comp.ToBuilder(sb);
                }
                // If nothing came back - either because there are no comparators,
                // or the only comparator has an empty comparison in it, just
                // return the null term.
                if (sb.Length == 0)
                {
                    return FilterItem.Empty.FilterTerm;
                }

                return sb.ToString();
            }
        }

        #endregion

        /// <summary>
        /// The collection of filter items exposed by this definition.
        /// </summary>
        private ICollection<FilterItem> _items;

        /// <summary>
        /// Creates a new filter definition for integers, identified by the given
        /// name.
        /// </summary>
        /// <param name="name">The name to associate with this definition</param>
        public IntegerFilterDefinition(string name)
            : base(name, true)
        {
            _items = new ReadOnlyCollection<FilterItem>(new FilterItem[]{
                new FilterItem(),
                new FilterItem("> 1", new IntegerFilterTerm(">1")),
                new FilterItem("<= 5", new IntegerFilterTerm("<=5"))
            });
        }

        /// <summary>
        /// Parses the given text into a filter item.
        /// </summary>
        /// <param name="txt">The text to parse</param>
        /// <returns>A filter item, recognisable to this filter definition, which
        /// represents the given text.</returns>
        public override FilterItem Parse(string txt)
        {
            if (string.IsNullOrEmpty(txt) || txt.Equals(FilterItem.Empty.FilterTerm))
            {
                return FilterItem.Empty;
            }
            return new FilterItem(txt, new IntegerFilterTerm(txt));
        }

        /// <summary>
        /// Gets the collection of filter items to show for this definition
        /// </summary>
        public override ICollection<FilterItem> Items
        {
            get { return _items; }
        }

    }
}
