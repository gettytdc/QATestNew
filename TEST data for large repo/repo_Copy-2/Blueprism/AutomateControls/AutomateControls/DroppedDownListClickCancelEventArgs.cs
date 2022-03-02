using System.ComponentModel;

namespace AutomateControls
{
    /// <summary>
    /// Delegate to use to handle a dropped down list click
    /// </summary>
    public delegate void DroppedDownListClickCancelEventHandler(
        object sender, DroppedDownListClickCancelEventArgs e);

    /// <summary>
    /// The args detailing a dropped down list being clicked.
    /// This can be used to cancel a click on an item.
    /// </summary>
    public class DroppedDownListClickCancelEventArgs : CancelEventArgs
    {
        // The index of the clicked item
        private int _index;

        /// <summary>
        /// Creates a new event args object detailing a click in a dropped down list
        /// at the specified index
        /// </summary>
        /// <param name="index">The index of the clicked item</param>
        public DroppedDownListClickCancelEventArgs(int index)
        {
            _index = index;
        }

        /// <summary>
        /// The index of the item which was clicked in the dropped down list
        /// </summary>
        public int Index { get { return _index; } }
    }
}
