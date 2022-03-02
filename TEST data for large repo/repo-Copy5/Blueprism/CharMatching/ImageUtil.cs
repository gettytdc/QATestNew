using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using BluePrism.BPCoreLib.Collections;

namespace BluePrism.CharMatching
{
    public static class ImageUtil
    {
        /// <summary>
        /// Gets a mask bitmap for the given image, retaining the specified colour.
        /// </summary>
        /// <param name="img">The image for which a mask bitmap is required.</param>
        /// <param name="retainColour">The colour to set in the returned mask bitmap
        /// </param>
        /// <returns>A bitmap in which all set pixels are black and all other pixels
        /// are white</returns>
        public static Bitmap GetMaskBmp(Image img, Color retainColour)
        {
            Bitmap bm2 =
                new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);

            Bitmap bmp = (img is Bitmap ? (Bitmap)img : new Bitmap(img));
            for (int y = 0; y <= bmp.Height - 1; y++)
            {
                for (int x = 0; x <= bmp.Width - 1; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    if (c.ToArgb() == retainColour.ToArgb())
                    {
                        bm2.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                    }
                    else
                    {
                        bm2.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }
            return bm2;
        }

        /// <summary>
        /// Gets the colour frequencies for the given image in reverse order - ie.
        /// such that the first entry in the dictionary is the most dominant colour,
        /// the second is the next most dominant etc.
        /// </summary>
        /// <param name="img">The image for which colour frequencies are required.
        /// </param>
        /// <returns>A dictionary, in reverse frequency order, of the colours in an
        /// image and the number of pixels which are of that colour.</returns>
        public static IDictionary<Color, int> GetColourFrequencies(Image img)
        {
            Bitmap bmp = (img is Bitmap ? (Bitmap)img : new Bitmap(img));
            clsCounterMap<Color> counter = new clsCounterMap<Color>();
            for (int y = 0; y <= bmp.Height - 1; y++)
            {
                for (int x = 0; x <= bmp.Width - 1; x++)
                {
                    counter[bmp.GetPixel(x, y)]++;
                }
            }
            return counter.CounterOrderedMap;
        }

        /// <summary>
        /// Gets the dominant colour from the given image.
        /// </summary>
        /// <param name="img">The image to check</param>
        /// <returns>The most frequently found colour in the image.</returns>
        public static Color GetDominantColour(Image img)
        {
            return CollectionUtil.First(GetColourFrequencies(img).Keys);
        }

        /// <summary>
        /// Gets the second most dominant colour in an image or
        /// <see cref="Color.Empty"/> if at least 2 distinct colours were not found
        /// in the image.
        /// </summary>
        /// <param name="img">The image to examine</param>
        /// <returns>The second most dominant colour in the image.</returns>
        /// <exception cref="ArgumentNullException">If the given image was null.
        /// </exception>
        public static Color GetSecondDominantColour(Image img)
        {
            if (img == null)
                throw new ArgumentNullException(nameof(img));
            bool first = true;
            foreach (Color c in GetColourFrequencies(img).Keys)
            {
                if (!first)
                    return c;
                first = false;
            }
            return Color.Empty;
        }
    }
}
