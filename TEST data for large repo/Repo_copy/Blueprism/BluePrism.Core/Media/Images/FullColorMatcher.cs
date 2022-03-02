using System;
using System.Drawing;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// IColorMatcher implementation that allows for closely matching colours with
    /// a specified tolerance. Alpha component must be an exact match.
    /// </summary>
    public class FullColorMatcher : IColorMatcher
    {
        private readonly int _tolerance;

        /// <summary>
        /// Creates a new instance of the <see cref="FullColorMatcher"/> that is
        /// used to determine if two colours are a match, given a specified
        /// tolerance.
        /// </summary>
        /// <param name="tolerance">The variance that is allowed in rgb values
        /// when checking whether two colours match, a value of 0 will give exact
        /// matching. </param>
        public FullColorMatcher(int tolerance)
        {
            _tolerance = tolerance;
        }

        /// <summary>
        /// Indicates whether 2 values should be treated as matching colours
        /// </summary>
        /// <param name="color1">The first colour to compare</param>
        /// <param name="color2">The second colour to compare</param>
        /// <returns>True if the colours should be treated as matching</returns>
        public bool Match(Color color1, Color color2)
        {
            return _tolerance == 0 ? ExactMatch(color1, color2)
                                   : ToleranceMatch(color1, color2);
        }

        protected virtual bool ExactMatch(Color color1, Color color2)
        {
            return color1.Equals(color2);
        }

        protected virtual bool ToleranceMatch(Color color1, Color color2)
        {
            return Math.Abs(color1.R - color2.R) <= _tolerance
                && Math.Abs(color1.G - color2.G) <= _tolerance
                && Math.Abs(color1.B - color2.B) <= _tolerance
                && color1.A == color2.A;
        }
    }
}
