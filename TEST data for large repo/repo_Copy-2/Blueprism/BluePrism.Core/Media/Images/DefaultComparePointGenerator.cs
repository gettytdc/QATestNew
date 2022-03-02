using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Standard implementation that compares each row from left to right in turn
    /// starting at the top
    /// </summary>
    public class DefaultComparePointGenerator : IComparePointGenerator
    {
        /// <summary>
        /// Gets sequence of points from each row from left to right in turn
        /// starting at the top
        /// </summary>
        /// <param name="width">The width of the image being searched for</param>
        /// <param name="height">The height of the image being searched for</param>
        /// <returns>A sequence of point objects in the order by which they should
        /// be compared</returns>
        public IEnumerable<Point> GetComparePoints(int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    yield return new Point(x, y);
                }
            }
        }
    }
}