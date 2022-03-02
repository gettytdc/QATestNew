using System;
using System.Drawing;

namespace BluePrism.Core.Media.Images
{
    public class GreyScaleColorMatcher : IColorMatcher
    {
        private int _greyTolerance;

        /// <summary>
        /// Constructor for GreyScaleColor Matcher, excepts an additional color matcher as a parameter.
        /// </summary>
        /// <param name="colorMatcher">An additional color matcher that Greyscale 
        /// will work in combination with. </param>
        public GreyScaleColorMatcher(int greyTolerance)
        {
            _greyTolerance = greyTolerance;
        }

        /// <summary>
        /// Indicates whether 2 values should be treated as matching greyscale values.
        /// </summary>
        /// <param name="color1">The first colour to compare</param>
        /// <param name="color2">The second colour to compare</param>
        /// <returns>True if the values should be treated as matching</returns>
        public bool Match(Color color1, Color color2)
        {
            byte greyValue1 = GetGreyscaleValueFromColor(color1);
            byte greyValue2 = GetGreyscaleValueFromColor(color2);

            return Math.Abs(greyValue1 - greyValue2) <= _greyTolerance;
        }

        /// <summary>
        /// Gets a value representing the brightness of a colour (strictly speaking the &quot;luma&quot; value
        /// as described at http://poynton.ca/notes/colour_and_gamma/ColorFAQ.html#RTFToC9) that is suitable 
        /// for converting a colour to a greyscale value. 
        /// </summary>
        /// <param name="color">The color to convert to a greyscale value. </param>
        /// <returns>A byte representing greyscale from the color vector. </returns>
        protected virtual byte GetGreyscaleValueFromColor(Color color)
        {
            // Weighting of RGB values described here - http://poynton.ca/notes/colour_and_gamma/ColorFAQ.html#RTFToC9
            
            return Convert.ToByte(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
        }
    }
}
