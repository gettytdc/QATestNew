using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Generates starting points that should be used when searching for a child
    /// image within a container image
    /// </summary>
    public interface IStartPointGenerator
    {
        /// <summary>
        /// Gets the points in the order in which the container image should be 
        /// searched for the child image
        /// </summary>
        /// <param name="container">The area within the container image being searched</param>
        /// <param name="width">The width of the image being searched for</param>
        /// <param name="height">The height of the image being searched for</param>
        /// <param name="originalPosition">The coordinates within the container that the 
        /// top left corner of the image was originally recorded.
        /// </param>
        /// <returns>A sequence of points in the order in which the container image should 
        /// be searched</returns>
        IEnumerable<Point> GetStartPoints(Rectangle container, int width, int height, Point originalPosition);
    }
}