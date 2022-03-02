using System.Windows.Forms;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// DataGridView which inhibits the 'ColumnWidthChanged' event while a
    /// 'ColumnHeaderMouseClick' event is in the process of being handled.
    /// Sorting a DataGridView caused the column widths to change rapidly while
    /// the sort was happening to cope with the sort arrow / separator width
    /// changing. Once it settled down it was the same width as before, so this just
    /// ignores all the messing around and lets it get back to where it was before.
    /// </summary>
    public class ColWidthInhibitingDataGridView : DataGridView
    {
        // Flag indicating if the columnclick event is being handled
        private bool _handlingColumnClick;

        /// <summary>
        /// Raises the ColumnHeaderMouseClick event.
        /// </summary>
        /// <param name="e">The args detailing the event</param>
        protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
        {
            _handlingColumnClick = true;
            try
            {
                base.OnColumnHeaderMouseClick(e);
            }
            finally
            {
                _handlingColumnClick = false;
            }
        }

        /// <summary>
        /// Raises the ColumnWidthChanged event if a ColumnHeaderMouseClick event is
        /// not currently being handled within this data grid view.
        /// </summary>
        /// <param name="e">The args detailing the event.</param>
        protected override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
            if (!_handlingColumnClick)
                base.OnColumnWidthChanged(e);
        }
    }
}
