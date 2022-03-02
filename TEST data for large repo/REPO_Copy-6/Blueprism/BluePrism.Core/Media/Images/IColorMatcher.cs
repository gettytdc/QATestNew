using System.Drawing;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Determines whether 2 colours should be treated as matching
    /// </summary>
    public interface IColorMatcher
    {
        /// <summary>
        /// Indicates whether 2 values should be treated as matching colours
        /// </summary>
        /// <param name="color1">The first colour to compare</param>
        /// <param name="color2">The second colour to compare</param>
        /// <returns>True if the colours should be treated as matching</returns>
        bool Match(Color color1, Color color2);
    }
}