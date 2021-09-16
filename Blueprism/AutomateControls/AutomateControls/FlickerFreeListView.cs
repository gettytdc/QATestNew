using System;
using System.Windows.Forms;
using AutomateControls.WindowsSupport;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.InteropServices.APIs;

namespace AutomateControls
{
    /// <summary>
    /// A list view which ignores requests to erase its background, by filtering
    /// out such messages. This means that it should not flicker when it is 
    /// resized.
    /// </summary>
    public class FlickerFreeListView : ListView
    {
        /// <summary>
        /// The structure used in a WM_WINDOWPOSCHANGING message
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        // ListView header format for a descending sort
        private const int HDF_SORTDOWN = 0x200;

        // ListView header format for an ascending sort
        private const int HDF_SORTUP = 0x400;

        // The first ListView message
        private const int LVM_FIRST = 0x1000;
        // The windows message to get the header handle from the listview
        private const int LVM_GETHEADER = LVM_FIRST + 31;

        // The column index to fill with available space
        private int _fillColumn = -1;

        /// <summary>
        /// Creates a new flicker free listview.
        /// </summary>
        public FlickerFreeListView()
        {
            // Set the control styles we need.
            // EnableNotifyMessage ensures that we get a chance to filter out Windows
            // messages before they get to WndProc
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.EnableNotifyMessage, true);
        }

        /// <summary>
        /// Gets the height of an individual listview item in this listview when
        /// displayed in 'Details' mode.
        /// </summary>
        [Browsable(true), Category("Appearance"), Description(
            "The height of an individual listview item in Details view")]
        public int ItemHeight
        {
            get
            {
                // The height of a listview item is the larger of the image size
                // and the height of the listview's font
                int rowHeight = 0;
                if (this.SmallImageList != null)
                {
                    rowHeight = this.SmallImageList.ImageSize.Height;
                }
                return Math.Max(this.Font.Height, rowHeight);
            }
        }

        /// <summary>
        /// Gets or sets the column to fill with available space in detail view.
        /// Any number outside of bounds (eg. -1) will not fill any column.
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(-1), Description(
         "Column to fill all available space in details view")]
        public int FillColumn
        {
            get { return _fillColumn; }
            set { _fillColumn = value; }
        }

        /// <summary>
        /// Filter out the 'Erase Background' message.
        /// </summary>
        /// <param name="m">The message this object is being notified of.</param>
        protected override void OnNotifyMessage(Message m)
        {
            if (m.Msg != WindowsMessage.WM_ERASEBKGND)
                base.OnNotifyMessage(m);
        }

        /// <summary>
        /// Handles the column width being changed.
        /// This just ensures that the listview is redrawn correctly - previously
        /// a column width change (a double click) could blank out the listview 
        /// area to the left of the resized column. No idea why.
        /// </summary>
        /// <param name="e">The arguments detailing the width change.</param>
        protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
        {
            base.OnColumnWidthChanged(e);
            Invalidate();
        }

        /// <summary>
        /// Sends a LVM_GETITEM message for the given column / header item.
        /// </summary>
        /// <param name="hnd">The handle to the listview's header control.</param>
        /// <param name="colIndex">The index of the column to send to.</param>
        /// <param name="item">The reference to the HDITEM to send or into which the
        /// header data should be received.</param>
        private void SendItem(
            IntPtr hnd, bool getItem, int colIndex, ref APIsStructs.HDITEM item)
        {
            APIsEnums.HeaderControlMessages msg = (getItem
             ? APIsEnums.HeaderControlMessages.GETITEM
             : APIsEnums.HeaderControlMessages.SETITEM);
            IntPtr colPtr = new IntPtr(colIndex);

            if (APIsUser32.SendMessage(hnd, msg, colPtr, ref item) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Sorts this listview on the specified column index and sort order
        /// </summary>
        /// <param name="columnIndex">The index of the column to sort on</param>
        /// <param name="order">The sort order to use</param>
        public void Sort(int columnIndex, SortOrder order)
        {
            // FIXME: Does this actually do anything? The columnIndex / order doesn't
            // appear to be used at all
            BeginUpdate();
            try
            {
                Cursor = Cursors.WaitCursor;
                Sort();
                SetSortIcon(columnIndex, order);
            }
            finally
            {
                EndUpdate();
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Sets the sort icon on the given column index, representing the specified
        /// sort order.
        /// </summary>
        /// <param name="columnIndex">The index of the required column</param>
        /// <param name="order">The sort order required.</param>
        public void SetSortIcon(int columnIndex, SortOrder order)
        {
            // Get the handle to the header sub-control of this listview
            IntPtr colHdr = APIsUser32.SendMessage(
                this.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

            // APIsEnums.HeaderControlMessages
            for (int col = 0; col <= Columns.Count - 1; col++)
            {
                APIsStructs.HDITEM item = new APIsStructs.HDITEM();
                item.mask = APIsEnums.HeaderItemFlags.FORMAT_;

                // Get the column header item format
                SendItem(colHdr, true, col, ref item);

                if (order != SortOrder.None && col == columnIndex)
                {
                    switch (order)
                    {
                        case SortOrder.Ascending:
                            item.fmt &= ~HDF_SORTDOWN;
                            item.fmt |= HDF_SORTUP;
                            break;
                        case SortOrder.Descending:
                            item.fmt &= ~HDF_SORTUP;
                            item.fmt |= HDF_SORTDOWN;
                            break;
                    }
                }
                else
                {
                    item.fmt &= ~(HDF_SORTDOWN | HDF_SORTUP);
                }

                // Set the new header format
                SendItem(colHdr, false, col, ref item);
            }
        }

        /// <summary>
        /// Handles the window message processing for this control
        /// </summary>
        /// <param name="m">The message to process</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WindowsMessage.WM_WINDOWPOSCHANGING)
                HandleWindowPosChanging(ref m);
            base.WndProc(ref m);
        }

        /// <summary>
        /// Resizes the freespace filling columns
        /// </summary>
        /// <param name="listViewWidth">The width of the listview to use in the width
        /// calculations</param>
        private void ResizeFillingColumns(int listViewWidth)
        {
            if (DesignMode) return;

            if (_fillColumn < 0 || _fillColumn >= Columns.Count) return;

            int totalWidth = 0;
            for (int i = 0, len = Columns.Count; i < len; i++)
            {
                if (i != _fillColumn)
                    totalWidth += Columns[i].Width;
            }
            Columns[_fillColumn].Width = listViewWidth - totalWidth;
        }

        /// <summary>
        /// Handles the layout event for this listview ensuring that the fill column
        /// is correctly handled on a layout of the control
        /// </summary>
        /// <param name="levent"></param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            ResizeFillingColumns(ClientSize.Width);
        }

        /// <summary>
        /// Handles the window position changing; this looks for shrinking resize
        /// events and deals with the fill column if it finds one.
        /// </summary>
        /// <param name="m"></param>
        protected virtual void HandleWindowPosChanging(ref Message m)
        {
            const int SWP_NOSIZE = 1;
            WINDOWPOS pos = (WINDOWPOS)m.GetLParam(typeof(WINDOWPOS));

            // ignore non-resize messages
            if ((pos.flags & SWP_NOSIZE) != 0) return;

            // only process when shrinking
            if (pos.cx >= Bounds.Width) return;

            // pos.cx is the window width, not the client area width, so we have to
            // subtract the border widths
            ResizeFillingColumns(pos.cx - (Bounds.Width - ClientSize.Width));
        }
    }
}
