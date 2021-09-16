using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Implementation of the Compare Point Generator that provides the order that 
    /// pixels will be compared within an image. This implementation gets a sequence
    /// of points that make up a rectangle (which represents the image with 0, 0 the 
    /// top left corner of the rectangle), where the sequence is a spiral starting at 
    /// the centre of the recrangle and spiralling outwards until all points in the
    /// specified rectangle are returned. This search is designed to be used when 
    /// there is most likely to be difference towards the centre of an image, as it 
    /// will find that difference the quickest.
    /// </summary>
    public class SpiralComparePointGenerator : IComparePointGenerator
    {
        
        /// <summary>
        /// Get a sequence of every point that makes up a rectangle with the
        /// specified width and height (where the top left corner is (0, 0)) starting
        /// in the centre, and gradually spiralling outwards.
        /// </summary>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        /// <returns>A spiralled sequence that allows you to navigate all of the 
        /// points that make up a rectangle in a spiral starting at the centre
        /// </returns>
        public IEnumerable<Point> GetComparePoints(int width, int height)
        {
            // Get a point as close to the centre of the rectangle as possibe
            var centre = new Point(width / 2, height / 2);

            var generator = new SpiralSearchSequenceGenerator(new Rectangle(0, 0, width, height), centre);
            return generator.GetSequence();
    
        }

    }
}