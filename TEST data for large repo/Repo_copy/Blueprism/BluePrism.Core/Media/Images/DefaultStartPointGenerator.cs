using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Standard implementation that generates points for each row from left to right in turn
    /// </summary>
    public class DefaultStartPointGenerator : IStartPointGenerator
    {
        /// <summary>
        /// Gets the points in the area of the container image that should be searched
        /// for each row from left to right in turn
        /// </summary>
        /// <param name="container">The area within the container image being searched</param>
        /// <param name="width">The width of the image being searched for</param>
        /// <param name="height">The height of the image being searched for</param>
        /// <param name="originalPosition">The coordinates within the container that the 
        /// top left corner of the image was originally recorded.
        /// </param>
        /// <returns>A sequence of points in the order by which the container image should 
        /// be searched</returns>
        public IEnumerable<Point> GetStartPoints(Rectangle container, int width, int height, Point originalPosition)
        {
            for (int y = container.Top; y <= container.Bottom - height; y++)
            {
                for (int x = container.Left; x <= container.Right - width; x++)
                {
                    yield return new Point(x, y);
                }
            }
        }
    }
}