using System;

namespace AutomateControls
{
    /// <summary>
    /// Delegate describing an event handler for filter events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The args describing the filter event.</param>
    public delegate void FilterEventHandler(object sender, FilterEventArgs e);

    /// <summary>
    /// Event arguments for a filter text event.
    /// </summary>
    public class FilterEventArgs : EventArgs
    {
        // The filter text for this event.
        private string _text;

        /// <summary>
        /// Creates a new set of FilterEventArgs wrapping the given filter text.
        /// </summary>
        /// <param name="filterText">The text for the filter in this event.
        /// </param>
        public FilterEventArgs(string filterText)
        {
            _text = filterText;
        }

        /// <summary>
        /// The filter text for this event.
        /// </summary>
        public string FilterText
        {
            get { return _text; }
        }
    }
}
