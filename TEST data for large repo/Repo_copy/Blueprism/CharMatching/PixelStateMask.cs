using System;
using System.Collections.Generic;
using BluePrism.BPCoreLib;
using System.Drawing;
using BluePrism.BPCoreLib.Collections;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Mask of pixel states applied to a character.
    /// </summary>
    public class PixelStateMask : GridData<PixelState>
    {
        #region - Class-scope declarations -

        /// <summary>
        /// Class to aid in encoding / decoding pixel state masks
        /// </summary>
        private static class MaskEncoder
        {
            /// <summary>
            /// Encodes the data into a single string
            /// </summary>
            /// <param name="data">The data to encode</param>
            /// <returns>An encoded string containing the specified data.</returns>
            public static string Encode(PixelState[,] data)
            {
                int w = data.GetLength(0);
                int h = data.GetLength(1);
                List<int> pixels = new List<int>(w * h);

                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        pixels.Add(Convert.ToInt32(data[x, y]));

                return CollectionUtil.Join(pixels, ",");
            }

            /// <summary>
            /// Extracts pixel state data from the given encoded string, assuming the
            /// specified size.
            /// </summary>
            /// <param name="width">The width of the pixel state data.</param>
            /// <param name="height">The height of the pixel state data.</param>
            /// <param name="encoded">The encoded pixel state data.</param>
            /// <returns>A 2d PixelState array </returns>
            public static PixelState[,] Decode(int width, int height, string encoded)
            {
                PixelState[,] _stateMask = new PixelState[width, height];

                if (!string.IsNullOrEmpty(encoded))
                {
                    int i = -1;
                    foreach (string val in encoded.Split(','))
                    {
                        i++;
                        _stateMask[i % width, i / width] = (PixelState)int.Parse(val);
                    }
                }
                return _stateMask;
            }
        }

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new pixel state mask from the given properties
        /// </summary>
        /// <param name="width">The width of the mask</param>
        /// <param name="height">The height of the mask</param>
        /// <param name="encoded">The encoded state mask</param>
        public PixelStateMask(int width, int height, string encoded)
            : base(MaskEncoder.Decode(width, height, encoded)) { }

        /// <summary>
        /// Creates a new pixel state mask from the given properties
        /// </summary>
        /// <param name="width">The width of the mask</param>
        /// <param name="height">The height of the mask</param>
        /// <param name="encoded">The encoded state mask</param>
        public PixelStateMask(int width, int height)
            : this(width, height, null) { }

        public PixelStateMask(PixelState[,] data)
            : base(data, true) { }

        private PixelStateMask(PixelState[,] data, bool copyData)
            : base(data, copyData) { }

        #endregion

        #region - Properties -

        /// <summary>
        /// The encoded form of this pixel state mask
        /// </summary>
        public string Encoded { get { return MaskEncoder.Encode(Data); } }

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
        public PixelStateMask Shift(Direction dirn)
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
        public PixelStateMask Shift(Direction dirn, int lines)
        {
            return (PixelStateMask)ShiftCore(dirn, lines);
        }

        /// <summary>
        /// Pads this mask in the given direction by 1 line - filling the new line
        /// with empty "uninked" pixels.
        /// </summary>
        /// <param name="dirn">The direction in which the mask should be padded.
        /// </param>
        public PixelStateMask Pad(Direction dirn)
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
        public PixelStateMask Pad(Direction dirn, int lines)
        {
            return (PixelStateMask)PadCore(dirn, lines);
        }

        /// <summary>
        /// Strips this mask from the given direction by 1 line, effectively deleting
        /// the outer line of the mask in the given direction.
        /// </summary>
        /// <param name="dirn">The direction from which the mask should be trimmed.
        /// </param>
        public PixelStateMask Strip(Direction dirn)
        {
            return Strip(dirn, 1);
        }

        /// <summary>
        /// Strips this mask from the given direction by the specified number of
        /// lines, effectively deleting the outer lines of the mask in the given
        /// direction.
        /// </summary>
        /// <param name="dirn">The direction from which the mask should be trimmed.
        /// </param>
        /// <param name="lines">The number of lines to delete from the mask.</param>
        public PixelStateMask Strip(Direction dirn, int lines)
        {
            return (PixelStateMask)StripCore(dirn, lines);
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
        public PixelStateMask TrimVertical()
        {
            return TrimVertical(0);
        }

        /// <summary>
        /// Trims this mask vertically leaving the top row as the first row with
        /// 'ink' on it and the last row as the last row with 'ink' on it, and then
        /// applying the given amount of padding to both edges, returning the result.
        /// If there is no ink on this mask, it will return the same value without
        /// any changes.
        /// </summary>
        /// <returns>If this mask is empty, it will be returned unchanged. Otherwise
        /// a mask with all empty rows above and below the first and last marked
        /// pixel is returned.</returns>
        public PixelStateMask TrimVertical(int padding)
        {
            return (PixelStateMask)TrimVerticalCore(padding);
        }

        /// <summary>
        /// Clones this mask, creating an exact copy of it
        /// </summary>
        /// <returns>A copy of this mask</returns>
        public PixelStateMask Clone()
        {
            return (PixelStateMask)CloneCore();
        }

        /// <summary>
        /// Clones the specified region of this mask into another mask
        /// </summary>
        /// <param name="r">The rectangle for which a mask is required.</param>
        /// <returns>A copy of the specified area of this mask in a new mask.
        /// </returns>
        /// <exception cref="IndexOutOfBoundsException">If any part of the given
        /// rectangle falls outside of this mask.</exception>
        public PixelStateMask Clone(Rectangle r)
        {
            return (PixelStateMask)CloneCore(r);
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Creates a new PixelStateMask with the given data or a copy of it
        /// </summary>
        /// <param name="data">The data to use as the base of the grid data instance
        /// </param>
        /// <param name="copyData">True to copy the data from the given array; False
        /// to just use the array reference given</param>
        /// <returns>A PixelStateMask initialised with the given data.</returns>
        protected override GridData<PixelState> Create(
            PixelState[,] data, bool copyData)
        {
            return new PixelStateMask(data, copyData);
        }

        #endregion

    }
}
