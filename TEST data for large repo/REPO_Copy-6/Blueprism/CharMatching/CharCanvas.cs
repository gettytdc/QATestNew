using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// The different states of a canvas location.
    /// </summary>
    public enum CharCanvasState
    {
        NoInkExists,
        InkExists,
        DeletedInkExists
    }

    /// <summary>
    /// Represents a canvas of pixels to be inspected for font recognition.
    /// </summary>
    public class CharCanvas : GridData<CharCanvasState>
    {
        #region - Class-scope Declarations -

        /// <summary>
        /// Removes horizontal bands/blocks of colour from the supplied pixrect, which
        /// do not match the specified background colour.
        /// </summary>
        /// <param name="PR">The pixrect of interest. This will not be modified.</param>
        /// <param name="iBackgroundColour">The background colour of interest. Blocks
        /// of colour not matching this value will be removed.</param>
        /// <returns>Returns a new pixrect which corresponds to the original, with
        /// blocks of colour removed.</returns>
        public static clsPixRect RemoveEnclosingBlocksFromPixRect(clsPixRect pr, int bgCol)
        {
            pr = pr.To16ColourPixRect(bgCol, bgCol);

            //Scan each row. Where there is a consecutive sequence of colours
            //different from the standard background colour, and where the sequence
            //exceeds the threshold height, remove this colour portion
            int start = 0;
            int end = 0;
            int loopCount = 0;
            do
            {
                int[] colours = pr.GetModalColoursByRow();
                int currColour = -1;
                start = -1;
                end = -1;

                for (int i = 0; i < colours.Length; i++)
                {
                    int iCol = colours[i];
                    if (start == -1)
                    {
                        if (iCol != bgCol)
                        {
                            start = i;
                            currColour = iCol;
                        }
                    }
                    else if (iCol != currColour)
                    {
                        end = i - 1;
                        break;
                    }
                }
                if (end == -1)
                    end = colours.Length - 1;

                if (start > -1)
                {
                    //We now have a block of colour from StartRow to EndRow
                    //which needs to be cleansed. We convert everything in
                    //this coloured block, not equal to the colour we are
                    //deleting (presumably text), to some new neutral colour
                    //The coloured block itself is changed to match the background
                    int neutral = 0x0;
                    // = Black
                    if (bgCol == neutral)
                    {
                        if (currColour != 0xff0000)
                            neutral = 0xff0000; // Red
                        else
                            neutral = 0x0000ff; // Blue
                    }

                    pr.EraseColourInRows(start, end, currColour, bgCol, neutral);
                }

                //We shouldn't need this, but just in case
                if (++loopCount > 100)
                {
                    throw new InvalidOperationException(
                        Resources.ErrorTooManyLoopIterationsInFunctionRemoveEnclosingBlocksFromPixRect);
                }
            } while (start > -1);

            return pr;
        }

        /// <summary>
        /// Creates a new canvas from the supplied pixrect object.
        /// </summary>
        /// <param name="pr">The pixrect from which to create the canvas.
        /// The pixrect will not be modified.</param>
        /// <param name="col">The colour of interest to be matched on
        /// the pixrect.</param>
        /// <returns>Returns a new canvas, corresponding to the incidence of the
        /// colour of interest on the supplied pixrect.</returns>

        public static CharCanvas FromPixRectByForegroundColour(clsPixRect pr, int col)
        {
            return FromBitMask(pr.GetBitmaskMap(col));
        }

        /// <summary>
        /// Creates a new canvas from the supplied pixrect object, using a background
        /// colour to determine which pixels represent the text. Any pixel not
        /// matching the background colour is determined to be part of the text.
        /// </summary>
        /// <param name="PR">The pixrect from which to create the canvas.
        /// The pixrect will not be modified.</param>
        /// <param name="iBackgroundColour">The background colour of interest to
        /// indirectly define which pixels are considered to be foreground "text".
        /// </param>
        /// <returns>Returns a new canvas, corresponding to the incidence of the
        /// background colour of interest on the supplied pixrect.</returns>
        public static CharCanvas FromPixRectByBackgroundColour(
            clsPixRect pr, int bgCol, bool eraseBlocks)
        {
            if (eraseBlocks)
                pr = RemoveEnclosingBlocksFromPixRect(pr, bgCol);
            return FromBitMask(pr.GetBitmaskMapByBackground(bgCol));
        }

        /// <summary>
        /// Creates a canvas representing the supplied BitMask.
        /// </summary>
        /// <param name="BitMask">The bitmask from which to create a canvas.</param>
        /// <returns>Returns a new canvas representing the supplied bitmask.</returns>
        private static CharCanvas FromBitMask(bool[,] mask)
        {
            int w = mask.GetLength(0);
            int h = mask.GetLength(1);

            CharCanvasState[,] state = new CharCanvasState[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    state[x, y] = (
                        mask[x, y]
                        ? CharCanvasState.InkExists
                        : CharCanvasState.NoInkExists
                    );
                }
            }

            return new CharCanvas(state);
        }

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new canvas from the given data.
        /// </summary>
        /// <param name="data">The data to use in the new canvas. Note that this
        /// array is used as is, not copied - for data integrity it should not be
        /// modified after a canvas has been created from it.</param>
        private CharCanvas(CharCanvasState[,] data) : this(data, false) { }

        /// <summary>
        /// Creates a new canvas from the given data or a copy of it.
        /// </summary>
        /// <param name="data">The data to use in the new canvas.</param>
        /// <param name="copy">True to copy the data, false to use the array given.
        /// Note that, if not copied, the array should not be modified after a canvas
        /// has been created from it.</param>
        private CharCanvas(CharCanvasState[,] data, bool copy) : base(data, copy) { }

        #endregion

        #region - Methods -

        /// <summary>
        /// Gets the char equivalent of the given char canvas state value for
        /// outputting this canvas as a string.
        /// </summary>
        /// <param name="value">The canvas state value for which a char is required.
        /// </param>
        /// <returns>A char representing the given canvas state value.</returns>
        protected override char GetCharEquivalent(CharCanvasState value)
        {
            switch (value)
            {
                case CharCanvasState.InkExists: return 'X';
                case CharCanvasState.DeletedInkExists: return '/';
                default: return ' ';
            }
        }

        /// <summary>
        /// Gets the colour equivalent of the given char canvas state value for
        /// outputting this canvas as a bitmap
        /// </summary>
        /// <param name="val">The canvas state value for which a colour is required.
        /// </param>
        /// <returns>The colour representing the given char canvas state</returns>
        protected override Color GetColorEquivalent(CharCanvasState value)
        {
            switch (value)
            {
                case CharCanvasState.InkExists: return Color.Black;
                case CharCanvasState.DeletedInkExists: return Color.SlateGray;
                default: return Color.White;
            }
        }

        /// <summary>
        /// Creates a new CharCanvas from the given data.
        /// </summary>
        /// <param name="data">The data to populate the new CharCanvas with.</param>
        /// <param name="copyData">True to copy the array, false if the given array
        /// can be used safely within the object.</param>
        /// <returns>A new GridData populated with the given data.</returns>
        protected override GridData<CharCanvasState> Create(
            CharCanvasState[,] data, bool copyData)
        {
            return new CharCanvas(data, copyData);
        }

        /// <summary>
        /// Gets a sub-canvas drawn from the data on this canvas and contained in
        /// the specified rectangle.
        /// </summary>
        /// <param name="r">The rectangle providing the co-ords for which the
        /// subcanvas is required.</param>
        /// <returns>A CharCanvas containing the data from this canvas within the
        /// specified rectangle.</returns>
        /// <remarks>If the rectangle exceeds the bounds of this canvas, any extra
        /// cells will be initialised with the default value (ie. No Ink)</remarks>
        public CharCanvas SubCanvas(Rectangle r)
        {
            return (CharCanvas)SubGridCore(r);
        }

        /// <summary>
        /// Gets a sub-canvas drawn from the data on this canvas at the specified
        /// point and of the given size.
        /// </summary>
        /// <param name="p">The point at which the sub canvas is required.</param>
        /// <param name="sz">The size of the required canvas.</param>
        /// <returns>A CharCanvas containing the data from this canvas within the
        /// specified rectangle.</returns>
        /// <remarks>If the parameters describe a rectangle which exceeds the bounds
        /// of this canvas, any extra cells will be initialised with the default
        /// value (ie. No Ink)</remarks>
        public CharCanvas SubCanvas(Point p, Size sz)
        {
            return SubCanvas(new Rectangle(p, sz));
        }

        /// <summary>
        /// Creates a clone of the specified rows from this canvas.
        /// </summary>
        /// <param name="startRow">The zero-based inclusive index from which to start
        /// copying.</param>
        /// <param name="endRow">The zero-based inclusive index at which to stop
        /// copying.</param>
        /// <returns>Returns a partial clone of the current canvas</returns>
        /// <exception cref="OutOfRangeException">If either startRow or endRow are
        /// outside the bounds of this canvas</exception>
        /// <exception cref="InvalidValueException">If endRow is before startRow
        /// </exception>
        public CharCanvas CopyRows(int startRow, int endRow)
        {
            int max = Height - 1; // The maximum allowed row index

            if (startRow < 0 || startRow > max)
                throw new OutOfRangeException(
                    Resources.StartRow0MustBeBetween0And1, startRow, max);
            if (endRow < 0 || endRow > max)
                throw new OutOfRangeException(
                    Resources.EndRow0MustBeBetween0And1, endRow, max);

            if (endRow < startRow)
                throw new InvalidValueException(
                    Resources.EndRow1MustNotExceedStartRow0, startRow, endRow);

            return SubCanvas(
                new Rectangle(0, startRow, Width, endRow + 1 - startRow));
        }

        /// <summary>
        /// Gets a char canvas containing the ink on this canvas with all empty
        /// columns on either side of the ink removed.
        /// </summary>
        /// <returns>A CharCanvas object with the empty columns from either side of
        /// the ink on the canvas removed.</returns>
        public CharCanvas TrimHorizontal()
        {
            return (CharCanvas)TrimHorizontalCore(0);
        }

        /// <summary>
        /// Trims the vertical space from this canvas
        /// </summary>
        /// <returns>A CharCanvas object with the ink from this canvas but with the
        /// empty rows from above and below this canvas removed.</returns>
        public CharCanvas TrimVertical()
        {
            return TrimVertical(0, 0, 0);
        }

        /// <summary>
        /// Trims the vertical space from this canvas
        /// </summary>
        /// <param name="topPadding">The padding to leave above the trimmed canvas.
        /// </param>
        /// <param name="bottomPadding">The padding to leave below the trimmed canvas
        /// </param>
        /// <param name="minHeight">The minimum height of the trimmed canvas. If
        /// necessary, extra padding is added to the bottom of the trimmed canvas to
        /// ensure it reaches this height</param>
        /// <returns>A CharCanvas object with the ink from this canvas but with the
        /// empty rows from above and below this canvas removed, padded to the
        /// required amount and having at least the minimum height specified.
        /// </returns>
        public CharCanvas TrimVertical(int topPadding, int bottomPadding, int minHeight)
        {
            return (CharCanvas)TrimVerticalCore(topPadding, bottomPadding, minHeight);
        }

        /// <summary>
        /// Deletes the given points from this canvas and returns it.
        /// </summary>
        /// <param name="inkCoords">A collection of points for which the ink should
        /// be deleted.</param>
        /// <returns>A new char canvas with the given points' ink deleted.</returns>
        public CharCanvas DeletePoints(ICollection<Point> inkCoords)
        {
            CharCanvasState[,] newState = CopiedValue;
            foreach (Point p in inkCoords)
            {
                newState[p.X, p.Y] = CharCanvasState.DeletedInkExists;
            }
            return new CharCanvas(newState);
        }

        #endregion

    }
}
