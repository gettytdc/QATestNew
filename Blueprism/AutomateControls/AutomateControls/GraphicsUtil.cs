using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BluePrism.BPCoreLib;

namespace AutomateControls
{
    public static class GraphicsUtil
    {
        /// <summary>Windows messages (WM_*, look in winuser.h)</summary>
        public enum WindowsMessage : int
        {
            WM_SETREDRAW = 0x0b,
            WM_MOUSEWHEEL = 0x020a,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115
        }

        /// <summary>
        /// Gets a rectangle which is the same as the given rectangle, shrunk
        /// by the given number of pixels on all sides.
        /// Note that if the shrinking of the rectangle by the specified amount
        /// would create a negative width or height, the original rectangle is
        /// returned unmolested.
        /// </summary>
        /// <param name="rect">The rectangle to shrink.</param>
        /// <param name="pix">The number of pixels to shrink the rectangle.
        /// </param>
        /// <returns>The rectangle after it has been shrunk by the given number
        /// of pixels.</returns>
        public static Rectangle Shrink(Rectangle rect, int pix)
        {
            int p2 = 2 * pix;
            if (p2 > rect.Height || p2 > rect.Width) // can't feather any more...
                return rect;

            return new Rectangle(
                rect.X + pix, rect.Y + pix, rect.Width - p2, rect.Height - p2);
        }

        /// <summary>
        /// Returns a rectangle which is the same as the given rectangle, but
        /// feathered by the given number of pixels on each side.
        /// </summary>
        /// <param name="rect">The rectangle to feather.</param>
        /// <param name="pix">The number of pixels to feather the rectangle by.
        /// </param>
        /// <returns>The feathered rectangle.</returns>
        public static Rectangle Feather(Rectangle rect, int pix)
        {
            return Shrink(rect, -pix);
        }

        /// <summary>
        /// Creates a graphics path which describes a rounded rectangle, using
        /// the given rectangle and radius.
        /// </summary>
        /// <param name="rect">The rectangle within which a rounded rectangle
        /// path is required.</param>
        /// <param name="radius">The radius in pixels to use for the rounded
        /// corners of the described rectangle.</param>
        /// <returns>A path describing a rectangle with rounded corners.
        /// </returns>
        public static GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            int d = 2 * radius;

            GraphicsPath path = new GraphicsPath();

            // left side + bottom left corner
            path.AddLine(rect.Left, rect.Top + d, rect.Left, rect.Bottom - d);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 180, -90);

            // bottom side + bottom right corner
            path.AddLine(rect.Left + d, rect.Bottom, rect.Right - d, rect.Bottom);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 90, -90);

            // right side + top right corner
            path.AddLine(rect.Right, rect.Bottom - d, rect.Right, rect.Top + d);
            path.AddArc(rect.Right - d, rect.Top, d, d, 0, -90);

            // top side + top left corner
            path.AddLine(rect.Right - d, rect.Top, rect.Left + d, rect.Top);
            path.AddArc(rect.Left, rect.Top, d, d, 270, -90);

            // should actually join up, but make it official.
            path.CloseAllFigures();

            return path;
        }

        /// <summary>
        /// Takes an image an returns a monochrome (ie. 2 colour, not grayscale)
        /// equivalent of it.
        /// </summary>
        /// <param name="img">The image for which a monochrome image is required.
        /// </param>
        /// <returns>A monochrome representation of the source image.</returns>
        public static Image MonochromeImage(Image img)
        {
            return GrayscaleImage(img, 0.8f);
        }

        /// <summary>
        /// Takes an image and returns a grayscale equivalent of it.
        /// </summary>
        /// <param name="img">The image to turn gray</param>
        /// <returns>The grayscaled image or null if no image was provided.
        /// </returns>
        public static Image GrayscaleImage(Image img)
        {
            return GrayscaleImage(img, null);
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
        /// be black. Null indicates no threshold.</param>
        /// <returns>The grayscaled image or null if no image was provided.
        /// </returns>
        private static Image GrayscaleImage(Image img, float? threshold)
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
        /// Draws a 3D line using the given graphics context from the specified
        /// location in the given direction for the number of pixels set.
        /// </summary>
        /// <param name="g">The graphics context on which to draw the 3D line</param>
        /// <param name="from">The point at which to start the line</param>
        /// <param name="dirn">The direction in which the line should be drawn
        /// </param>
        /// <param name="length">The length of the line to draw</param>
        public static void Draw3DLine(
            Graphics g, Point from, ListDirection dirn, int length)
        {
            Point to = from;
            switch (dirn)
            {
                case ListDirection.BottomUp: to.Y -= length; break;
                case ListDirection.TopDown: to.Y += length; break;
                case ListDirection.LeftToRight: to.X += length; break;
                case ListDirection.RightToLeft: to.X -= length; break;
            }
            if (from == to) // nothing to do
                return;

            // Draw the 'main' line
            g.DrawLine(SystemPens.ControlDark, from, to);


            // Draw the 'shadow' line, offset by 1
            Size offset = new Size(0, 1); // vert offset - for horiz lines
            if (dirn == ListDirection.BottomUp || dirn == ListDirection.TopDown)
                offset = new Size(1, 0); // horiz offset - for vert lines

            g.DrawLine(SystemPens.ControlLight, from + offset, to + offset);

        }
    }
}
