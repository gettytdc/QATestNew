using System;
using System.Collections.Generic;
using System.Drawing;
using BluePrism.BPCoreLib.Collections;

// This file brings together the cell highlighter interface and a few simple
// implementations.
namespace AutomateControls.Diary
{
    /// <summary>
    /// Interface describing an object which decides whether a cell representing
    /// a particular date/time should be highlighted or not, and the colour that
    /// should be used to highlight it.
    /// </summary>
    public abstract class CellHighlighter
    {
        /// <summary>
        /// The default colour to use when one is not specified in the creation
        /// of a BaseHighlighter object.
        /// </summary>
        public static readonly Color DefaultColour = Color.FromArgb(0xff, 0xff, 0xee);

        /// <summary>
        /// Gets the highlight colour to use for the given date/time. Returning
        /// null indicates that the cell should not be highlighted.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>The colour to use as the highlight colour, or null to 
        /// indicate that it should not be highlighted.</returns>
        public abstract Color? GetHighlight(DateTime date);
    }

    /// <summary>
    /// Cell highlighter which holds a single colour for use as a highlight colour.
    /// </summary>
    public abstract class SingleColourCellHighlighter : CellHighlighter
    {
        /// <summary>
        /// The single colour to use for the highlight colour.
        /// </summary>
        private Color _color;

        /// <summary>
        /// Gets / sets the highlight colour to use for this cell highlighter.
        /// </summary>
        public Color HighlightColour
        {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>
        /// Creates a new single colour cell highlighter which uses the default colour.
        /// </summary>
        /// <param name="c">The colour to use when highlighting cells.</param>
        public SingleColourCellHighlighter() : this(DefaultColour) { }

        /// <summary>
        /// Creates a new single colour cell highlighter which uses the specified colour.
        /// </summary>
        /// <param name="c">The colour to use when highlighting cells.</param>
        public SingleColourCellHighlighter(Color c)
        {
            _color = c;
        }
    }

    /// <summary>
    /// Highlighter implementation which doesn't highlight any cells.
    /// </summary>
    public class EmptyHighlighter : CellHighlighter
    {
        /// <summary>
        /// Always returns null to indicate that the cell should not be highlighted.
        /// </summary>
        /// <param name="date">The date to not check.</param>
        /// <returns>null.</returns>
        public override Color? GetHighlight(DateTime date)
        {
            return null;
        }
    }

    /// <summary>
    /// Highlighter implementation which highlights the current day.
    /// </summary>
    public class CurrentDateHighlighter : SingleColourCellHighlighter
    {
        /// <summary>
        /// Creates a new current day highlighter using the default colour.
        /// </summary>
        public CurrentDateHighlighter() : base() { }

        /// <summary>
        /// Creates a new current day highlighter using the given colour.
        /// </summary>
        /// <param name="col"></param>
        public CurrentDateHighlighter(Color col) : base(col) { }

        /// <summary>
        /// Gets the highlight colour to use for the given date.
        /// </summary>
        /// <param name="date">The date to check for highlighting.</param>
        /// <returns>The colour to use to highlight the date if it falls on
        /// today's date, otherwise null.</returns>
        public override Color? GetHighlight(DateTime date)
        {
            return ((date.Date == DateTime.Today) ? new Color?(HighlightColour) : null);
        }
    }

    /// <summary>
    /// Highlighter implementation which highlights specific days of the week.
    /// </summary>
    public class DaysHighlighter : SingleColourCellHighlighter
    {
        /// <summary>
        /// The collection of days to highlight.
        /// </summary>
        private ICollection<DayOfWeek> _days;

        /// <summary>
        /// Creates a new highlighter which highlights the given days of the
        /// week with the default colour.
        /// </summary>
        /// <param name="days">The days of the week which should be highlighted.
        /// </param>
        public DaysHighlighter(params DayOfWeek[] days) : this(DefaultColour, days) { }

        /// <summary>
        /// Creates a new highlighter which highlights the given days of the
        /// week with the specified colour.
        /// </summary>
        /// <param name="c">The colour to use to highlight the given dates.</param>
        /// <param name="days">The days of the week which should be highlighted.
        /// </param>
        public DaysHighlighter(Color c, params DayOfWeek[] days)
            : base(c)
        {
            _days = new clsSet<DayOfWeek>(days);
        }

        /// <summary>
        /// Gets the highlight colour for the given date, or null to indicate that
        /// it should not be highlighted.
        /// </summary>
        /// <param name="date">The date to check for highlighting.</param>
        /// <returns>The colour to use to highlight the cell representing the given
        /// date/time or null to indicate that it should not be highlighted.</returns>
        public override Color? GetHighlight(DateTime date)
        {
            return (_days.Contains(date.DayOfWeek) ? new Color?(HighlightColour) : null);
        }
    }

