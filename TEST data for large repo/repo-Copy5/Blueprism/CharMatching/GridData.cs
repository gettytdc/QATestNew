using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Base class for a 2-d grid of value type data, which can recognise 'empty'
    /// (ie. non-default) cells and offers operations for trimming, padding and
    /// shifting the values in the grid as well as equality checks and hashing.
    /// By default, this is immutable. Subclasses may open up access to the data
    /// by using the protected <see cref="Data"/> property if required.
    /// </summary>
    /// <typeparam name="T">The type of data backed by the GridData. It is essential
    /// that this type overrides the <see cref="Object.Equals"/> method to test
    /// other data objects for equality, as this is used throughout to test values
    /// of the backed grid.</typeparam>
    public abstract class GridData<T> where T : struct
    {
        #region - Class scope declarations -

        /// <summary>
        /// Tests the two masks to see if they are equal
        /// </summary>
        /// <param name="m1">The first mask to test</param>
        /// <param name="m2">The second mask to test</param>
        /// <returns>True if the masks are equal, false otherwise.</returns>
        public static bool operator ==(GridData<T> g1, GridData<T> g2)
        {
            // Cast into object to avoid recursive calls to this operator
            return (((object)g1) == null ? ((object)g2) == null : g1.Equals(g2));
        }

        /// <summary>
        /// Tests the two masks to see if they are unequal
        /// </summary>
        /// <param name="m1">The first mask to test</param>
        /// <param name="m2">The second mask to test</param>
        /// <returns>False if the masks are equal, true otherwise.</returns>
        public static bool operator !=(GridData<T> g1, GridData<T> g2)
        {
            return !(g1 == g2);
        }

        #endregion

        #region - Member variables -

        /// <summary>
        /// The data that this grid data represents
        /// </summary>
        private readonly T[,] _data;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new GridData object wrapping a copy of the given data.
        /// </summary>
        /// <param name="data"></param>
        protected GridData(T[,] data) : this(data, true) { }

        /// <summary>
        /// Creates a new GridData object wrapping the given data or a copy of it
        /// as specified.
        /// </summary>
        /// <param name="data">The data to use for this grid data</param>
        /// <param name="copyData">True to copy the given data array; False to
        /// just use the given array reference itself</param>
        protected GridData(T[,] data, bool copyData)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            _data = (copyData ? (T[,])data.Clone() : data);
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets whether the pixel is set within this mask at the given co-ordinates
        /// </summary>
        /// <param name="x">The x co-ord of the required location</param>
        /// <param name="y">The y co-ord of the required location</param>
        /// <returns>A flag indicating if a pixel at the given location is set.
        /// </returns>
        public T this[int x, int y]
        {
            get { return this[new Point(x, y)]; }
        }

        /// <summary>
        /// Gets whether the pixel is set within this mask at the given co-ordinates
        /// </summary>
        /// <param name="p">The point of the pixel required.</param>
        /// <returns>A flag indicating if a pixel at the given location is set.
        /// </returns>
        /// <exception cref="OutOfRangeException">If the pixel location given falls
        /// outside this grid.</exception>
        public T this[Point p]
        {
            get
            {
                if (!Contains(p))
                    throw new OutOfRangeException(
                        Resources.Point0IsNotWithinGridBounds1, p, Size);
                return _data[p.X, p.Y];
            }
        }

        /// <summary>
        /// The width of this mask
        /// </summary>
        public int Width { get { return _data.GetLength(0); } }

        /// <summary>
        /// The height of this mask
        /// </summary>
        public int Height { get { return _data.GetLength(1); } }

        /// <summary>
        /// The size of this mask
        /// </summary>
        public Size Size { get { return new Size(Width, Height); } }

        /// <summary>
        /// The underlying data for this mask
        /// </summary>
        protected T[,] Data
        {
            get { return _data; }
        }

        /// <summary>
        /// A copy of the underlying data for this mask. Changes made to the
        /// returned array are not reflected in this object.
        /// </summary>
        public T[,] CopiedValue { get { return (T[,])_data.Clone(); } }

        /// <summary>
        /// Checks if this grid data object is empty or not.
        /// </summary>
        public bool IsEmpty
        {
            get { return (NonEmptyCount == 0); }
        }

        /// <summary>
        /// A count of all of the 'non-empty' values in this grid. In this context,
        /// 'non-empty' means any value other than the default value for the type
        /// of this grid.
        /// </summary>
        public int NonEmptyCount
        {
            get
            {
                return GetCount(delegate(T value) {
                    return !value.Equals(default(T));
                });

            }
        }

        /// <summary>
        /// The index of the first non-empty column in this grid, or -1 if the grid
        /// is empty.
        /// </summary>
        public int FirstNonEmptyColumn
        {
            get
            {
                for (int i = 0, w = Width; i < w; i++)
                {
                    if (!IsColumnEmpty(i))
                        return i;
                }
                return -1;
            }
        }

        /// <summary>
        /// The index of the last non-empty column in this grid, or -1 if the grid
        /// is empty.
        /// </summary>
        public int LastNonEmptyColumn
        {
            get
            {
                for (int i = Width - 1; i >= 0; --i)
                {
                    if (!IsColumnEmpty(i))
                        return i;
                }
                return -1;
            }
        }

        /// <summary>
        /// The index of the first non-empty row in this grid, or -1 if the grid
        /// is empty.
        /// </summary>
        public int FirstNonEmptyRow
        {
            get
            {
                for (int i = 0, h = Height; i < h; i++)
                {
                    if (!IsRowEmpty(i))
                        return i;
                }
                return -1;
            }
        }

        /// <summary>
        /// The index of the last non-empty row in this grid, or -1 if the grid
        /// is empty.
        /// </summary>
        public int LastNonEmptyRow
        {
            get
            {
                for (int i = Height - 1; i >= 0; --i)
                {
                    if (!IsRowEmpty(i))
                        return i;
                }
                return -1;
            }
        }

        /// <summary>
        /// Gets this char canvas as a string display.
        /// </summary>
        public string StringDisplay
        {
            get { return ToString(); }
        }

        #endregion

        #region - Core methods -

        /// <summary>
        /// Trims this grid vertically leaving the top row as the first row with
        /// 'ink' on it and the last row as the last row with 'ink' on it, returning
        /// the result.
        /// If there is no ink on this grid, it will return the same value without
        /// any changes.
        /// </summary>
        /// <returns>If this grid is empty, it will be returned unchanged. Otherwise
        /// a grid with all empty rows above and below the first and last marked
        /// pixel is returned.</returns>
        protected GridData<T> TrimVerticalCore(int padding)
        {
            return TrimVerticalCore(padding, padding, 0);
        }

        /// <summary>
        /// Trims this grid vertically leaving the top row as the first row with
        /// 'ink' on it and the last row as the last row with 'ink' on it, returning
        /// the result.
        /// If there is no ink on this grid, it will return the same value without
        /// any changes.
        /// </summary>
        /// <returns>If this grid is empty, it will be returned unchanged. Otherwise
        /// a grid with all empty rows above and below the first and last marked
        /// pixel is returned.</returns>
        protected GridData<T> TrimVerticalCore(
            int topPadding, int bottomPadding, int minHeight)
        {
            // Get the first and last rows with any pixels set in it
            int firstRow = FirstNonEmptyRow;
            int lastRow = LastNonEmptyRow;

            // If there are no pixels set, just return the grid as is.
            if (lastRow <= firstRow)
                return this;

            // Height is (lastRow + 1 - firstRow) (to get height without padding)
            // plus the padding required above and below the image.
            int trimmedHeight = (lastRow + 1) + topPadding + bottomPadding - firstRow;

            // If that's not enough, just pad out the bottom a bit further.
            if (trimmedHeight < minHeight)
                trimmedHeight = minHeight;

            T[,] data = new T[Width, trimmedHeight];
            for (int x = 0, w = Width; x < w; x++)
            {
                // Only apply those between first and last rows - the others are
                // all initialised to 'unset' anyway, no need to overwrite with
                // the same data.
                // We also translate the y co-ord from this to the new Mask using
                // the padding / firstRow values
                for (int y = firstRow; y <= lastRow; y++)
                {
                    data[x, y - firstRow + topPadding] = _data[x, y];
                }
            }
            return Create(data, false);
        }

        /// <summary>
        /// Trims this grid horizontally leaving the top column as the first column
        /// with 'ink' on it and the last column as the last column with 'ink' on it,
        /// returning the result.
        /// If there is no ink on this grid, it will return the same value without
        /// any changes.
        /// </summary>
        /// <returns>If this grid is empty, it will be returned unchanged. Otherwise
        /// a grid with all empty columns above and below the first and last marked
        /// pixel is returned.</returns>
        protected GridData<T> TrimHorizontalCore(int padding)
        {
            // Get the first and last rows with any pixels set in it
            int firstCol = FirstNonEmptyColumn;
            int lastCol = LastNonEmptyColumn;

            // If there are no pixels set, just return the mask as is.
            if (lastCol <= firstCol)
                return this;

            // Width is (lastCol + 1 - firstCol) (to get width without padding)
            // plus the padding required left and right of the image.
            T[,] data = new T[(lastCol + 1) + (2 * padding) - firstCol, Height];

            for (int y = 0, h = Height; y < h; y++)
            {
                // Only apply those between first and last cols - the others are
                // all initialised to 'unset' anyway, no need to overwrite with
                // the same data.
                // We also translate the x co-ord from this to the new Mask using
                // the padding / firstCol values
                for (int x = firstCol; x <= lastCol; x++)
                {
                    data[x - firstCol + padding, y] = _data[x, y];
                }
            }
            return Create(data, false);
        }

        /// <summary>
        /// Pads this grid in the given direction by the specified number of lines -
        /// filling the new lines with empty default values.
        /// </summary>
        /// <param name="dirn">The direction in which the mask should be padded.
        /// </param>
        /// <param name="lines">The number of lines to pad the mask by</param>
        protected GridData<T> PadCore(Direction dirn, int lines)
        {
            int padx = 0, pady = 0;
            T[,] arr;

            // Top and Bottom create the same size array - likewise Left and Right.
            // Top and Left need to pad the data when transferred to the new array
            switch (dirn)
            {
                case Direction.Top: pady = lines; goto case Direction.Bottom;
                case Direction.Bottom: arr = new T[Width, Height + lines]; break;

                case Direction.Left: padx = lines; goto case Direction.Right;
                case Direction.Right: arr = new T[Width + lines, Height]; break;

                // No direction, no padding, no change.
                default:
                    return this;

            }
            // The array should be initialized to 'false' already - ie. all entries
            // are set to 'uninked' so we should be able to just transfer (padded
            // as appropriate) the data from the source array to the new one
            for (int i = 0, w = Width; i < w; i++)
            {
                for (int j = 0, h = Height; j < h; j++)
                {
                    arr[i + padx, j + pady] = _data[i, j];
                }
            }

            return Create(arr, false);
        }

        /// <summary>
        /// Trims this mask from the given direction by the specified number of
        /// lines, effectively deleting the outer lines of the mask in the given
        /// direction.
        /// </summary>
        /// <param name="dirn">The direction from which the mask should be trimmed.
        /// </param>
        /// <param name="lines">The number of lines to delete from the mask.</param>
        protected GridData<T> StripCore(Direction dirn, int lines)
        {
            // We'll clone the array from x|y(start-end).
            // Depending on the direction of the trimming, those lines may differ
            int xStart = 0, xEnd = Width;
            int yStart = 0, yEnd = Height;

            switch (dirn)
            {
                case Direction.Top: yStart = lines; break;
                case Direction.Right: xEnd -= lines; break;
                case Direction.Bottom: yEnd -= lines; break;
                case Direction.Left: xStart = lines; break;

                // No direction given, no trimming required.
                default:
                    return this;
            }
            // If we've trimmed down to nothing, that's a no-no
            if (xEnd <= xStart || yEnd <= yStart)
            {
                throw new LimitReachedException(
                    Resources.ThisMaskCannotBeStrippedFromThe0AnyFurther,
                    dirn.ToString().ToLower());
            }

            // Otherwise take a subset of the data using the new Start/End delimiters
            T[,] arr = new T[xEnd - xStart, yEnd - yStart];
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    arr[i - xStart, j - yStart] = _data[i, j];
                }
            }
            return Create(arr, false);
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
        protected GridData<T> ShiftCore(Direction dirn, int lines)
        {
            // There's probably a way of doing this without creating a redundant
            // intermediate array, but since these arrays are typically 15x20 or
            // thereabouts, it seems overkill to try and work it out.
            return PadCore(GetOpposite(dirn), lines).StripCore(dirn, lines);
        }

        /// <summary>
        /// Clones this grid, creating an exact copy of it
        /// </summary>
        /// <returns>A copy of this mask</returns>
        protected GridData<T> CloneCore()
        {
            GridData<T> g = Create(_data, true);
            Debug.Assert(g._data != _data,
                "Cloned grid data refers to same array as old grid data");
            return g;
        }

        /// <summary>
        /// Clones the specified region of this grid data
        /// </summary>
        /// <param name="r">The rectangle for which a clone is required.</param>
        /// <returns>A copy of the specified area of this grid in a new grid.
        /// </returns>
        /// <exception cref="IndexOutOfBoundsException">If any part of the given
        /// rectangle falls outside of this grid.</exception>
        protected GridData<T> CloneCore(Rectangle r)
        {
            T[,] data = new T[r.Width, r.Height];
            for (int x = 0; x < r.Width; x++)
            {
                for (int y = 0; y < r.Height; y++)
                {
                    data[x, y] = _data[r.X + x, r.Y + y];
                }
            }
            return Create(data, false);
        }

        /// <summary>
        /// Gets a sub-grid drawn from the data on this grid and contained in
        /// the specified rectangle.
        /// </summary>
        /// <param name="r">The rectangle providing the co-ords for which the
        /// subgrid is required.</param>
        /// <returns>A grid containing the data from this grid within the
        /// specified rectangle.</returns>
        /// <remarks>If the rectangle exceeds the bounds of this grid, any extra
        /// cells will be initialised with the default value</remarks>
        protected GridData<T> SubGridCore(Rectangle r)
        {
            T[,] data = new T[r.Width, r.Height];
            for (int i = r.X, right = r.Right; i < right; i++)
            {
                for (int j = r.Y, bottom = r.Bottom; j < bottom; j++)
                {
                    if (i >= 0 && j >= 0 && i < Width && j < Height)
                        data[i - r.X, j - r.Y] = this[i, j];
                }
            }
            return Create(data, false);
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Creates a GridData instance using the given data, or a copy of the
        /// given data as specified.
        /// </summary>
        /// <param name="data">The data to use as the base of the grid data instance
        /// </param>
        /// <param name="copyData">True to copy the data from the given array; False
        /// to just use the array reference given</param>
        /// <returns>A GridData instance based on the given data.</returns>
        protected abstract GridData<T> Create(T[,] data, bool copyData);

        /// <summary>
        /// Gets the char equivalent of the given value. By default this uses either
        /// 'X' for a non-default value or ' ' (ie. a space) for a value equal to the
        /// default value of the type.
        /// </summary>
        /// <param name="value">The value for which a char equivalent is required.
        /// </param>
        /// <returns>The char equivalent of the given value.</returns>
        protected virtual char GetCharEquivalent(T value)
        {
            if (value.Equals(default(T)))
                return ' ';
            return 'X';
        }

        /// <summary>
        /// Gets a string representation of this canvas
        /// </summary>
        /// <returns>A formatted string representation of this char canvas with
        /// 'X' representing a pixel with ink; '/' representing a pixel with its
        /// ink erased and ' ' representing a pixel which never had ink.</returns>
        public override string ToString()
        {
            Dictionary<T, char> map = new Dictionary<T, char>();
            StringBuilder sb = new StringBuilder();
            int y = 0;

            Each(delegate(T val, Point coord) {
                // Blank line when the y co-ord changes
                if (coord.Y > y)
                {
                    sb.AppendLine();
                    y = coord.Y;
                }
                char chr;
                if (!map.TryGetValue(val, out chr))
                    map[val] = (chr = GetCharEquivalent(val));
                sb.Append(chr);
            });
            return sb.ToString();
        }

        /// <summary>
        /// Checks if this grid contains the given point, ie. the point is not
        /// considered out of bounds for this grid.
        /// </summary>
        /// <param name="pt">The point to check for on this grid</param>
        /// <returns>true if the point refers to a point on this grid; false if it
        /// is out of bounds.</returns>
        public bool Contains(Point pt)
        {
            return (pt.X >= 0 && pt.X < Width && pt.Y >= 0 && pt.Y < Height);
        }

        /// <summary>
        /// Gets the opposite direction to the given value. If the direction does not
        /// represent a single direction value, a value of
        /// <see cref="Direction.None"/> is returned, otherwise the opposite
        /// direction value to that given is returned.
        /// </summary>
        /// <param name="dirn">The direction for which the opposite is required -
        /// this should be a single direction value, ie. not a combination of two
        /// Direction values.</param>
        /// <returns>The opposite direction to that given or Direction.None if the
        /// given direction was not a single direction value.</returns>
        protected Direction GetOpposite(Direction dirn)
        {
            switch (dirn)
            {
                case Direction.Top: return Direction.Bottom;
                case Direction.Bottom: return Direction.Top;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default:
                    return Direction.None;
            }
        }

        /// <summary>
        /// Counts the empty lines in this grid data object from the given direction.
        /// </summary>
        /// <param name="dirn">The direction from which the empty lines should be
        /// counted</param>
        /// <returns>The number of empty lines found in this grid data from the given
        /// direction</returns>
        public int CountEmptyLines(Direction dirn)
        {
            int start, end, step;
            // The function to call to see if a row/column is empty
            Func<int, bool> callee = null;

            switch (dirn)
            {
                case Direction.Top:
                    start = 0; end = Height - 1; step = 1; callee = IsRowEmpty;
                    break;
                case Direction.Bottom:
                    start = Height - 1; end = 0; step = -1; callee = IsRowEmpty;
                    break;
                case Direction.Left:
                    start = 0; end = Width - 1; step = 1; callee = IsColumnEmpty;
                    break;
                case Direction.Right:
                    start = Width - 1; end = 0; step = -1; callee = IsColumnEmpty;
                    break;

                default: throw new ArgumentException(
                    Resources.InvalidDirectionSpecified + dirn);
            }

            int count = 0;
            for (int i = start; i != end + step; i += step)
            {
                if (!callee(i))
                    return count;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Gets a count of all elements mathing the given predicate
        /// </summary>
        /// <param name="match">The predicate delegate which determines a match or
        /// otherwise.</param>
        /// <returns>A count of all elements in this grid which matched the given
        /// predicate.</returns>
        public int GetCount(Predicate<T> match)
        {
            int count = 0;
            Each(delegate(T element) { if (match(element)) count++; });
            return count;
        }

        /// <summary>
        /// Gets a count of all elements with the given value in this grid.
        /// </summary>
        /// <param name="value">The value to search for within the grid</param>
        /// <returns>The number of elements within the grid which contained the given
        /// value</returns>
        public int GetCount(T value)
        {
            return GetCount(delegate(T element) {
                return Object.Equals(value, element);
            });
        }

        /// <summary>
        /// Calls an action on all elements within the grid
        /// </summary>
        /// <param name="action">The action delegate to perform for each value in
        /// the grid. The only parameter given is the value of the element.</param>
        public void Each(Action<T> action)
        {
            Each(delegate(T val, Point p) { action(val); });
        }

        /// <summary>
        /// Calls an action on all elements in this grid.
        /// </summary>
        /// <param name="action">The action delegate to perform for each value in
        /// this grid. The parameters given are 1) the value of the element and
        /// 2) the co-ordinate of the element within the grid.</param>
        public void Each(Action<T, Point> action)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    action(_data[i, j], new Point(i, j));
                }
            }
        }

        /// <summary>
        /// Checks if all of the elements in the specified row are set to the default
        /// value.
        /// </summary>
        /// <param name="row">The (zero-based) index of the row to test</param>
        /// <returns>True if every cell in the specified row holds the default value
        /// for the data type of this GridData class; False if any cell is found to
        /// hold anything other than the default</returns>
        public bool IsRowEmpty(int row)
        {
            return (GetColumnOfFirstNonEmptyCell(row) == -1);
        }

        /// <summary>
        /// Checks if all of the elements in the specified column are set to the
        /// default value.
        /// </summary>
        /// <param name="col">The (zero-based) index of the column to test.</param>
        /// <returns>True if every cell in the specified column holds the default
        /// value; false if any cell is found to hold another value.</returns>
        public bool IsColumnEmpty(int col)
        {
            return (GetRowOfFirstNonEmptyCell(col) == -1);
        }

        /// <summary>
        /// Gets the 0-based index of the first row to contain a non-default value in
        /// the specified column.
        /// </summary>
        /// <param name="col">The zero-based index of the column of interest</param>
        /// <returns>The zero-based index of the first row which contains a non-empty
        /// cell in the specified column, or -1 if no such row exists.</returns>
        /// <exception cref="OutOfRangeException">If the given column was negative or
        /// beyond the width of this grid</exception>
        public int GetRowOfFirstNonEmptyCell(int col)
        {
            if (col < 0 || col >= Width)
                throw new OutOfRangeException(
                    Resources.Column0NotWithinGridOfWidth1, col, Width);
            T defaultVal = default(T);
            for (int y = 0, h = Height; y < h; y++)
            {
                if (!_data[col, y].Equals(defaultVal))
                    return y;
            }
            return -1;
        }

        /// <summary>
        /// Gets the 0-based index of the first column to contain a non-default value
        /// in the specified row.
        /// </summary>
        /// <param name="row">The zero-based index of the row of interest</param>
        /// <returns>The zero-based index of the first column within the specified
        /// row to contain a non-default value, or -1 if no such exists.</returns>
        /// <exception cref="OutOfRangeException">If the given row was negative or
        /// beyond the height of this grid</exception>
        public int GetColumnOfFirstNonEmptyCell(int row)
        {
            if (row < 0 || row >= Height)
                throw new OutOfRangeException(
                    Resources.Row0NotWithinGridOfHeight1, row, Height);

            // Test against the default value for this type.
            // If any non-default values are found, the row is not empty
            // Otherwise, the row is empty.
            T defaultVal = default(T);

            for (int x = 0, w = Width; x < w; x++)
            {
                if (!_data[x, row].Equals(defaultVal))
                    return x;
            }
            return -1;
        }

        /// <summary>
        /// Gets the 0-based index of the last column to contain a non-default value
        /// in the specified row.
        /// </summary>
        /// <param name="row">The zero-based index of the row of interest</param>
        /// <returns>The zero-based index of the last column within the specified
        /// row to contain a non-default value, or -1 if no such exists.</returns>
        public int GetColumnOfLastNonEmptyCell(int row)
        {
            // Test against the default value for this type.
            // If any non-default values are found, the row is not empty
            // Otherwise, the row is empty.
            T defaultVal = default(T);

            for (int x = Width - 1; x >= 0; x--)
            {
                if (!_data[x, row].Equals(defaultVal))
                    return x;
            }
            return -1;
        }

        /// <summary>
        /// Gets the colour equivalent of the given value for the purposes of
        /// representing this grid as a bitmap.
        /// The default behaviour is to treat the default value of the type as white,
        /// and everything else as black.
        /// </summary>
        /// <param name="val">The value for which a colour equivalent is required.
        /// </param>
        /// <returns>The colour corresponding to the given value for this grid.
        /// </returns>
        protected virtual Color GetColorEquivalent(T val)
        {
            return (val.Equals(default(T)) ? Color.White : Color.Black);
        }

        /// <summary>
        /// Converts this mask into a bitmap where all set pixels are represented by
        /// black and all clear pixels are represented by white
        /// </summary>
        /// <returns>A bitmap representation of this mask</returns>
        public Bitmap ToBitmap()
        {
            return ToBitmap(new Rectangle(0, 0, Width, Height), 1);
        }

        /// <summary>
        /// Converts this mask into a scaled bitmap where all set pixels are
        /// represented by black and all clear pixels are represented by white
        /// </summary>
        /// <param name="scale">The scale to use for the returned bitmap - 1 would
        /// give a direct 1x1 mapping of the pixels in this mask to the pixels in
        /// the returned bitmap</param>
        /// <returns>A bitmap representation of this mask</returns>
        /// <remarks>If subclasses need to override how bitmaps work, all of the
        /// <c>ToBitmap</c> methods call <see cref="ToBitmap(Rectangle,int)"/>,
        /// which is virtual - they can override that to, for instance, add a
        /// subclass-specific colour map parameter to the <see cref="ToBitmapCore"/>
        /// call.
        /// </remarks>
        public Bitmap ToBitmap(int scale)
        {
            return ToBitmap(new Rectangle(0, 0, Width, Height), scale);
        }

        /// <summary>
        /// Converts the specified area of this mask into a bitmap where all set
        /// pixels are represented by black and all clear pixels are represented by
        /// white
        /// </summary>
        /// <param name="r">The rectangular area of this mask which is required as
        /// a bitmap</param>
        /// <returns>A bitmap representation of the specified area of this mask
        /// </returns>
        /// <remarks>If subclasses need to override how bitmaps work, all of the
        /// <c>ToBitmap</c> methods call <see cref="ToBitmap(Rectangle,int)"/>,
        /// which is virtual - they can override that to, for instance, add a
        /// subclass-specific colour map parameter to the <see cref="ToBitmapCore"/>
        /// call.
        /// </remarks>
        public Bitmap ToBitmap(Rectangle r)
        {
            return ToBitmap(r, 1);
        }

        /// <summary>
        /// Converts the specified area of this mask into a bitmap where all set
        /// pixels are represented by black and all clear pixels are represented by
        /// white
        /// </summary>
        /// <param name="r">The rectangular area of this mask which is required as
        /// a bitmap</param>
        /// <param name="scale">The scale of the bitmap required - 1 is one pixel in
        /// the bitmap represents one pixel in this mask - 2 creates 4 pixels in the
        /// bitmap to represent a pixel in the mask, and so forth.</param>
        /// <returns>A bitmap representation of this mask</returns>
        protected virtual Bitmap ToBitmap(Rectangle r, int scale)
        {
            Dictionary<T, Color> colourMap = new Dictionary<T, Color>();

            // Create a bitmap of the required size.
            Bitmap bmp = new Bitmap(r.Width * scale, r.Height * scale,
                PixelFormat.Format32bppArgb);

            for (int x = 0; x < r.Width; x++)
            {
                for (int y = 0; y < r.Height; y++)
                {
                    // Get the value
                    T val = this[x + r.X, y + r.Y];

                    // Get the colour corresponding to that value - try the cache
                    // first; if not there get it from the method and cache it.
                    Color col;
                    if (!colourMap.TryGetValue(val, out col))
                    {
                        colourMap[val] = (col = GetColorEquivalent(val));
                    }

                    // Fill in the rectangle which the scale requires - ie.
                    // if scale is 2, fill in a 2x2 rectangle.
                    for (int i = 0; i < scale; i++)
                        for (int j = 0; j < scale; j++)
                            bmp.SetPixel(x + i, y + j, col);
                }
            }
            return bmp;
        }

        /// <summary>
        /// Gets a hash of this grid - just a function of width, height and the
        /// centre value of the grid.
        /// </summary>
        /// <returns>A hash value representing this mask</returns>
        public override int GetHashCode() => ((Width << 16) | Height) ^ _data[Width / 2, Height / 2].GetHashCode();

        /// <summary>
        /// Checks if this mask is equal to the given object. It is considered equal
        /// if it is a mask of the same size and content as this mask - ie. its
        /// pixels exactly match this mask.
        /// </summary>
        /// <param name="obj">The object to check for equality against</param>
        /// <returns>True if the given object is a mask with equal value to this one,
        /// false otherwise.</returns>
        public override bool Equals(object obj)
        {
            GridData<T> g = obj as GridData<T>;
            int w;
            int h;
            if (g == null || ((w = Width) != g.Width) || ((h = Height) != g.Height))
                return false;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!_data[x, y].Equals(g._data[x, y]))
                        return false;
                }
            }
            return true;
        }

        #endregion

    }
}
