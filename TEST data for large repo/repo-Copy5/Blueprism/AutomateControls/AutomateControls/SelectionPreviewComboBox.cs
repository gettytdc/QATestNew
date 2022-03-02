using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace AutomateControls
{
    /// <summary>
    /// Combobox class which adds a preview of the index changing 'before' the index
    /// is actually changed, allowing the change to be cancelled if necessary
    /// </summary>
    public class SelectionPreviewComboBox : ComboBox
    {
        // Flag indicating that this control is handling a changed index
        private bool _handlingChange;

        // The last committed index when this control had focus
        private int _savedIndex = -1;

        // The saved selected index while the change is being handled
        private int _newSelectedIndex;

        /// <summary>
        /// Event fired when the selected index is changin
        /// </summary>
        public event CancelEventHandler SelectedIndexChanging;

        /// <summary>
        /// Saves the selected index when this combo box is entered
        /// </summary>
        protected override void OnEnter(EventArgs e)
        {
            _savedIndex = SelectedIndex;
            base.OnEnter(e);
        }

        /// <summary>
        /// Handles the selected index having been changed.
        /// This temporarily reverts the change and raises a cancellable event
        /// indicating that the index is changing.
        /// </summary>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            // Ensure we don't get stuck in a recursive loop here.
            if (_handlingChange)
                return;
            _handlingChange = true;

            try
            {
                // Save the new index that has been changed to and temporarily
                // revert the change to our previously saved index.
                _newSelectedIndex = SelectedIndex;
                // If an item is deleted, that may be the saved index.
                // Unfortunately there's
                // a) no way to tell if this change is due to an item being deleted
                // b) no way to tell if this change is due to a user change rather
                //    than a programmatical change
                // c) no way to override any of the item collections in the base
                //    class to record either of the above.
                // So, all we can do is see if that index is within the bounds of
                // this combo box, which leads us to the point where this event will
                // occur if an item in the body of the list is deleted while it is
                // selected and the selected index will actually be the wrong one
                // (ie. it will be pointing to the item which has taken its place
                // in the list), and the event will not occur at all if the item
                // which has been deleted is at the end of the list.
                // FIXME: This really is pants but I can't figure a way around it
                //        which successfully mimics an event occurring before the
                //        SelectedIndexChanged event is released.
                //        SelectionChangeCommitted only occurs for user-initiated
                //        changes, but it is fired after the SelectedIndexChanged
                //        event is fired, meaning that a lot of UI will already have
                //        updated their contents accordingly, so the 'cancel' option
                //        is just out of date. Unfortunately, I cannot tell how the
                //        SelectionChangeCommitted event works out whether an event
                //        is user-initiated or program-initiated.
                if (_savedIndex >= -1 && _savedIndex < Items.Count)
                {
                    SelectedIndex = _savedIndex;

                    // Raise "Changing" event - allow listeners to preview the change
                    CancelEventArgs ce = new CancelEventArgs();
                    OnSelectedIndexChanging(ce);

                    // If a listener cancelled the event, do nothing - ie. leave the
                    // index in its reverted state. We don't need to propogate the
                    // IndexChanged event because the index hasn't actually been
                    // changed to any outside parties.
                    if (ce.Cancel)
                        return;
                }

                // If no-one cancelled the event, restore the saved new index
                // and propogate the IndexChanged event. Save the new index as
                // the base to revert to on future changes.
                SelectedIndex = _newSelectedIndex;
                _savedIndex = _newSelectedIndex;
                base.OnSelectedIndexChanged(e);

            }
            finally
            {
                _handlingChange = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="SelectedIndexChanging"/> event which allows
        /// listeners to preview an index change on the combo box
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSelectedIndexChanging(CancelEventArgs e)
        {
            if (SelectedIndexChanging != null)
                SelectedIndexChanging(this, e);
        }
    }
}
