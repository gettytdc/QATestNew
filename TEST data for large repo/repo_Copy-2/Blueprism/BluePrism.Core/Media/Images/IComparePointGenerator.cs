using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Generates points in order by which they should be compared when comparing the pixels 
    /// of an image. Implementations will optimise the comparison for certain scenarios, 
    /// aiming to detect differences as soon as possible.
    /// </summary>
    public interface IComparePointGenerator
    {
        /// <summary>
        /// Gets sequence of points in the order by which they should be compared when 
        /// comparing the pixels of an image
        /// </summary>
        /// <param name="width">The width of the image being searched for</param>
        /// <param name="height">The height of the image being searched for</param>
        /// <returns>A sequence of points in the order by which the images should be compared</returns>
        IEnumerable<Point> GetComparePoints(int width, int height);
    }
}