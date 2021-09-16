using System;

namespace AutomateControls.Diary
{
    /// <summary>
    /// Event arguments class detailing the date range changing in a diary
    /// view control.
    /// </summary>
    public class DiaryViewDateRangeChangingEventArgs : EventArgs
    {
        // The start of the date range
        private DateTime _from;

        // The end of the date range.
        private DateTime _to;

        /// <summary>
        /// Creates a new event args detailing a date range changing in a
        /// diary view.
        /// </summary>
        /// <param name="from">The start date of the new date range.</param>
        /// <param name="to">The end date of the new date range.</param>
        public DiaryViewDateRangeChangingEventArgs(DateTime from, DateTime to)
        {
            _from = from;
            _to = to;
        }

        /// <summary>
        /// The start date of the new date range.
        /// </summary>
        public DateTime From { get { return _from; } }

        /// <summary>
        /// The end date of the new date range.
        /// </summary>
        public DateTime To { get { return _to; } }
    }
}
