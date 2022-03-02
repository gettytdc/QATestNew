using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Searches an image for an instance of another image contained within it. ImageSearcher takes
    /// a lock on the container image and needs to be disposed following use to release the lock. 
    /// </summary>
    /// <remarks>Implementing separately to clsPixRect initially but we may end up extending
    /// clsPixRect as the behaviour is worked out</remarks>
    public class ImageSearcher : IDisposable
    {
        /// <summary>
        /// A BitmapReader for the container image being searched
        /// </summary>
        private BitmapReader _container;

        /// <summary>
        /// The PixelFormat of the container image
        /// </summary>
        private readonly PixelFormat _containerPixelFormat;
        
        /// <summary>
        /// Creates a new ImageSearcher based on the specified container image
        /// </summary>
        /// <param name="container">The container image being searched</param>
        public ImageSearcher(Bitmap container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _containerPixelFormat = container.PixelFormat;
            _container = new BitmapReader(container);
        }

        /// <summary>
        /// Finds the first point at which an image is found within the containing image
        /// </summary>
        /// <param name="subImage">The image to search for. </param>
        /// <param name="originalPosition">
        ///     The original position of the top left corner of the image within the container.
        ///     The search may be optimised to look in or around this location first.
        /// </param>
        /// <param name="searchArea">The area of the containing image within which to search for
        /// the sub image</param>
        /// <returns>A point representing the top-left corner at which the image was found
        /// or null if the image was not found</returns>
        public Point? FindSubImage(Bitmap subImage, Point originalPosition, Rectangle searchArea = default(Rectangle), IColorMatcher colorMatcher = null)
        {
            if (_container == null)
            {
                throw new ObjectDisposedException("The object has been disposed");
            }
            if (subImage.PixelFormat != _containerPixelFormat)
            {
                throw new ArgumentException("The PixelFormat does not match the container image");
            }

            const int ExactColorTolerance = 0;
            colorMatcher = colorMatcher != null ? colorMatcher : new FullColorMatcher(ExactColorTolerance);

            Point? result = null;

            using (var childReader = new BitmapReader(subImage))
            {
                var containerArea = new Rectangle(0, 0, _container.Width, _container.Height);
                if (searchArea.Equals(Rectangle.Empty))
                {
                    searchArea = containerArea;
                }
                else
                {
                    searchArea.Intersect(containerArea);
                }
                var startPointGenerator = new SpiralStartPointGenerator();
                var startPoints = startPointGenerator.GetStartPoints(searchArea,
                    subImage.Width, subImage.Height, originalPosition);
                
                var comparePointGenerator = new SpiralComparePointGenerator();
                var comparePoints = comparePointGenerator.GetComparePoints(subImage.Width, subImage.Height)
                    .ToArray();

                foreach (var startPoint in startPoints)
                {
                    if (ContainsImageAtPoint(childReader, startPoint, comparePoints, colorMatcher))
                    {
                        result = startPoint;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Compares pixels of container image at the starting point with pixels of the image
        /// and indicates whether they match
        /// </summary>
        /// <param name="container">A BitmapReader for the container image</param>
        /// <param name="child">A BitmapReader for the child image</param>
        /// <param name="start">The top-left point within the container image at which 
        ///     to compare pixels with the child image</param>
        /// <param name="comparePoints">A sequence of points containing all points in the
        ///     child image in the order by which they should be compared</param>
        /// <param name="matcher">The IColorMatcher used to compare pixel colours</param>
        /// <returns>True if the container image matches the child image, false if not</returns>
        private bool ContainsImageAtPoint(BitmapReader child, 
            Point start, Point[] comparePoints, IColorMatcher matcher)
        {             

            int length = comparePoints.Length;
            for (int i = 0; i < length; i++)
            {
                var childPoint = comparePoints[i];

                var containerPoint = new Point(childPoint.X + start.X, childPoint.Y + start.Y);
                if (!_container.MatchesOtherAtPoint(containerPoint, child, childPoint, matcher))
                {
                    return false;
                }
            }
            return true;
        }

        public void Dispose()
        {
            if (_container != null)
            {
                _container.Dispose();
                _container = null;
            }
        }
    }
}