    /// <summary>
    /// Highlighter which uses a specified time range to determine if a cell should
    /// be highlighted or not.
    /// </summary>
    public class TimeRangeHighlighter : SingleColourCellHighlighter
    {
        /// <summary>
        /// The beginning of the time range at which highlighting should occur.
        /// </summary>
        private TimeSpan _from;

        /// <summary>
        /// The (inclusive) end of the time range at which highlighting should occur.
        /// </summary>
        private TimeSpan _to;

        /// <summary>
        /// Creates new highlighter which highlights times between the given values.
        /// </summary>
        /// <param name="from">The beginning of the time range at which highlighting
        /// should occur.</param>
        /// <param name="to">The (inclusive) end of the time range at which
        /// highlighting should occur.</param>
        public TimeRangeHighlighter(TimeSpan from, TimeSpan to) : this(DefaultColour, from, to) { }

        /// <summary>
        /// Creates new highlighter which highlights times between the given values.
        /// </summary>
        /// <param name="c">The color to use to highlight cell entries.</param>
        /// <param name="from">The beginning of the time range at which highlighting
        /// should occur.</param>
        /// <param name="to">The (inclusive) end of the time range at which
        /// highlighting should occur.</param>
        public TimeRangeHighlighter(Color c, TimeSpan from, TimeSpan to) 
            : base(c)
        {
            _from = from;
            _to = to;
        }

        /// <summary>
        /// Gets the highlight colour to use for the given date/time.
        /// </summary>
        /// <param name="date">The date/time at which the highlight colour is required.
        /// </param>
        /// <returns>The colour to use to highlight the cell representing the given
        /// date/time, or null to indicate no highlighting should occur.</returns>
        public override Color? GetHighlight(DateTime date)
        {
            TimeSpan time = date.TimeOfDay;
            return ((time >= _from && time <= _to) ? new Color?(HighlightColour) : null);
        }
    }

    /// <summary>
    /// Highlighter which merges an arbitrary number of highlighters together such
    /// that if any of the highlighters determine that a cell should be highlighted
    /// then it will be. Note that the colour returned will be the colour of the
    /// first successful highlighter, so the order of the highlighters given is
    /// important.
    /// </summary>
    public class MergingHighlighter : CellHighlighter
    {
        /// <summary>
        /// The highlighters to merge into one highlighter.
        /// </summary>
        private ICollection<CellHighlighter> _highlighters;

        /// <summary>
        /// Creates a new highlighter which merges the given highlighters 
        /// together.
        /// </summary>
        /// <param name="highlighters">The highlighters to merge into a single
        /// highlighter. The order determines the order in which the highlighters
        /// are asked if a particular date/time should be highlighted. The first
        /// to decide that a cell should be highlighted will determine the colour
        /// to be used for highlighting that cell.</param>
        public MergingHighlighter(params CellHighlighter[] highlighters)
        {
            _highlighters = highlighters;
        }
        /// <summary>
        /// Checks if the cell representing the given date/time should be highlighted
        /// or not.
        /// </summary>
        /// <param name="date">The date to check to see if its cell should be
        /// highlighted.</param>
        /// <returns>The colour to use to highlight the cell, or null if it should
        /// not be highlighted.</returns>
        public override Color? GetHighlight(DateTime date)
        {
            foreach (CellHighlighter ch in _highlighters)
            {
                Color? c = ch.GetHighlight(date);
                if (c.HasValue)
                    return c;
            }
            return null;
        }
    }

    /// <summary>
    /// Highlighter which highlights all dates / times before a particular threshold
    /// with one colour, and any on or after the threshold with a different colour.
    /// </summary>
    public class ThresholdHighlighter : CellHighlighter
    {
        /// <summary>
        /// The threshold date/time on which to change the highlight colour.
        /// </summary>
        private DateTime _threshold;

        /// <summary>
        /// The highlight colour to use before the threshold date/time.
        /// </summary>
        private Color? _before;

        /// <summary>
        /// The highlight colour to use on or after the threshold date/time.
        /// </summary>
        private Color? _after;

        /// <summary>
        /// Creates a new highlighter which decides on the highlight colour based
        /// on the relation of the cell's date/time with the specified threshold.
        /// </summary>
        /// <param name="threshold">The date / time which determines which colour
        /// to use for the highlight.</param>
        /// <param name="before">The colour to use for dates before the threshold
        /// date (or null to indicate no highlighting)</param>
        /// <param name="after">The colour to use for dates on or after the 
        /// threshold date (or null to indicate no highlighting)</param>
        public ThresholdHighlighter(DateTime threshold, Color? before, Color? after)
        {
            _threshold = threshold;
            _before = before;
            _after = after;
        }

        /// <summary>
        /// Gets the highlight colour for the given date/time.
        /// </summary>
        /// <param name="date">The date/time to check for highlighting.</param>
        /// <returns>The colour to use to highlight the cell, or null to indicate
        /// that it should not be highlighted.</returns>
        public override Color? GetHighlight(DateTime date)
        {
            return (date < _threshold ? _before : _after);

        }
    }
}
