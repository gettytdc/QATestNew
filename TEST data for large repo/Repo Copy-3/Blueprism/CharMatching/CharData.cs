using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;

using BluePrism.BPCoreLib;
using BluePrism.BPCoreLib.Collections;
using BluePrism.Server.Domain.Models;
using System.Diagnostics;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Class to encapsulate a single piece of character data - the glyph which
    /// recognises a character alongside the character that it represents.
    /// </summary>
    [DebuggerDisplay("CharData: {Value}")]
    public class CharData : clsDataMonitor, IComparable
    {
        #region - Operator overloads -

        /// <summary>
        /// Appends the character from the second CharData to the first CharData's
        /// character and returns the resultant string
        /// </summary>
        /// <param name="c1">The first char data object</param>
        /// <param name="c2">The char data object to append to c1</param>
        /// <returns>A string containing the 2 characters appended</returns>
        public static string operator +(CharData c1, CharData c2)
        {
            if (c1 == null && c2 == null)
                return "";
            if (c1 == null)
                return c2._val;
            if (c2 == null)
                return c1._val;
            return c1._val + c2._val;
        }

        /// <summary>
        /// Casts the given char data object into its char equivalent.
        /// </summary>
        /// <param name="c1">The char data object to cast</param>
        /// <returns>The char represented by the specified data object.</returns>
        public static explicit operator string(CharData c1)
        {
            if (c1 == null)
                return NullValue;
            return c1._val;
        }

        #endregion

        #region - Inner Classes / Enums -

        /// <summary>
        /// Various outcomes available after attempting to match a character pixel as
        /// part of font recognition. By recording such states we can build a picture
        /// of what has happened to be displayed as convenient debugging info.
        /// </summary>
        private enum MatchOutcome
        {
            /// <summary>
            /// Pixel is not relevant to this character match, because it lies out of
            /// the bounds of this character. This pixel happens to be white, but not
            /// that we care.
            /// </summary>
            IrrelevantWithWhiteUnderneath,
            /// <summary>
            /// Pixel is not relevant to this character match, because it lies out of
            /// the bounds of this character. This pixel happens to be black, but not
            /// that we care.
            /// </summary>
            IrrelevantWithBlackUnderneath,
            /// <summary>
            /// Pixel is not relevant to this character match, because it lies out of
            /// the bounds of this character. This pixel happens to be some deleted ink, 
            /// but not that we care.
            /// </summary>
            IrrelevantWithDeletedInkUnderneath,
            /// <summary>
            /// We failed to achieve a match with this pixel. The pixel is white, so we
            /// failed to match our black pixel against this one.
            /// </summary>
            NoMatchOnWhiteBase,
            /// <summary>
            /// We failed to achieve a match with this pixel. The pixel is black, so we
            /// failed to match our white pixel against this one.
            /// </summary>
            NoMatchOnBlackBase,
            /// <summary>
            /// Successful match of white against white
            /// </summary>
            MatchWhiteOnWhite,
            /// <summary>
            /// Successful match of black against black
            /// </summary>
            MatchBlackOnBlack
        }

        /// <summary>
        /// Class to represent an equality comparer for CharData objects which
        /// ignores the char set within the CharData and compares only based on the
        /// glyph.
        /// </summary>
        private class CharlessComparerImpl
            : IEqualityComparer<CharData>, IComparer<CharData>
        {
            /// <summary>
            /// True if the given char data objects are equal - ie. have the same
            /// mask and state data, regardless of their assigned chars.
            /// </summary>
            /// <param name="obj1">The first char data object to test</param>
            /// <param name="obj2">The second char data object to test</param>
            /// <returns></returns>
            public bool Equals(CharData obj1, CharData obj2)
            {
                return (obj1 == null ? obj2 == null : obj1.Equals(obj2, false));
            }

            /// <summary>
            /// Gets a hashcode, disregarding the assigned char, of the given
            /// CharData object.
            /// </summary>
            /// <param name="value">The data for which a hashcode is required</param>
            /// <returns>A hash of the given char data, taking into account its
            /// mask and state data, but not its assigned char</returns>
            public int GetHashCode(CharData value)
            {
                return (value == null ? 0 : value.GetHashCode(false));
            }

            /// <summary>
            /// Compares the given CharData object, determining if one is greater
            /// than, equal to or less than the other, disregarding the value which
            /// has been assigned to them.
            /// </summary>
            /// <param name="x">The first char data object to test</param>
            /// <param name="y">The second char data object to test</param>
            /// <returns>A negative value if x is 'less than' y; 0 if x is the same
            /// as y; a positive value if x is 'greater than' y. Note that for this
            /// class, the chars assigned to x and y are ignored, if present.
            /// </returns>
            public int Compare(CharData x, CharData y)
            {
                // If they are both null, they are considered equal
                if (x == null && y == null)
                    return 0;

                // If one of x or y is null but not the other, we consider
                // [not-null] to be greater than [null]
                if (x == null) return -1;
                if (y == null) return 1;

                // Both non-null, so we delegate to the CompareTo() method, ensuring
                // we instruct it to disregard the assigned char
                return x.CompareTo(y, false);
            }
        }

        /// <summary>
        /// Comparer which <em>only</em> takes into account the character assigned
        /// to the char data object.
        /// </summary>
        /// <remarks>On all methods implemented by this class, a null CharData object
        /// is treated as identical to a CharData object with no character assigned.
        /// </remarks>
        private class CharOnlyComparerImpl
            : IEqualityComparer<CharData>, IComparer<CharData>
        {
            /// <summary>
            /// Checks if the assigned characters on the given data objects are
            /// the same
            /// </summary>
            /// <param name="x">The first char data object to test</param>
            /// <param name="y">The second char data object to test</param>
            /// <returns>True if the given char data objects represent the same
            /// assigned character or if both chardata objects are null.</returns>
            public bool Equals(CharData x, CharData y)
            {
                // A CharData being null, or a CharData having no character assigned
                // is treated as the same thing in this class.
                return (
                    (x == null || !x.HasValue)
                    ? (y == null || !y.HasValue)
                    : x.Value == y.Value
                );
            }

            /// <summary>
            /// Gets the hashcode for the given CharData object, taking into account
            /// only the char which is assigned to it.
            /// </summary>
            /// <param name="obj">The CharData object for which the hash code is
            /// required</param>
            /// <returns>A hash function for the given CharData object which operates
            /// purely on the assigned character.</returns>
            public int GetHashCode(CharData obj)
            {
                int charHash = (obj == null
                    ? CharData.NullValue.GetHashCode() : obj.Value.GetHashCode());
                return 1017 ^ charHash;
            }

            /// <summary>
            /// Compares the assigned characters in the given char data objects.
            /// </summary>
            /// <param name="x">The first CharData object to compare the assigned
            /// char on.</param>
            /// <param name="y">The CharData object to compare the first object to.
            /// </param>
            /// <returns>A positive value, 0 or a negative value if the assigned char
            /// on <paramref name="x"/> is greater than, equal to or less than the
            /// assigned char on <paramref name="y"/>.</returns>
            /// <remarks>For the purposes of this comparison, a null CharData object
            /// is considered equal to a CharData object with no char assigned to it.
            /// </remarks>
            public int Compare(CharData x, CharData y)
            {
                string xval = (x != null ? x.Value : CharData.NullValue);
                string yval = (y != null ? y.Value : CharData.NullValue);
                return xval.CompareTo(yval);
            }

        }

        #endregion

        #region - Static Methods -

        /// <summary>
        /// Generates the checks to make for intrusions from a point from different
        /// directions.
        /// </summary>
        /// <returns>A map against direction of the points to check for intrusion in
        /// a sensible order in which they are checked.</returns>
        private static
            IDictionary<Direction, ICollection<Point>> GenerateOffsetChecks()
        {
            Dictionary<Direction, ICollection<Point>> map =
                new Dictionary<Direction, ICollection<Point>>();

            map[Direction.Left] = new Point[]{
                new Point(-1, -1), new Point(-1, 0), new Point(-1, 1),
                new Point(0, -1), new Point(0, 1),
                new Point(1, -1), new Point(1, 0), new Point(1, 1)
            };
            map[Direction.Right] = new Point[]{
                new Point(1, -1), new Point(1, 0), new Point(1, 1),
                new Point(0, -1), new Point(0, 1),
                new Point(-1, -1), new Point(-1, 0), new Point(-1, 1)
            };
            return GetReadOnly.IDictionary(map);
        }

        /// <summary>
        /// Scans the supplied image for characters, using vertical slicing, and
        /// using the second most dominant colour in the image as the foreground
        /// colour, on the assumption that the most dominant colour is the background
        /// colour, and the second most dominant is likely to be the foreground
        /// colour.
        /// </summary>
        /// <param name="img">The image to extract a collection of characters from.
        /// A null image will be treated as having no characters.</param>
        /// <returns>The collection of character extracted from the given image
        /// </returns>
        public static ICollection<CharData> Extract(Image img)
        {
            if (img == null)
                return GetEmpty.ICollection<CharData>();
            return Extract(img, ImageUtil.GetSecondDominantColour(img));
        }

        /// <summary>
        /// Scans the supplied image for characters, using vertical slicing.
        /// </summary>
        /// <param name="bmp">The image to scan.</param>
        /// <param name="fgColour">The active colour.</param>
        /// <returns>A collection of chardata objects representing the potential
        /// characters found.</returns>
        public static ICollection<CharData> Extract(Image img, Color fgColour)
        {
            return Extract(img, fgColour, false);
        }

        /// <summary>
        /// Scans the supplied image for characters, using vertical slicing.
        /// </summary>
        /// <param name="bmp">The image to scan.</param>
        /// <param name="fgColour">The active colour.</param>
        /// <returns>A collection of chardata objects representing the potential
        /// characters found.</returns>
        public static ICollection<CharData> Extract(
            Image img, Color fgColour, bool autoTrim)
        {
            if (img == null)
                return GetEmpty.ICollection<CharData>();

            Bitmap bmp = ImageUtil.GetMaskBmp(img, fgColour);
            Mask mask = new Mask(img, fgColour);

            int firstCharColumn = -1;
            int firstSpaceColumn = -1;
            int spaceWidth = 0;
            int black = Color.Black.ToArgb();
            IBPSet<CharData> chars = new clsSet<CharData>();

            for (int x = 0, w = bmp.Width; x < w; x++)
            {
                bool isSpaceColumn = true;
                for (int y = 0, h = bmp.Height; y < h; y++)
                {
                    if (mask[x, y])
                    {
                        //The left hand edge of a character has been found.
                        if (firstCharColumn == -1)
                            firstCharColumn = x;

                        isSpaceColumn = false;
                        break;
                    }
                }

                if (isSpaceColumn)
                {
                    if (firstCharColumn == -1)
                    {
                        // If there's nothing in the list, then this space column
                        // is to the left of the first character.
                        if (chars.Count != 0)
                            spaceWidth++;
                    }
                    else
                    {
                        // The left edge of a character has been found and 
                        // this space column is at the right hand edge.
                        if (spaceWidth > 0)
                        {
                            // There was a space on the left of this character.
                            // Count this column for the next character
                            spaceWidth = 1;
                        }

                        //Add the character
                        firstSpaceColumn = x;

                        // Clone that area of the bitmap
                        Mask m = mask.Clone(new Rectangle(firstCharColumn, 0,
                                firstSpaceColumn - firstCharColumn, bmp.Height));

                        // Trim the mask vertically before adding to the chars
                        if (autoTrim)
                            m = m.TrimVertical(1);
                        chars.Add(new CharData(m));

                        //Reset for the next column.
                        firstCharColumn = -1;
                    }
                }

            }
            return chars;
        }

        #endregion

        #region - Static Variables / Constants -

        /// <summary>
        /// A hash/equality tester for CharData objects which disregards the glyph
        /// assigned to the CharData object.
        /// </summary>
        public static readonly IEqualityComparer<CharData> CharlessEqualityComparer =
            new CharlessComparerImpl();

        /// <summary>
        /// A comparer for CharData objects which disregards the char assigned to the
        /// CharData object.
        /// </summary>
        public static readonly IComparer<CharData> CharlessComparer =
            new CharlessComparerImpl();

        /// <summary>
        /// A comparer for CharData objects which only takes into account the char
        /// which is assigned to the objects.
        /// </summary>
        public static readonly IComparer<CharData> CharOnlyComparer =
            new CharOnlyComparerImpl();

        /// <summary>
        /// Constant representing a null character. It's slightly more meaningful
        /// than the character literal
        /// </summary>
        public const string NullValue = "";

        /// <summary>
        /// A Map of offsets from a pixel to check for intrusions - the offsets are
        /// relative to the pixel being checked.
        /// </summary>
        private static readonly IDictionary<Direction, ICollection<Point>>
            _OffsetChecks = GenerateOffsetChecks();

        #endregion

        #region - Events -

        /// <summary>
        /// Event fired whenever the character represented by this object has changed
        /// </summary>
        public event TextValueChangedEventHandler CharChanged;

        /// <summary>
        /// Event fired whenever the mask which represents the structure of this
        /// object has changed
        /// </summary>
        public event EventHandler MaskChanged;

        #endregion

        #region - Instance Variables -

        /// <summary>
        /// A boolean mask representing the 'ink' pixels of the character image.
        /// </summary>
        private Mask _mask;

        /// <summary>
        /// The character that this font glyph represents.
        /// </summary>
        private string _val;

        /// <summary>
        /// The state of the pixels
        /// </summary>
        private PixelStateMask _stateMask;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new CharData object representing the given char, using the
        /// specified encoded masks
        /// </summary>
        /// <param name="val">The value represented by this CharData object.
        /// </param>
        /// <param name="encodedData">The encoded mask for this object</param>
        /// <param name="encodedStateMask">The encoded state mask for this object.
        /// </param>
        public CharData(string val, string encodedData, string encodedStateMask)
            : this(val, new Mask(encodedData), encodedStateMask) { }

        /// <summary>
        /// Creates a new CharData object using the given mask
        /// </summary>
        /// <param name="m">The mask providing the glyph for this CharData</param>
        public CharData(Mask m) : this(NullValue, m, null) { }

        /// <summary>
        /// Creates a new CharData object representing the given character with the
        /// specified mask
        /// </summary>
        /// <param name="val">The value that this object will represent</param>
        /// <param name="m">The mask describing the character glyph modelled by
        /// this object.</param>
        public CharData(string val, Mask m) : this(val, m, null) { }

        /// <summary>
        /// Creates a new CharData object using a mask extracted from the given
        /// bitmap
        /// </summary>
        /// <param name="bmp">The bitmap in which all black pixels describe the mask
        /// to be used in this object.</param>
        public CharData(Bitmap bmp)
            : this(NullValue, new Mask(bmp, Color.Black), null) { }

        /// <summary>
        /// Creates a new CharData object using a mask extracted from the given
        /// bitmap
        /// </summary>
        /// <param name="bmp">The bitmap in which all pixels of the specified
        /// foreground colour describe the mask to be used in this object.</param>
        /// <param name="fgColour">The colour of the pixels to use as the mask in the
        /// given bitmap</param>
        public CharData(Bitmap bmp, Color fgColour)
            : this(NullValue, new Mask(bmp, fgColour), null) { }

        /// <summary>
        /// Creates a new CharData object representing the specified character and
        /// using a mask extracted from the given bitmap
        /// </summary>
        /// <param name="val">The value that this object will represent</param>
        /// <param name="bmp">The bitmap in which all pixels of the specified
        /// foreground colour describe the mask to be used in this object.</param>
        /// <param name="fgColour">The colour of the pixels to use as the mask in the
        /// given bitmap</param>
        public CharData(string val, Bitmap bmp)
            : this(val, new Mask(bmp, Color.Black), null) { }

        /// <summary>
        /// Creates a new CharData object representing the specified character and
        /// using a mask extracted from the given bitmap
        /// </summary>
        /// <param name="val">The value that this object will represent</param>
        /// <param name="bmp">The bitmap in which all pixels of the specified
        /// foreground colour describe the mask to be used in this object.</param>
        /// <param name="fgColour">The colour of the pixels to use as the mask in the
        /// given bitmap</param>
        public CharData(string val, Bitmap bmp, Color fgColour)
            : this(val, new Mask(bmp, fgColour), null) { }

        /// <summary>
        /// Creates a new CharData object representing the specified character and
        /// using the given mask and encoded state mask
        /// </summary>
        /// <param name="val">The value that this object will represent</param>
        /// <param name="m">The mask describing the character glyph modelled by
        /// this object.</param>
        /// <param name="encodedStateMask">The encoded state mask for this object
        /// or null to create an empty state mask.</param>
        private CharData(string val, Mask m, string encodedStateMask)
        {
            _val = val;
            _mask = m;
            _stateMask = new PixelStateMask(Width, Height, encodedStateMask);
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets the state mask encoded as a string.
        /// </summary>
        /// <returns>Returns a sequence of boolean pixel values, represented as ones
        /// and zeros. There is precisely
        /// one value per pixel. The sequence of values is 'across' then down, row by
        /// row. Eg 000111000 corresponds to a matrix like this:
        ///   000
        ///   111
        ///   000
        /// </returns>
        public string EncodedMask { get { return _mask.Encoded; } }

        /// <summary>
        /// Gets the state mask encoded as a string.
        /// </summary>
        /// <returns>Returns a comma-separated sequence of state values. There is
        /// precisely one value per pixel. The sequence of values is 'across' then
        /// down, row by row. Eg 0,0,0,1,1,1,0,0,0 corresponds to a matrix like this:
        ///   000
        ///   111
        ///   000
        /// </returns>
        public string EncodedStateMask { get { return _stateMask.Encoded; } }

        /// <summary>
        /// The mask representing the image of this character
        /// </summary>
        public Mask Mask
        {
            get { return _mask; }
            set { _mask = value; OnMaskChanged(EventArgs.Empty); }
        }

        /// <summary>
        /// An array of "states", determining the behaviour, type or function of 
        /// various pixels. Each pixel defaults to None.
        /// </summary>
        public PixelStateMask StateMask
        {
            get { return _stateMask; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (value.Width != Width || value.Height != Height)
                    throw new ArgumentException(Resources.SizeOfTheStateMaskMustMatchTheSizeOfThisCharacterSGlyph);

                _stateMask = value;
            }
        }

        /// <summary>
        /// Flag indicating if a value is set in this character data object or not
        /// </summary>
        public bool HasValue
        {
            get { return (_val != NullValue); }
        }

        /// <summary>
        /// The textual value represented by this char data, or
        /// <see cref="NullValue"/> if the value is not specifically set.
        /// </summary>
        public string Value
        {
            get { return (_val ?? NullValue); }
            set
            {
                if (_val != value)
                {
                    string oldVal = _val;
                    _val = value;
                    OnTextValueChanged(new TextValueChangedEventArgs(oldVal, value));
                }
            }
        }

        /// <summary>
        /// The character represented by this char data in a string.
        /// </summary>
        public string CharacterString
        {
            get { return (_val ?? ""); }
            set { _val = (string.IsNullOrEmpty(value) ? NullValue : value); }
        }

        /// <summary>
        /// The height of the character glyph represented by this object.
        /// </summary>
        public int Height { get { return _mask.Height; } }

        /// <summary>
        /// The width of the character glyph represented by this object.
        /// </summary>
        public int Width { get { return _mask.Width; } }

        /// <summary>
        /// The size of the character glyph represented by this object.
        /// </summary>
        public Size Size { get { return _mask.Size; } }

        #endregion

        #region - Methods -

        /// <summary>
        /// Raises the <see cref="CharChanged"/> event for this char data.
        /// </summary>
        protected virtual void OnTextValueChanged(TextValueChangedEventArgs e)
        {
            if (CharChanged != null)
                CharChanged(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MaskChanged"/> event for this char data.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMaskChanged(EventArgs e)
        {
            if (MaskChanged != null)
                MaskChanged(this, e);
        }

        /// <summary>
        /// Clears the character being held in this char data object.
        /// </summary>
        public void ClearCharacter()
        {
            _val = NullValue;
        }

        /// <summary>
        /// Gets the true height of the character - that is, the height from its
        /// lowest 'black' pixel, to the highest 'black pixel'. This ignores white
        /// space padding the character above and below these two points.
        /// </summary>
        /// <returns>Returns the true height of the character, or zero if the
        /// character is all white.</returns>
        public int GetTrueHeight()
        {
            int h = Height;
            int above = _mask.CountEmptyLines(Direction.Top);
            if (above == h) // ie. all lines are empty...
                return 0;

            int below = _mask.CountEmptyLines(Direction.Bottom);
            return h - above - below;
        }

        /// <summary>
        /// Vertically autotrims this character such that it contains all the rows
        /// which currently contain ink, plus <paramref name="padding"/> rows on
        /// both top and bottom edges of empty rows.
        /// </summary>
        /// <param name="padding">The number of empty rows to leave on both the top
        /// and bottom edge of the character after trimming it.</param>
        public void TrimVertical(int padding)
        {
            _mask.TrimVertical(padding);
            _stateMask.TrimVertical(padding);
            OnMaskChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Strips lines from this character from the given direction by the given
        /// amount.
        /// </summary>
        /// <param name="dirn">The direction to strip lines from</param>
        /// <param name="amount">The number of lines to strip</param>
        public void Strip(Direction dirn, int amount)
        {
            _mask = _mask.Strip(dirn, amount);
            _stateMask = _stateMask.Strip(dirn, amount);
            OnMaskChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Shifts this character to the given direction by the given amount.
        /// </summary>
        /// <param name="dirn">The direction to shift</param>
        /// <param name="amount">The number of lines to shift</param>
        public void Shift(Direction dirn, int amount)
        {
            _mask = _mask.Shift(dirn, amount);
            _stateMask = _stateMask.Shift(dirn, amount);
            OnMaskChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Pads this character in the given direction by the given amount.
        /// </summary>
        /// <param name="dirn">The direction to pad</param>
        /// <param name="amount">The number of lines to pad</param>
        public void Pad(Direction dirn, int amount)
        {
            _mask = _mask.Pad(dirn, amount);
            _stateMask = _stateMask.Pad(dirn, amount);
            OnMaskChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Determines whether this character is found at the specified position on
        /// the supplied canvas.
        /// </summary>
        /// <param name="offset">The offset from which to start analysing the canvas.
        /// </param>
        /// <param name="canvas">The canvas on which to test for this character.
        /// </param>
        /// <param name="nonStrictMatch">Relevant only when the Strict parameter is
        /// true. Indicates, in the event of a match (ie
        /// return value of true), whether the match was made on a non-strict basis.
        /// That is, whether the return value would have been false if the Strict
        /// parameter were false.</param>
        /// <param name="strictLeft">Set to false to allow non-strict matching on the
        /// left hand side, on the basis of "no-check" attributes in the character
        /// mask.</param>
        /// <param name="strictRight">Set to false to allow non-strict matching on
        /// the right hand side, on the basis of "no-check" attributes in the
        /// character mask.</param>
        /// <returns>true if this character exists at the specified position;
        /// false otherwise.</returns>
        public bool IsAtPosition(Point offset, CharCanvas canvas,
            out bool nonStrictMatch, bool strictLeft, bool strictRight)
        {

            //Developer debug option - set this to true to collect information
            //about how matching has taken place. You can then save this as a bitmap
            //and inspect it. PW would like to turn this into a graphical forms-based
            //debugging utitlity in the future.
            MatchOutcome[,] Outcomes = null;
            bool Debug = false;
            if (Debug)
            {
                Outcomes = GetBlankOutcomes(canvas);
            }

            nonStrictMatch = false;
            bool Retval = true;

            if (offset.X + Width > canvas.Width)
                return false;
            if (offset.Y + Height > canvas.Height)
                return false;

            int delCount = 0;
            int realSpaceCount = 0;
            int realInkCount = 0;
            int invasionCount = 0;

            int x = 0;
            int y = 0;
            for (y = 0; y < Height; y++)
            {
                int firstInkCol = _mask.GetColumnOfFirstNonEmptyCell(y);
                int lastInkCol = _mask.GetColumnOfLastNonEmptyCell(y);

                for (x = 0; x < Width; x++)
                {
                    // The 'centre' left pixel in this row - all pixels up to and
                    // including this one are considered on the left of the char;
                    // everything after it is not (either interior or right)
                    int centreLeft = (int)Math.Round((double)Width / 2.0);

                    bool InInterior = firstInkCol > -1 && lastInkCol > -1 && firstInkCol < x && x < lastInkCol;
                    bool IsOnLeft = (firstInkCol == -1 && x <= centreLeft) || x <= firstInkCol;
                    bool IsOnRight = !(IsOnLeft || InInterior);

                    bool masked = _mask[x, y];
                    CharCanvasState canvasState = canvas[x + offset.X, y + offset.Y];

                    bool inkOnInk = (masked && canvasState == CharCanvasState.InkExists);
                    bool spaceOnSpace = (!masked && canvasState == CharCanvasState.NoInkExists);
                    bool inkOnDeletedInk = (masked && canvasState == CharCanvasState.DeletedInkExists);
                    bool spaceOnDeletedInk = (!masked && canvasState == CharCanvasState.DeletedInkExists);

                    bool foundMatch = false;
                    if (inkOnInk)
                    {
                        //Fully legitimate match
                        foundMatch = true;
                        realInkCount++;
                    }
                    else if (spaceOnSpace)
                    {
                        //Fully legitimate match
                        foundMatch = true;
                        realSpaceCount++;
                    }
                    else if (IsOnLeft && spaceOnDeletedInk)
                    {
                        //Also legitimate
                        foundMatch = true;
                    }
                    else if (IsOnLeft && inkOnDeletedInk)
                    {
                        //May be due to overlap of characters. Such overlaps would only take place
                        //where we match on deleted ink, we would also expect this character's 
                        //mask to allow flexibility, so we check this also
                        if (this.StateMask[x, y] == PixelState.NoCheck)
                        {
                            //We don't allow too many overlaps as a ratio of
                            //real matches, so make a count of these
                            delCount++;
                            foundMatch = true;
                        }
                    }

                    if (foundMatch)
                    {
                        if (Debug)
                        {
                            Outcomes[x + offset.X, y + offset.Y] = (
                                _mask[x, y]
                                ? MatchOutcome.MatchBlackOnBlack
                                : MatchOutcome.MatchWhiteOnWhite
                            );
                        }
                    }
                    else
                    {
                        // Have we tried to assert a black pixel over a white one
                        bool MatchedInkOnNoInk = _mask[x, y];

                        // Have we tried to assert a white pixel over a black one
                        bool MatchedNoInkOnInk = !MatchedInkOnNoInk;
                        
                        if (Debug)
                        {
                            Outcomes[x + offset.X, y + offset.Y] = (
                                MatchedInkOnNoInk
                                ? MatchOutcome.NoMatchOnWhiteBase
                                : MatchOutcome.NoMatchOnBlackBase
                            );
                        }

                        if (InInterior)
                        {
                            //no fuzzy matching on interior of character
                            if (Debug)
                            {
                                Retval = false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (IsOnLeft && strictLeft)
                        {
                            if (Debug)
                            {
                                Retval = false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (IsOnLeft && !strictLeft)
                        {
                            //If on the left then we expect the ink to correspond to an already
                            //matched (and therefore already deleted) character
                            if (MatchedNoInkOnInk && spaceOnDeletedInk && _stateMask[x, y] == PixelState.NoCheck)
                            {
                                invasionCount++;
                            }
                            else
                            {
                                //There is the odd occasion when an item can poke from the
                                //right, such that it appears on the left for a slim character.
                                //Eg "\" appears as a vertical line in the Arial 10 Italic font. The lower
                                //left portion of "]" can poke underneath this line in the "\]" combination.
                                //Alternatively a long "y" can loop right underneath over to the left
                                // In such cases we cannot expect
                                //the ink to have been deleted, despite appearing on the left.
                                // We check the pixels just before it instead. If this is blank then we
                                //presume the black pixel is being poked from the right.
                                if (PixelIntrudesFromRight(canvas, offset, new Point(x, y)))
                                {
                                    invasionCount++;
                                }
                                else
                                {
                                    //The point at which the above logic fails, is if a character
                                    //pokes into the space of this character entirely, such as an
                                    //italic g followed by a full stop - the full stop can hug the g tightly
                                    //such that it is fully contained in this character's space
                                    //TODO - how to fix this?
                                    if (Debug)
                                    {
                                        Retval = false;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        else if (IsOnRight && strictRight)
                        {
                            if (Debug)
                            {
                                Retval = false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (IsOnRight && !strictRight)
                        {
                            if (MatchedNoInkOnInk && _stateMask[x, y] == PixelState.NoCheck)
                            {
                                if (PixelIntrudesFromRight(canvas, offset, new Point(x, y)))
                                {
                                    invasionCount++;
                                }
                                else
                                {
                                    //The point at which the above "intrusion" logic fails, is if a character
                                    //pokes into the space of this character entirely, such as an
                                    //italic g followed by a full stop - the full stop can hug the g tightly
                                    //such that it is fully contained in this character's space
                                    //TODO - how to fix this?
                                    if (Debug)
                                    {
                                        Retval = false;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                if (Debug)
                                {
                                    Retval = false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            nonStrictMatch = invasionCount > 0;

            int realMatchCount = realInkCount + realSpaceCount;

            bool tooManyDeleted =
                realInkCount == 0 ||
                (delCount > 0) &&
                (delCount / (delCount + realMatchCount) > 0.15);

            bool tooManyInvasions = (invasionCount > 0) &&
                (invasionCount / (realMatchCount + invasionCount) > 0.25);

            return Retval && !(tooManyDeleted || tooManyInvasions);
        }


        /// <summary>
        /// Determines whether a pixel belongs to a trail which can be traced
        /// to the outside of this character, on the right.
        /// </summary>
        /// <param name="canvas">The canvas under inspection. A trail of pixels will
        /// be followed from within this character to any available point outside of
        /// this character on the canvas.</param>
        /// <param name="offset">The offset on the canvas at which this character is
        /// presumed to reside.</param>
        /// <param name="pixel">The starting point of the trail to be followed.
        /// </param>
        /// <returns>Returns true if a trail leads outside of this character; false
        /// otherwise.</returns>
        private bool PixelIntrudesFromRight(
            CharCanvas canvas, Point offset, Point pixel)
        {
            return PixelIntrudesFromSide(Direction.Right, canvas, offset, pixel);
        }

        /// <summary>
        /// Determines whether a pixel belongs to a trail which can be traced
        /// to the outside of this character, on the left.
        /// </summary>
        /// <param name="Canvas">The canvas under inspection. A trail of pixels will
        /// be followed from within this character to any available point outside of
        /// this character on the canvas.</param>
        /// <param name="Offset">The offset on the canvas at which this character is
        /// presumed to reside.</param>
        /// <param name="pixel">The starting point of the trail to be followed.</param>
        /// <returns>Returns true if a trail leads outside of this character; false
        /// otherwise.</returns>
        private bool PixelIntrudesFromLeft(
            CharCanvas canvas, Point offset, Point pixel)
        {
            return PixelIntrudesFromSide(Direction.Left, canvas, offset, pixel);
        }

        /// <summary>
        /// Determines whether a pixel belongs to a trail which can be traced
        /// to the outside of this character, on the indicated side.
        /// </summary>
        /// <param name="dirn">The direction in which to follow the trail.</param>
        /// <param name="canvas">The canvas under inspection. A trail of pixels will
        /// be followed from within this character to any available point outside of
        /// this character on the canvas.</param>
        /// <param name="offset">The offset on the canvas at which this character is
        /// presumed to reside.</param>
        /// <param name="pixel">The starting point of the trail to be followed.
        /// </param>
        /// <param name="history">A list of pixels already visited, which
        /// should therefore be ignored. An empty list is recommended to begin with.
        /// </param>
        /// <returns>Returns true if a trail leads outside of this character; false
        /// otherwise.</returns>
        private bool PixelIntrudesFromSide(
            Direction dirn, CharCanvas canvas, Point offset, Point pixel)
        {
            return PixelIntrudesFromSide(
                dirn, canvas, offset, pixel, new clsSet<Point>());
        }

        /// <summary>
        /// Recursive method which determines whether a pixel belongs to a trail
        /// which can be traced to the outside of this character, on the indicated
        /// side.
        /// </summary>
        /// <param name="dirn">The direction in which to follow the trail.</param>
        /// <param name="canvas">The canvas under inspection. A trail of pixels will
        /// be followed from within this character to any available point outside of
        /// this character on the canvas.</param>
        /// <param name="offset">The offset on the canvas at which this character is
        /// presumed to reside.</param>
        /// <param name="pixel">The starting point of the trail to be followed.
        /// </param>
        /// <param name="history">A collection of pixels already visited, which
        /// should therefore be ignored.</param>
        /// <returns>Returns true if a trail leads outside of this character; false
        /// otherwise.</returns>
        private bool PixelIntrudesFromSide(Direction dirn, CharCanvas canvas,
            Point offset, Point pixel, IBPSet<Point> history)
        {
            Debug.Assert(dirn == Direction.Left || dirn == Direction.Right);

            // Motivation:
            //
            // It could be that a character is poking from the right into some
            // NoCheck space. We verify this by looking for connected pixels to its
            // right. Such pixels are assumed not to belong to this character, so we
            // check this also.
            //
            // Where there is no pixel immediately to the right then we
            // have to think harder. If there is another black pixel poking in from
            // above or below, which in turn can be validated by find other pixels
            // to the right then this is also ok - think how a "d" might
            // poke into "/" and still be clear to the right in the middle
            // of its round section. The items at the top/bottom of the round
            // section would be validated instead.

            // Add the pixel to the history - if the add fails, the pixel's already
            // there and has thus already been checked.
            if (!history.Add(pixel))
                return false;

            // Get the canvas-relative location of the pixel
            Point canvasPixel = pixel;
            canvasPixel.Offset(offset);

            // Validate the assumpion that this is a blank chr pixel with black
            // ink on the canvas, and a NoCheck mask in place
            if (canvas[canvasPixel] != CharCanvasState.InkExists ||
                _mask[pixel] || _stateMask[pixel] != PixelState.NoCheck)
                return false;

            foreach (Point creepOffset in _OffsetChecks[dirn])
            {
                // Get the pixel we want to test - locally (ie. a char co-ord)
                Point local = pixel;
                local.Offset(creepOffset);

                // If we're vertically outside the scope of this char, move on -
                // we're looking for horizontal escape from the char, not vertical
                if (local.Y < 0 || local.Y >= Height)
                    continue;

                // And globally, ie. on the canvas
                Point global = local;
                global.Offset(offset);

                // if it's outside the canvas, move onto the next offset
                if (!canvas.Contains(global))
                    continue;

                // if it's within the canvas but there is no ink there, move on.
                if (canvas[global] != CharCanvasState.InkExists)
                    continue;

                // So, it's within the canvas and there's ink there.
                // If we're outside the (horizontal) bounds of this character, then
                // we've successfully linked to an ink pixel outside of this char.
                // Return success.
                if (local.X < 0 || local.X >= Width)
                    return true;

                // It's an ink pixel on the canvas, and it's still within this char
                // If it's ink in this char too, we ignore it - this pixel can't
                // possibly be an intrusion from another char.
                if (_mask[local])
                    continue;

                // So we have linked to another black pixel not belonging to this
                // character, which may in turn lead outside of the character on
                // the desired side
                if (PixelIntrudesFromSide(dirn, canvas, offset, local, history))
                    return true;

            }

            //Haven't managed to creep out
            return false;
        }

        /// <summary>
        /// Gets debug information on a canvas, initialising all match outcomes in
        /// the grid to be irrelevant until marked otherwise.
        /// </summary>
        /// <param name="canvas">The canvas for which the blank outcome grid is
        /// required.</param>
        /// <returns>A grid of match outcomes with the same dimensions as the given
        /// canvas with the outcomes all set to irrelevant (with substate set from
        /// the given canvas values)</returns>
        private MatchOutcome[,] GetBlankOutcomes(CharCanvas canvas)
        {
            MatchOutcome[,] Outcomes = new MatchOutcome[canvas.Width, canvas.Height];
            for (int x = 0; x <= canvas.Width - 1; x++)
            {
                for (int y = 0; y <= canvas.Height - 1; y++)
                {
                    switch (canvas[x, y])
                    {
                        case CharCanvasState.InkExists:
                            Outcomes[x, y] = MatchOutcome.IrrelevantWithBlackUnderneath;
                            break;
                        case CharCanvasState.NoInkExists:
                            Outcomes[x, y] = MatchOutcome.IrrelevantWithWhiteUnderneath;
                            break;
                        case CharCanvasState.DeletedInkExists:
                            Outcomes[x, y] = MatchOutcome.IrrelevantWithDeletedInkUnderneath;
                            break;
                    }
                }
            }
            return Outcomes;
        }

        /// <summary>
        /// Checks to see whether the supplied character fits inside this one,
        /// in the sense that every black pixel in the supplied character maps
        /// onto a black pixel within this one. Eg in some fonts the 1 will fit
        /// inside the I.
        /// </summary>
        /// <param name="Ch">The supposed sub-character to check.</param>
        /// <returns>Returns true if the supplied character fits inside this
        /// one.</returns>
        public bool ContainsChar(CharData c)
        {
            return _mask.Contains(c._mask);
        }

        /// <summary>
        /// Deletes this character from the supplied canvas, at the specified position.
        /// Black ink will be replaced by deleted ink.
        /// </summary>
        /// <param name="offset">The offset on the canvas at which this character
        /// resides.</param>
        /// <param name="canvas">The canvas to be manipulated.</param>
        /// <remarks>It is the caller's responsibility to check that this is an
        /// appropriate thing to do - ie that the character really resides here.
        /// </remarks>
        public ICollection<Point> GetCanvasPointsOfCharInk(
            Point offset, CharCanvas canvas)
        {
            int minHeight = Math.Min(Height, canvas.Height - offset.Y);
            int minWidth = Math.Min(Width, canvas.Width - offset.X);
            ICollection<Point> inkPoints = new List<Point>();

            for (int y = 0; y < minHeight; y++)
            {
                for (int x = 0; x < minWidth; x++)
                {
                    Point p = new Point(x, y);
                    if (_mask[p])
                    {
                        p.Offset(offset);
                        inkPoints.Add(p);
                    }
                }
            }
            return inkPoints;
        }

        /// <summary>
        /// Gets a bitmap representing this character
        /// </summary>
        /// <returns>A bitmap illustrating this character</returns>
        public Bitmap ToBitmap()
        {
            return ToBitmap(1);
        }

        /// <summary>
        /// Builds a bitmap which illustrates this character.
        /// </summary>
        /// <param name="scale">The scale for the bitmap - 1 means a pixel in the
        /// bitmap is the same as a pixel in the font - higher numbers make it
        /// bigger.</param>
        /// <returns>Returns a bitmap representing this character.</returns>
        /// <remarks>Can be useful during debug.</remarks>
        public Bitmap ToBitmap(int scale)
        {
            Bitmap b = new Bitmap(Width * scale, Height * scale);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Color c;
                    if (_mask[x, y])
                    {
                        if (StateMask[x, y] == PixelState.NoCheck)
                        {
                            c = Color.DarkSlateGray;
                        }
                        else
                        {
                            c = Color.Black;
                        }
                    }
                    else
                    {
                        if (StateMask[x, y] == PixelState.NoCheck)
                        {
                            c = Color.LightGray;
                        }
                        else
                        {
                            c = Color.White;
                        }
                    }
                    for (int i = 0; i < scale; i++)
                    {
                        for (int j = 0; j < scale; j++)
                        {
                            b.SetPixel(x * scale + i, y * scale + j, c);
                        }
                    }
                }
            }
            return b;
        }

        /// <summary>
        /// Produces a bitmap showing debug information about match outcomes for this character.
        /// </summary>
        /// <param name="Outcomes">The match outcomes to be printed to a bitmap.</param>
        /// <param name="CharacterMask">The character mask of the character relating to the 
        /// match attempt. This will be overlaid on the outcomes picture.</param>
        /// <param name="Offset">The offset at which to begin drawing from within the outcomes
        /// array.</param>
        /// <param name="S">The size of bitmap desired.</param>
        /// <returns>Returns a bitmap displaying the supplied match outcomes information.</returns>
        private Bitmap BitmapFromMatchOutcomes(MatchOutcome[,] Outcomes, PixelState[,] CharacterMask, Point Offset, Size S)
        {
            Bitmap b = new Bitmap(S.Width, S.Height);
            for (int i = Offset.X; i <= Offset.X + S.Width - 1; i++)
            {
                for (int j = Offset.Y; j <= Offset.Y + S.Height - 1; j++)
                {
                    PixelState MaskValue = PixelState.None;
                    if (i - Offset.X < CharacterMask.GetLength(0) && j - Offset.Y < CharacterMask.GetLength(1))
                        MaskValue = CharacterMask[i - Offset.X, j - Offset.Y];
                    b.SetPixel(i - Offset.X, j - Offset.Y, GetOutcomeColour(Outcomes[i, j], MaskValue));
                }
            }
            return b;
        }

        /// <summary>
        /// Associates a colour with each match outcome and character state mask combination.
        /// </summary>
        /// <param name="Outcome">The outcome of interest.</param>
        /// <param name="MaskValue">The state mask value of interest.</param>
        /// <returns>Returns a colour, appropriate for the supplied outcome/state.</returns>
        private Color GetOutcomeColour(MatchOutcome Outcome, PixelState MaskValue)
        {
            switch (Outcome)
            {
                case MatchOutcome.IrrelevantWithBlackUnderneath:
                    return Color.White;
                case MatchOutcome.IrrelevantWithWhiteUnderneath:
                    return Color.White;
                case MatchOutcome.IrrelevantWithDeletedInkUnderneath:
                    return Color.SlateGray;
                case MatchOutcome.MatchWhiteOnWhite:
                    if (MaskValue == PixelState.None)
                    {
                        return Color.LightGreen;
                    }
                    else if (MaskValue == PixelState.NoCheck)
                    {
                        return Color.LightGray;
                    }
                    break;
                case MatchOutcome.MatchBlackOnBlack:
                    if (MaskValue == PixelState.None)
                    {
                        return Color.DarkGreen;
                    }
                    else if (MaskValue == PixelState.NoCheck)
                    {
                        return Color.SlateGray;
                    }
                    break;
                case MatchOutcome.NoMatchOnWhiteBase:
                    if (MaskValue == PixelState.None)
                    {
                        return Color.Red;
                    }
                    else if (MaskValue == PixelState.NoCheck)
                    {
                        return Color.DarkRed;
                    }
                    break;
                case MatchOutcome.NoMatchOnBlackBase:
                    if (MaskValue == PixelState.None)
                    {
                        return Color.Blue;
                    }
                    else if (MaskValue == PixelState.NoCheck)
                    {
                        return Color.DarkBlue;
                    }
                    break;
                default:
                    throw new InvalidArgumentException(Resources.OutcomeNotRecognised);
            }
            return Color.Pink;
        }

        /// <summary>
        /// Checks if this font character represents a character identical to the
        /// given one.
        /// </summary>
        /// <param name="c">The font character to check</param>
        /// <returns>True if the given character is not null and has a mask and
        /// pixel state mask which matches this char data object.
        /// </returns>
        public bool IsEquivalentChar(CharData c)
        {
            return (c != null &&
                c._mask.Equals(_mask) && c._stateMask.Equals(_stateMask));
        }

        /// <summary>
        /// Compares this character data object to the given object.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>A positive integer, zero or a negative integer if this data
        /// object is greater than, equal to or less than the given data object
        /// respectively.</returns>
        /// <exception cref="InvalidCastException">If the given object is not an
        /// instance of CharData.</exception>
        public int CompareTo(object obj)
        {
            return CompareTo(obj, true);
        }

        /// <summary>
        /// Compares this character data object to the given object.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <param name="checkAssignedChar">true to check the char value assigned to
        /// the CharData objects when comparing; false to disregard the char value
        /// and base the comparison on the glyph only.</param>
        /// <returns>A positive integer, zero or a negative integer if this data
        /// object is greater than, equal to or less than the given data object
        /// respectively.</returns>
        /// <exception cref="InvalidCastException">If the given object is not an
        /// instance of CharData.</exception>
        public int CompareTo(object obj, bool checkAssignedChar)
        {
            CharData c = (CharData)obj;

            // Are the bitmap masks identical?
            // If so, return the character comparison value for consistency
            if (_mask.Equals(c._mask))
                return (checkAssignedChar ? Value.CompareTo(c.Value) : 0);

            // So the mask is not equal - we want the 'larger' character to appear
            // earlier in the sort order.
            // 'Larger' is somewhat nebulous though...

            // First check count of 'non-empty' pixels. Subtract this way around to
            // ensure 'larger' is before 'smaller' - ie. 'larger' < 'smaller'...
            int emptyDiff = c.Mask.NonEmptyCount - Mask.NonEmptyCount;
            if (emptyDiff != 0)
                return emptyDiff / Math.Abs(emptyDiff); // Normalise to 1/-1

            int widthDiff = c.Width - Width;
            if (widthDiff != 0)
                return widthDiff / Math.Abs(widthDiff);

            // So far they look equal. Does one fit inside the other? If
            // so then we would prefer the "larger" character to appear
            // earlier in the sort order
            if (Height == c.Height)
            {
                if (ContainsChar(c))
                    return -1;
                if (c.ContainsChar(this))
                    return 1;
            }

            // Otherwise make a judgement based on the apparent "true" height
            // Again, we're comparing in reverse here to ensure 'larger' < 'smaller'
            int val = c.GetTrueHeight().CompareTo(GetTrueHeight());
            if (val != 0)
                return val;
            // If, after all that, it's considered equal (same width, same 'true'
            // height, so... what? Different 'false' height?), use the characters
            // that this data object represents as a final arbiter of order.
            // For chars we want 'normal' order eg. 'a' < 'b' < 'c' etc.
            return (checkAssignedChar ? Value.CompareTo(c.Value) : 0);
        }

        /// <summary>
        /// Gets the hashcode for this chardata
        /// </summary>
        /// <returns>A hash of the data held in this char data.</returns>
        public override int GetHashCode()
        {
            return GetHashCode(true);
        }

        /// <summary>
        /// Gets the hashcode for this chardata, including or excluding the char
        /// set within it as specified.
        /// </summary>
        /// <param name="includeVal">True to include the char value in this data
        /// when generating the hash code.</param>
        /// <returns>A hash of the data held in this char data.</returns>
        public int GetHashCode(bool includeVal)
        {
            return (_mask.GetHashCode() << 16) ^ _stateMask.GetHashCode() ^
                (includeVal ? _val.GetHashCode() << 8 : 0);
        }

        /// <summary>
        /// Checks if this char data object is equal to the given object
        /// </summary>
        /// <param name="obj">The object to test against this for equality.</param>
        /// <returns>True if the given object is a non-null CharData object with the
        /// same character, mask and state mask as this object.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj, true);
        }

        /// <summary>
        /// Checks if this char data object is equal to the given object, including
        /// or disregarding the character assigned to it as specified.
        /// </summary>
        /// <param name="obj">The object to test against this for equality.</param>
        /// <param name="includeVal">True to test the values set in the
        /// CharData objects, false to disregard them</param>
        /// <returns>True if the given object is a non-null CharData object with the
        /// same character (if <paramref name="includeVal"/> is set), mask and state
        /// mask as this object.</returns>
        public bool Equals(object obj, bool includeVal)
        {
            CharData cd = obj as CharData;

            // Null, different masks and (possibly) the assigned char must be the
            // same for equality
            if (cd == null || (includeVal && _val != cd._val) ||
                !_mask.Equals(cd._mask) || !_stateMask.Equals(cd._stateMask))
                return false;

            // That's everything - must be equal
            return true;
        }

        #endregion
    }
}
