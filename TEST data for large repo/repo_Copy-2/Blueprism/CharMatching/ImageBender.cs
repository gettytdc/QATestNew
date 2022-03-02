using BluePrism.CharMatching.Properties;
using System;
using System.Drawing;
using System.Drawing.Imaging;

using BluePrism.BPCoreLib.Collections;
using System.Diagnostics;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Class which 'bends' images - ie. manipulates them in some way.
    /// It was originally intended to operate on a member image, but almost
    /// all of the methods are static, so it may well become a static class
    /// at some point in the near future.
    /// </summary>
    public class ImageBender
    {
        #region - Static Utility Methods -

        /// <summary>
        /// Thrown when a line operation is attempted on an area of the image
        /// which is not a line.
        /// </summary>
        [Serializable]
        private class InvalidLineException : Exception
        {
            public InvalidLineException(string msg, params object[] args)
                : base(string.Format(msg, args)) { }
        }

        /// <summary>
        /// Takes an image and returns a grayscale equivalent of it, setting
        /// the pixels to maximum intensity if they surpass the given
        /// threshold.
        /// </summary>
        /// <param name="img">The image to turn gray</param>
        /// <param name="threshold">The threshold at which a pixel should be
        /// set to either black or white. 0.8 indicates that anything with a
        /// 0.8 intensity (luminance > 204) will be white, anything else will
        /// be black - ie. setting a threshold will return a 1-bit image.
        /// Null indicates no threshold and also means that the full range of
        /// 255 greys may be utilised in the returned image.</param>
        /// <returns>The grayscaled image or null if no image was provided.
        /// </returns>
        public static Image GrayscaleImage(Image img, float? threshold)
        {
            if (img == null) return null;

            ColorMatrix matrix = new ColorMatrix(new float[][] { 
                new float[] { 0.299f, 0.299f, 0.299f, 0, 0 }, 
                new float[] { 0.587f, 0.587f, 0.587f, 0, 0 }, 
                new float[] { 0.114f, 0.114f, 0.114f, 0, 0 }, 
                new float[] { 0,      0,      0,      1, 0 }, 
                new float[] { 0,      0,      0,      0, 1 }
            });

            Image newImg = (Image)img.Clone();

            using (Graphics gr = Graphics.FromImage(newImg)) // SourceImage is a Bitmap object
            {
                //Create the ImageAttributes object and apply the ColorMatrix
                ImageAttributes attrs = new ImageAttributes();
                attrs.SetColorMatrix(matrix);
                if (threshold.HasValue)
                    attrs.SetThreshold(threshold.Value);

                //Create a new Graphics object from the image.
                using (Graphics g = Graphics.FromImage(newImg))
                {
                    //Draw the image using the ImageAttributes we created.
                    g.DrawImage(newImg,
                        new Rectangle(0, 0, newImg.Width, newImg.Height),
                        0,
                        0,
                        newImg.Width,
                        newImg.Height,
                        GraphicsUnit.Pixel,
                        attrs);
                }
                return newImg;
            };
        }

        /// <summary>
        /// Checks if the given line (horizontal or vertical only) in the
        /// bitmap contains the given colour or not.
        /// </summary>
        /// <param name="bmp">The bitmap to test</param>
        /// <param name="from">The point to go from</param>
        /// <param name="to">The point to go to</param>
        /// <param name="col">The colour to check for</param>
        /// <returns>true if the given colour is in the line linking the
        /// two points in the bitmap; false otherwise.</returns>
        /// <exception cref="InvalidLineException">If the two points do not
        /// describe a horizontal or vertical line</exception>
        private static bool ContainsColour(Bitmap bmp, Point from, Point to, Color col)
        {
            if (from.X != to.X && from.Y != to.Y)
            {
                throw new InvalidLineException(
                    Resources.CanOnlyTestAHorizontalOrVerticalLineNotTheLineFrom0To1,
                    from, to);
            }
            int argb = col.ToArgb();

            if (from.X == to.X)
            {
                for (int y = Math.Min(from.Y, to.Y), toY = Math.Max(from.Y, to.Y); y <= toY; y++)
                    if (argb == bmp.GetPixel(from.X, y).ToArgb())
                        return true;
            }
            else
            {
                for (int x = Math.Min(from.X, to.X), toX = Math.Max(from.X, to.X); x <= toX; x++)
                    if (argb == bmp.GetPixel(x, to.Y).ToArgb())
                        return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the predominant colour in the bitmap - ie. the colour which
        /// is used for the most pixels in the bitmap. If two colours have the
        /// same number of occurrent pixels, one is arbitrarily chosen.
        /// </summary>
        /// <param name="bmp">The bitmap to test.</param>
        /// <returns>The predominant colour in the bitmap</returns>
        public static Color GetPredominantColour(Bitmap bmp)
        {
            clsCounterMap<int> counters = new clsCounterMap<int>();
            for (int x = 0, xMax = bmp.Width; x < xMax; x++)
            {
                for (int y = 0, yMax = bmp.Height; y < yMax; y++)
                    counters[bmp.GetPixel(x, y).ToArgb()]++;
            }
            return Color.FromArgb(counters.HighestKey);
        }

        /// <summary>
        /// Gets the colour distribution of the given image, returning the counts of
        /// each colour in the bitmap mapped against the colour's ARGB value.
        /// </summary>
        /// <param name="img">The image for which the colour distribution is required
        /// </param>
        /// <returns>A counter map of counts against ARGB values found in the image.
        /// </returns>
        public static clsCounterMap<int> GetColourDistribution(Image img)
        {
            using (Bitmap bmp = new Bitmap(img))
            {
                clsCounterMap<int> counters = new clsCounterMap<int>();
                for (int x = 0, xMax = bmp.Width; x < xMax; x++)
                {
                    for (int y = 0, yMax = bmp.Height; y < yMax; y++)
                        counters[bmp.GetPixel(x, y).ToArgb()]++;
                }
                return counters;
            }
        }

        /// <summary>
        /// Gets the predominant colour in the line described in the given bitmap
        /// - ie. the colour which is used for the most pixels in the line.
        /// If two colours have the same number of occurrent pixels, one is
        /// arbitrarily chosen.
        /// </summary>
        /// <param name="bmp">The bitmap to test.</param>
        /// <param name="from">The point the line starts from</param>
        /// <param name="to">The point the line goes to</param>
        /// <returns>The predominant colour in the bitmap</returns>
        /// <exception cref="InvalidLineException">If the two points do not
        /// describe a horizontal or vertical line</exception>
        private static Color GetPredominantColour(Bitmap bmp, Point from, Point to)
        {
            if (from.X != to.X && from.Y != to.Y)
            {
                throw new InvalidLineException(
                    Resources.CanOnlyTestAHorizontalOrVerticalLineNotTheLineFrom0To1,
                    from, to);
            }
            clsCounterMap<int> counters = new clsCounterMap<int>();
            if (from.X == to.X)
            {
                for (int y = Math.Min(from.Y, to.Y), toY = Math.Max(from.Y, to.Y); y <= toY; y++)
                    counters[bmp.GetPixel(from.X, y).ToArgb()]++;
            }
            else
            {
                for (int x = Math.Min(from.X, to.X), toX = Math.Max(from.X, to.X); x <= toX; x++)
                    counters[bmp.GetPixel(x, to.Y).ToArgb()]++;
            }
            return Color.FromArgb(counters.HighestKey);
        }

        /// <summary>
        /// Gets the single colour which occurs on the describe line.
        /// </summary>
        /// <param name="bmp">The bitmap to test</param>
        /// <param name="from">The point to go from</param>
        /// <param name="to">The point to go to</param>
        /// <returns>The single colour which occurs in the given line, or null
        /// if more than one colour occurs on the line</returns>
        /// <exception cref="InvalidLineException">If the two points do not
        /// describe a horizontal or vertical line</exception>
        private static Color? GetSingleColour(Bitmap bmp, Point from, Point to)
        {
            if (from.X != to.X && from.Y != to.Y)
            {
                throw new InvalidLineException(
                    Resources.CanOnlyTestAHorizontalOrVerticalLineNotTheLineFrom0To1,
                    from, to);
            }
            Color? contender = null;
            if (from.X == to.X)
            {
                for (int y = Math.Min(from.Y, to.Y), toY = Math.Max(from.Y, to.Y); y <= toY; y++)
                {
                    Color c = bmp.GetPixel(from.X, y);
                    if (!contender.HasValue)
                        contender = c;
                    else if (contender.Value != c)
                        return null;
                }
            }
            else
            {
                for (int x = Math.Min(from.X, to.X), toX = Math.Max(from.X, to.X); x <= toX; x++)
                {
                    Color c = bmp.GetPixel(x, to.Y);
                    if (!contender.HasValue)
                        contender = c;
                    else if (contender.Value != c)
                        return null;
                }
            }
            return contender;
        }

        /// <summary>
        /// Checks if the pixels on the given line all match the specified colour
        /// </summary>
        /// <param name="bmp">The bitmap to test</param>
        /// <param name="from">The point to go from</param>
        /// <param name="to">The point to go to</param>
        /// <param name="c">The colour to test for on the line</param>
        /// <returns>true if all of the pixels on the described line are of the
        /// same colour as that given</returns>
        /// <exception cref="InvalidLineException">If the two points do not
        /// describe a horizontal or vertical line</exception>
        private static bool MatchesColour(Bitmap bmp, Point from, Point to, Color c)
        {
            return MatchesColour(bmp, from, to, c, 100);
        }

        /// <summary>
        /// Checks if at least a specified percentage of pixels on a described line
        /// match a given colour.
        /// </summary>
        /// <param name="bmp">The bitmap to test</param>
        /// <param name="from">The point to go from</param>
        /// <param name="to">The point to go to</param>
        /// <param name="c">The colour to test for on the line</param>
        /// <param name="percentage">The percentage of pixels which should match
        /// the given colour for this indicate a match</param>
        /// <returns>true if at least the specified percentage of the pixels on
        /// the described line are of the same colour as that given</returns>
        /// <exception cref="InvalidLineException">If the two points do not
        /// describe a horizontal or vertical line</exception>
        private static bool MatchesColour(Bitmap bmp, Point from, Point to, Color c, double percentage)
        {
            if (from.X != to.X && from.Y != to.Y)
            {
                throw new InvalidLineException(
                    Resources.CanOnlyTestAHorizontalOrVerticalLineNotTheLineFrom0To1,
                    from, to);
            }
            int count = 0;
            int other = 0;
            if (from.X == to.X)
            {
                for (int y = Math.Min(from.Y, to.Y), toY = Math.Max(from.Y, to.Y); y <= toY; y++)
                {
                    Color b = bmp.GetPixel(from.X, y);
                    if (c.ToArgb() == b.ToArgb())
                        count++;
                    else
                    {
                        if (percentage == 100.0)
                            return false;
                        other++;
                    }
                }
            }
            else
            {
                for (int x = Math.Min(from.X, to.X), toX = Math.Max(from.X, to.X); x <= toX; x++)
                {
                    Color b = bmp.GetPixel(x, to.Y);
                    if (c.ToArgb() == b.ToArgb())
                        count++;
                    else
                    {
                        if (percentage == 100.0)
                            return false;
                        other++;
                    }
                }
            }
            double total = count + other;
            if (total == 0)
                return true;
            double pc = (100.0 * count) / total;
            return (pc >= percentage);
        }

        // Er, the percentage required for something
        private const double percReqd = 100.0;

        public static Rectangle FindRectangleOfColour(Bitmap bmp, Color color)
        {
            Point? origin = FindCornerOfColour(bmp, color, Corner.TopLeft);
            if (!origin.HasValue)
                return Rectangle.Empty;

            Point? farpoint = FindCornerOfColour(bmp, color, Corner.BottomRight);
            Debug.Assert(farpoint.HasValue);
            if (farpoint.Value == origin.Value)
                return new Rectangle(origin.Value, Size.Empty);
            Point from = origin.Value;
            Point to = farpoint.Value;
            // now, "to" represents point (ie. col & row) in which the first pixel
            // of the specified colour appears, so we need to include that - ie.
            // add 1 to the width & height to make it inclusive.
            to.Offset(1, 1);
            return new Rectangle(from, new Size(to.X + 1 - from.X, to.Y + 1 - from.Y));

        }
        public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

        public static Point? FindOriginOfColour(Bitmap bmp, Color color)
        {
            return FindCornerOfColour(bmp, color, Corner.TopLeft);
        }

        public static Point? FindCornerOfColour(Bitmap bmp, Color color, Corner which)
        {
            int argb = color.ToArgb();

            int w = bmp.Width;
            int h = bmp.Height;

            int yFrom, yTo, yStep, xFrom, xTo, xStep;
            switch (which)
            {
                case Corner.TopLeft:
                    yFrom = 0; yTo = h - 1; yStep = 1;
                    xFrom = 0; xTo = w - 1; xStep = 1;
                    break;
                case Corner.TopRight:
                    yFrom = 0; yTo = h - 1; yStep = 1;
                    xFrom = w - 1; xTo = 0; xStep = -1;
                    break;
                case Corner.BottomRight:
                    yFrom = h - 1; yTo = 0; yStep = -1;
                    xFrom = w - 1; xTo = 0; xStep = -1;
                    break;
                case Corner.BottomLeft:
                    yFrom = h - 1; yTo = 0; yStep = -1;
                    xFrom = 0; xTo = w - 1; xStep = 1;
                    break;
                default: throw new NotSupportedException();
            }

            // Find the first column
            for (int x = xFrom; x != (xTo + xStep); x += xStep)
            {
                if (ContainsColour(bmp, new Point(x, 0), new Point(x, h - 1), color))
                {
                    // Now find the first row..
                    for (int y = yFrom; y != (yTo + yStep); y += yStep)
                    {
                        if (ContainsColour(bmp, new Point(0, y), new Point(w - 1, y), color))
                            return new Point(x, y);

                        // if (bmp.GetPixel(x, y).ToArgb() == argb)
                    }
                }
            }

            return null;
        }

        public static Bitmap AsBitmap(Image img)
        {
            if (img == null)
                return null;
            Bitmap b = img as Bitmap;
            return (b != null ? b : new Bitmap(img));
        }

        public static Image SubImage(Image image, Rectangle region)
        {
            Rectangle rect = new Rectangle(Point.Empty, image.Size);
            rect.Intersect(region);
            if (rect != region)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format(Resources.Rectangle0IsOutOfBoundsForTheGivenImageSize1, region, image.Size));
            }

            Bitmap b = AsBitmap(image);
            if (b == null)
                return null;
            return b.Clone(region, b.PixelFormat);
        }

        private static bool ColorPixelsMatch(Bitmap keyBmp, Rectangle keyRect, Bitmap searchBmp, Rectangle searchRect, Color col)
        {
            if (keyRect.Size != searchRect.Size)
                throw new ArgumentException(string.Format(Resources.RectanglesMustBeSameSize01, keyRect, searchRect));
            int argb = col.ToArgb();

            Point offset = searchRect.Location - (Size)keyRect.Location;
            int xOffset = offset.X;
            int yOffset = offset.Y;
            bool foundAny = false;

            for (int x1 = keyRect.Left; x1 < keyRect.Right; x1++)
            {
                if (x1 >= keyBmp.Width)
                    break;

                int x2 = x1 + xOffset;
                for (int y1 = keyRect.Top; y1 < keyRect.Bottom; y1++)
                {
                    if (y1 >= keyBmp.Height)
                        break;

                    int y2 = y1 + yOffset;
                    if (keyBmp.GetPixel(x1, y1).ToArgb() == argb)
                    {
                        foundAny = true;
                        if (searchBmp.GetPixel(x2, y2).ToArgb() != argb)
                            return false;
                    }
                }
            }
            return foundAny;
        }

        public static Rectangle? Contains(Bitmap src, Bitmap dest, Rectangle box, Color fgColor)
        {
            // go through each pixel in the bitmap and see if the subimage in the
            // given bitmap within the given rectangle matches
            int w = box.Width;
            int h = box.Height;
            for (int x = 0; x < src.Width - w; x++)
            {
                for (int y = 0; y < src.Height - h; y++)
                {
                    Rectangle rect = new Rectangle(x, y, w, h);
                    if (ColorPixelsMatch(dest, box, src, rect, fgColor))
                        return rect;
                }
            }
            return null;
        }

        #endregion

        // The image we are 'bending'
        private Image _img;

        /// <summary>
        /// Creates a new image bender with the given source image.
        /// </summary>
        /// <param name="img">The image on which the image manipulation will
        /// take place.</param>
        public ImageBender(Image img)
        {
            _img = img;
        }

        public Rectangle FindBox(Point p)
        {
            return FindBox(new Rectangle(p, Size.Empty), true);
        }

        public Rectangle FindBox(Rectangle rect, bool expand)
        {
            using (Image i = GrayscaleImage(_img, 0.8f))
            {
                // Get the image into a bitmap so we can manipulate at pixel level
                Bitmap bmp = (i as Bitmap);
                if (bmp==null)
                    bmp = new Bitmap(i);

                Point pNW = rect.Location;
                Point pNE = new Point(rect.Right, rect.Top);
                Point pSW = new Point(rect.Left, rect.Bottom);
                Point pSE = new Point(rect.Right, rect.Bottom);

                Color startingColour = GetPredominantColour(bmp, pNW, pNE);
                
                // Assume that we want the opposite of the predominant colour for our box
                Color target = (startingColour.ToArgb() == Color.Black.ToArgb()
                    ? Color.White 
                    : Color.Black);

                // So work our way out until we find a line of our target colour.

                // top
                while (pNW.Y >= 0 && !MatchesColour(bmp, pNW, pNE, target, percReqd))
                {
                    pNW.Y--;
                    pNE.Y--;
                }
                if (pNW.Y < 0)
                    return rect;

                // right
                while (pNE.X < bmp.Width && !MatchesColour(bmp, pNE, pSE, target, percReqd))
                {
                    pNE.X++;
                    pSE.X++;
                }
                if (pNE.X >= bmp.Width)
                    return rect;

                // bottom
                while (pSE.Y < bmp.Height && !MatchesColour(bmp, pSE, pSW, target, percReqd))
                {
                    pSE.Y++;
                    pSW.Y++;
                }
                if (pSE.Y >= bmp.Height)
                    return rect;

                // and left
                while (pNW.X >= 0 && !MatchesColour(bmp, pNW, pSW, target, percReqd))
                {
                    pNW.X--;
                    pSW.X--;
                }
                if (pNW.X < 0)
                    return rect;

                return new Rectangle(pNW, new Size(pNE.X - pNW.X, pSW.Y - pNW.Y));          
                    
            }
        }

    }
}
