using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace BluePrism.UnitTesting.TestSupport
{
    /// <summary>
    /// Creates example bitmap from a list of colours and pixel data stored in a string
    /// format that makes image readable
    /// </summary>
    /// <remarks></remarks>
    public class TestBitmapGenerator
    {

        private PixelFormat _format = PixelFormat.Format32bppArgb;
        private readonly Dictionary<char, Color> _colors = new Dictionary<char, Color>();
        private string _pixelData;

        /// <summary>
        /// Adds a colour and character used to specify it within the pixel data
        /// </summary>
        /// <param name="character"></param>
        /// <param name="color"></param>
        /// <returns>The current instance of TestBitmapGenerator allowing other setup calls
        /// to be chained</returns>
        /// <remarks></remarks>
        public TestBitmapGenerator WithColour(char character, Color color)
        {
            _colors.Add(character, color);
            return this;
        }

        /// <summary>
        /// Adds colour and characters used to specify it within the pixel data
        /// </summary>
        /// <param name="values"></param>
        /// <returns>The current instance of TestBitmapGenerator allowing other setup calls
        /// to be chained</returns>
        /// <remarks></remarks>
        public TestBitmapGenerator WithColours(IDictionary<char, Color> values)
        {
            foreach (char key in values.Keys)
            {
                WithColour(key, values[key]);
            }
            return this;
        }

        /// <summary>
        /// Sets the content of the image from a string containing pixel data. The characters
        /// for each pixel in the string should match a colour specified via WithColour or
        /// WithColours methods. Row lengths must be equal. Any leading or trailing empty 
        /// lines will be discarded. Leading or trailing whitespace on any line is
        /// ignored.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>The current instance of TestBitmapGenerator allowing other setup calls
        /// to be chained</returns>
        public TestBitmapGenerator WithPixels(string data)
        {
            if (string.IsNullOrEmpty(data)) {
                throw new ArgumentException("Data cannot be empty", nameof(data));
            }
            var rows = GetRows(data);
            if (rows.Select(r => r.Length).Distinct().Count() > 1) {
                throw new ArgumentException("Rows should be of equal length", data);
            }
            _pixelData = data;

            return this;
        }

        /// <summary>
        /// Sets the content of the image from an array of strings containing pixel data
        /// for each row. The characters for each pixel in the string should match a colour 
        /// specified via WithColour or WithColours methods. Row lengths must be equal. 
        /// Any leading or trailing empty lines will be discarded.
        /// </summary>
        /// <param name="rows">An array of strings containing pixel data for each row</param>
        /// <returns>The current instance of TestBitmapGenerator allowing other setup calls
        /// to be chained</returns>
        public TestBitmapGenerator WithPixels(string[] rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            return WithPixels(string.Join(Environment.NewLine, rows));
        }

        /// <summary>
        /// Sets the PixelFormat of the image
        /// </summary>
        /// <param name="format">The format to use</param>
        /// <returns>The current instance of TestBitmapGenerator allowing other setup calls
        /// to be chained</returns>
        public TestBitmapGenerator WithPixelFormat(PixelFormat format)
        {
            _format = format;
            return this;
        }

        private static string[] GetRows(string data)
        {
            return data
                .Split(new[] { "\n", "\r\n" }, StringSplitOptions.None)
                .Select(x => x.Trim())
                .SkipWhile(x => x == "")
                .Reverse()
                .SkipWhile(x => x == "")
                .Reverse()
                .ToArray();
        }

    
        /// <summary>
        /// Creates bitmap image from the data specified
        /// </summary>
        public Bitmap Create()
        {
            var rows = GetRows(_pixelData);
            int width = rows.Select(r => r.Length).Distinct().First();
            int height = rows.Length;

            var bitmap = new Bitmap(width, height, _format);
            for (int x = 0; x <= width - 1; x++) {
                for (int y = 0; y <= height - 1; y++) {
                    char colorChar = rows[y][x];
                    Color color = default(Color);
                    if (!_colors.TryGetValue(colorChar, out color)) {
                        throw new InvalidOperationException(string.Format("Unrecognised colour character \"{0}\"", colorChar));
                    }

                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }
        
    }
}
