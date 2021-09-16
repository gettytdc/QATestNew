using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Implementation of the Start Point Generator that a returns sequence of points
    /// within a container where an images top left hand may be. This implementation 
    /// gets a sequence of points that make all of the potential start points 
    /// within the container, where the sequence of points is a spiral starting at 
    /// the point where it would expect the top left corner of the image to be if
    /// it has not moved since the container image was captured. It then spirals 
    /// outwards until all of the possible starting points for the image within the
    /// container are returned
    /// </summary>
    public class SpiralStartPointGenerator : IStartPointGenerator
    {
        /// <summary>
        /// Gets a sequence of all points within a container where the specified image's
        /// top left point could possibly be. The sequence starts at the point where 
        /// the image was within the container when the container image was captured and 
        /// spirals out until all of the possible image start points within the container 
        /// have been returned </summary>
        /// <param name="container">A rectangle representing the area within which points should be
        /// generated</param>
        /// <param name="width">The width of the image being searched for</param>
        /// <param name="height">The height of the image being searched for</param>
        /// <param name="originalPosition">The top left corner of where the image in the container
        /// when the image was originally captured</param>
        /// <returns>A spiral sequence that includes every conceivable point
        /// where an image's top left coordinate could be within a container</returns>
        public IEnumerable<Point> GetStartPoints(Rectangle container,
            int width, int height, Point originalPosition)
        {
            var availableStartPointsContainer = new Rectangle(container.X, container.Y, container.Width - width + 1, container.Height - height + 1);
            // Create a new spiral search sequence starting at the top left hand corner of the image
            // as it appears in the container
            var generator =
                new SpiralSearchSequenceGenerator(availableStartPointsContainer, originalPosition);

            return generator.GetSequence();

        }
    }
}