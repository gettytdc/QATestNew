using BluePrism.CharMatching.Properties;
using System;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using BluePrism.BPCoreLib;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Class representing ink on paper as a purely binary value - ie. either the ink
    /// is there or it is not. It holds no colour information or such like.
    /// </summary>
    public class Mask : GridData<bool>
    {
        #region - Class-scope declarations -

        /// <summary>
        /// The regex for parsing the encoded value of a mask
        /// </summary>
        private static readonly Regex EncodedPattern =
            new Regex(@"^(?<width>\d+),(?<height>\d+),(?<bits>[01,]+)$" , RegexOptions.None, RegexTimeout.DefaultRegexTimeout);

        /// <summary>
        /// Class to aid in encoding / decoding masks
        /// </summary>
        private static class MaskEncoder
        {
            /// <summary>
            /// Encodes the data into a single string
            /// </summary>
            /// <param name="data">The data to encode</param>
            /// <returns>An encoded string containing the specified data.</returns>
            public static string Encode(bool[,] data)
            {
                int w = data.GetLength(0);
                int h = data.GetLength(1);
                StringBuilder sb = new StringBuilder(w * h + 10);
                sb.Append(w).Append(',').Append(h).Append(',');
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        sb.Append(data[x, y] ? '1' : '0');
                    }
                }
                return sb.ToString();
            }

            /// <summary>
            /// Decodes the given string into a 2d bool array
            /// </summary>
            /// <param name="value">The value to decode</param>
            /// <returns>The decoded mask value.</returns>
            public static bool[,] Decode(string value)
            {
                Match m = EncodedPattern.Match(value);
                if (!m.Success) throw new ArgumentException(
                    "value", Resources.InvalidEncodedMaskValue + value);

                int w = int.Parse(m.Groups["width"].Value);
                int h = int.Parse(m.Groups["height"].Value);

                // probably a no-op but it was churning out commas at some point...
                // just get rid if they are there
                string bits = m.Groups["bits"].Value.Replace(",", "");

                if (bits.Length != w * h) throw new ArgumentException(
                      "value", Resources.MismatchBetweenSizeAndNumberOfBits + value);

                bool[,] data = new bool[w, h];
                int i = -1;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        data[x, y] = (bits[++i] != '0');
                    }
                }
                return data;
            }
        }

        /// <summary>
        /// Creates mask data of the given size from the given bitmap, using the
        /// given foreground colour as the colour to treat as a 'set' pixel
        /// </summary>
        /// <param name="sz">The size of the mask to create</param>
        /// <param name="img">The image to create a mask from, or null to just
        /// create an empty mask</param>
        /// <param name="fgCol">The colour to use as the foreground colour when
        /// determining if pixels are 'set' or not</param>
        /// <remarks>The size given here is the value used to determine the size of
        /// the mask, regardless of the size of the bitmap. The two sizes should be
        /// the same if a non-null bitmap is given or exceptions may occur.</remarks>
        private static bool[,] ExtractData(Size sz, Image img, Color fgCol)
        {
            bool[,] data = new bool[sz.Width, sz.Height];

            if (img != null)
            {
                int fg = fgCol.ToArgb();
                Bitmap bmp = (img is Bitmap ? (Bitmap)img : new Bitmap(img));
                for (int y = 0, h = img.Height; y < h; y++)
                {
                    for (int x = 0, w = img.Width; x < w; x++)
                    {
                        data[x, y] = (bmp.GetPixel(x, y).ToArgb() == fg);
                    }
                }
            }
            return data;
        }

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new mask from the given bitmap, using the given foreground
        /// colour as the colour to treat as a 'set' pixel
        /// </summary>
        /// <param name="img">The image to create a mask from</param>
        public Mask(Image img, Color fgCol) : this(img.Size, img, fgCol) { }

        /// <summary>
        /// Creates a new mask of the given size from the given bitmap, using the
        /// given foreground colour as the colour to treat as a 'set' pixel
        /// </summary>
        /// <param name="sz">The size of the mask to create</param>
        /// <param name="img">The image to create a mask from, or null to just
        /// create an empty mask</param>
        /// <param name="fgCol">The colour to use as the foreground colour when
        /// determining if pixels are 'set' or not</param>
        /// <remarks>The size given here is the value used to determine the size of
        /// the mask, regardless of the size of the bitmap. The two sizes should be
        /// the same if a non-null bitmap is given or exceptions may occur.</remarks>
        private Mask(Size sz, Image img, Color fgCol)
            : base(ExtractData(sz, img, fgCol)) { }

        /// <summary>
        /// Creates a new mask from the given encoded data.
        /// </summary>
        /// <param name="encodedData">The encoded data from which to create the mask
        /// </param>
        /// <exception cref="ArgumentException">If the given string did not parse
        /// correctly into a mask.</exception>
        /// <seealso cref="Encoded"/>
        public Mask(string encodedData)
            : base(MaskEncoder.Decode(encodedData), false) { }

        /// <summary>
        /// Creates a new mask from the given data.
        /// </summary>
        /// <param name="data">The data to draw from for the given mask. A copy
        /// of the data is used in this mask</param>
        /// <exception cref="ArgumentNullException">If the given data array is null.
        /// </exception>
        public Mask(bool[,] data) : base(data, true) { }

        /// <summary>
        /// Creates a new mask from the given data, copying it first as necessary.
        /// Note that Masks are semantically immutable - ie. their value should not
        /// be changed after creation, so if <paramref name="copyData"/> is false,
        /// the array passed in should not be available for modification in any
        /// other code.
        /// </summary>
        /// <param name="data">The data to draw from for the given mask.</param>
        /// <param name="copyData">True to copy the given data into a new array;
        /// False to use the given array instance itself</param>
        /// <remarks>This constructor is private to enforce the immutable nature
        /// of masks from outside the class. Any calling code is responsible for
        /// ensuring that the array given, if not set to be copied in this
        /// constructor, is not available for modification in any code which may
        /// execute after this constructor has completed.</remarks>
        /// <exception cref="ArgumentNullException">If the given data array is null
        /// </exception>
        private Mask(bool[,] data, bool copyData) : base(data, copyData) { }

        #endregion

        #region - Properties -

        /// <summary>
        /// Get or sets the encoded representation of this mask
        /// </summary>
        /// <exception cref="ArgumentException">When setting, if the given string
        /// does not represent an encoded mask. The string should be in the format:
        /// "{width},{height},{bits}" - eg. "5,4,01110001000010000100" represents a
        /// mask somewhat like :
        /// .***.
        /// ..*..
        /// ..*..
        /// ..*..
        /// </exception>
        public string Encoded { get { return MaskEncoder.Encode(this.Data); } }

        #endregion

        #region - Core Method Specialisations -

        /// <summary>
        /// Gets a mask based on this mask shifted in the given direction by 1 line
        /// </summary>
        /// <param name="dirn">The direction that the mask should be shifted</param>
        /// <returns>A mask with the pattern shifted in the given direction - it will
        /// be the same size as this mask and the end result is equivalent to a mask
        /// after an opposite direction <see cref="Pad"/>() operation and a same
        /// direction <see cref="Strip"/>() operation in sequence.
        /// </returns>
        public Mask Shift(Direction dirn)
        {
            return Shift(dirn, 1);
        }

        /// <summary>
        /// Gets a mask based on this mask shifted in the given direction by the
        /// specified number of lines
        /// </summary>
        /// <param name="dirn">The direction that the mask should be shifted</param>
        /// <returns>A mask with the pattern shifted in the given direction - it will
        /// be the same size as this mask and the end result is equivalent to a mask
        /// after an opposite direction <see cref="Pad"/>() operation and a same
        /// direction <see cref="Strip"/>() operation in sequence.
        /// </returns>
        public Mask Shift(Direction dirn, int lines)
        {
            return (Mask)ShiftCore(dirn, lines);
        }

        /// <summary>
        /// Pads this mask in the given direction by 1 line - filling the new line
        /// with empty "uninked" pixels.
        /// </summary>
        /// <param name="dirn">The direction in which the mask should be padded.
        /// </param>
        public Mask Pad(Direction dirn)
        {
            return Pad(dirn, 1);
        }

        /// <summary>
        /// Pads this mask in the given direction by the specified number of lines -
        /// filling the new lines with empty "uninked" pixels.
        /// </summary>
        /// <param name="dirn">The direction in which the mask should be padded.
        /// </param>
        /// <param name="lines">The number of lines to pad the mask by</param>
        public Mask Pad(Direction dirn, int lines)
        {
            return (Mask)PadCore(dirn, lines);
        }

        /// <summary>
        /// Trims this mask from the given direction by 1 line, effectively deleting
        /// the outer line of the mask in the given direction.
        /// </summary>
        /// <param name="dirn">The direction from which the mask should be trimmed.
        /// </param>
        public Mask Strip(Direction dirn)
        {
            return Strip(dirn, 1);
        }

        /// <summary>
        /// Trims this mask from the given direction by the specified number of
        /// lines, effectively deleting the outer lines of the mask in the given
        /// direction.
        /// </summary>
        /// <param name="dirn">The direction from which the mask should be trimmed.
        /// </param>
        /// <param name="lines">The number of lines to delete from the mask.</param>
        public Mask Strip(Direction dirn, int lines)
        {
            return (Mask)StripCore(dirn, lines);
        }

        /// <summary>
        /// Trims this mask vertically leaving the top row as the first row with
        /// 'ink' on it and the last row as the last row with 'ink' on it, returning
        /// the result.
        /// If there is no ink on this mask, it will return the same value without
        /// any changes.
        /// </summary>
        /// <returns>If this mask is empty, it will be returned unchanged. Otherwise
        /// a mask with all empty rows above and below the first and last marked
        /// pixel is returned.</returns>
        public Mask TrimVertical()
        {
            return TrimVertical(0);
        }

        /// <summary>
        /// Trims this mask vertically leaving the top row as the first row with
        /// 'ink' on it and the last row as the last row with 'ink' on it, returning
        /// the result.
        /// If there is no ink on this mask, it will return the same value without
        /// any changes.
        /// </summary>
        /// <returns>If this mask is empty, it will be returned unchanged. Otherwise
        /// a mask with all empty rows above and below the first and last marked
        /// pixel is returned.</returns>
        public Mask TrimVertical(int padding)
        {
            return (Mask)TrimVerticalCore(padding);
        }

        /// <summary>
        /// Clones this mask, creating an exact copy of it
        /// </summary>
        /// <returns>A copy of this mask</returns>
        public Mask Clone()
        {
            return (Mask)CloneCore();
        }

        /// <summary>
        /// Clones the specified region of this mask into another mask
        /// </summary>
        /// <param name="r">The rectangle for which a mask is required.</param>
        /// <returns>A copy of the specified area of this mask in a new mask.
        /// </returns>
        /// <exception cref="IndexOutOfBoundsException">If any part of the given
        /// rectangle falls outside of this mask.</exception>
        public Mask Clone(Rectangle r)
        {
            return (Mask)CloneCore(r);
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Creates a new Mask with the given data or a copy of it
        /// </summary>
        /// <param name="data">The data to use as the base of the grid data instance
        /// </param>
        /// <param name="copyData">True to copy the data from the given array; False
        /// to just use the array reference given</param>
        /// <returns>A Mask initialised with the given data.</returns>
        protected override GridData<bool> Create(bool[,] data, bool copyData)
        {
            return new Mask(data, copyData);
        }

        /// <summary>
        /// Checks if this mask contains the given mask - ie. all the set pixels in
        /// the given mask are also set in this mask.
        /// </summary>
        /// <param name="m">The mask to see if it is contained within this mask.
        /// </param>
        /// <returns>true if all the pixels set in the given mask are set in this
        /// mask; false otherwise. Note this will return true for an empty mask - ie.
        /// one with no pixels set in it.</returns>
        public bool Contains(Mask m)
        {
            if (m == null)
                throw new ArgumentNullException(nameof(m));
            if (m.Size != this.Size) throw new ArgumentException(
                Resources.MasksMustBeTheSameSizeToCheckForContainment);

            for (int y = 0, h = m.Height; y < h; y++)
            {
                for (int x = 0, w = m.Width; x < w; x++)
                {
                    if (m[x, y] && !this[x, y])
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets a string representation of this canvas
        /// </summary>
        /// <returns>A formatted string representation of this char canvas with
        /// 'X' representing a pixel with ink; '/' representing a pixel with its
        /// ink erased and ' ' representing a pixel which never had ink.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    sb.Append(this[i, j] ? 'X' : ' ');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #endregion

    }
}
