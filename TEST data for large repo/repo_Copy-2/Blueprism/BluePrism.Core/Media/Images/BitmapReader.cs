using System;
using System.Drawing;
using System.Drawing.Imaging;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Provides rapid access to bitmap data. It works by locking the bitmap data into system memory
    /// using the LockBits method and should be released via the dispose method immediately after 
    /// use. The Bitmap instance itself will not be disposed when the BitmapReader is disposed.
    /// </summary>
    public class BitmapReader : IDisposable
    {
        private readonly Bitmap _bitmap;
        private BitmapData _data;
        private bool disposed = false;

        /// <summary>
        /// Creates a new BitmapReader using the supplied bitmap
        /// </summary>
        /// <param name="bitmap"></param>
        public BitmapReader(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));
            _bitmap = bitmap;
            Width = bitmap.Width;
            Height = bitmap.Height;
            PixelSize = GetPixelSize(bitmap.PixelFormat);
            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            _data = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, bitmap.PixelFormat);
        }

        /// <summary>
        /// The pixel size of the Bitmap
        /// </summary>
        public int PixelSize { get; private set; }

        /// <summary>
        /// The height of the Bitmap
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The width of the Bitmap
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the pixel colour at the specified point
        /// </summary>
        /// <param name="point">The point at which to get the colour</param>
        public Color GetPixel(Point point)
        {
            return GetPixel(point.X, point.Y);
        }

        /// <summary>
        /// Gets the pixel colour at the specified point
        /// </summary>
        /// <param name="x">The X coordinate of the point</param>
        /// <param name="y">The Y coordinate of the point</param>
        public unsafe Color GetPixel(int x, int y)
        {
            byte* pointer = GetPointer(x, y);
            return (PixelSize == 3) ?
                Color.FromArgb(pointer[2], pointer[1], pointer[0]) :
                Color.FromArgb(pointer[3], pointer[2], pointer[1], pointer[0]);
        }
        
        /// <summary>
        /// Gets pointer to data at the specified point
        /// </summary>
        /// <param name="x">The X coordinate of the point</param>
        /// <param name="y">The Y coordinate of the point</param>
        /// <returns></returns>
        private unsafe byte* GetPointer(int x, int y)
        {
            return (byte*)_data.Scan0 + (y * _data.Stride) + (x * PixelSize);
        }

        /// <summary>
        /// Determines whether data at given point matches data at another point in
        /// a different bitmap.
        /// </summary>
        /// <param name="currentImagePoint">The point in the current image. </param>
        /// <param name="otherBitmap">A BitmapReader containing the other bitmap. </param>
        /// <param name="subImagePoint">The point in the sub image. </param>
        /// <param name="matcher">The IColorMatcher used to compare pixel colours</param>
        public bool MatchesOtherAtPoint(Point currentImagePoint, BitmapReader otherBitmap, Point subImagePoint, IColorMatcher matcher)
        {
            var color = GetPixel(currentImagePoint);
            var otherColor = otherBitmap.GetPixel(subImagePoint);
            return matcher.Match(color, otherColor);
        }

        /// <summary>
        /// Gets the number of bytes used per pixel for a given PixelFormat
        /// </summary>
        /// <param name="format">The format of the bitmap</param>
        /// <returns></returns>
        private int GetPixelSize(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 4;
                default:
                    throw new InvalidArgumentException("PixelFormat not supported");
            }
        }

        /// <summary>
        /// Disposes of this BitmapReader. Note that this does not dispose the underlying Bitmap instance.
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;

            _bitmap.UnlockBits(_data);
            _data = null;
        }
    }
}
