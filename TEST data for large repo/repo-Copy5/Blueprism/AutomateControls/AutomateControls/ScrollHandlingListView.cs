using System.Runtime.InteropServices;

using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices.APIs;

namespace AutomateControls
{
    /// <summary>
    /// Listview which allows access to its current scroll state, and provides
    /// a method of ensuring that the scroll position of a listview is kept
    /// when it is updated.
    /// </summary>
    public class ScrollHandlingListView : FlickerFreeListView
    {
        /// <summary>
        /// Constant describing the horizontal scrollbar
        /// </summary>
        private const int SB_HORZ = 0x00;

        /// <summary>
        /// Constant describing the vertical scrollbar
        /// </summary>
        private const int SB_VERT = 0x01;

        /// <summary>
        /// The first listview message constant on which others are based.
        /// </summary>
        private const Int32 LVM_FIRST = 0x1000;

        /// <summary>
        /// The windows message indicating listview should perform a scroll operation
        /// </summary>
        private const Int32 LVM_SCROLL = LVM_FIRST + 20;
    
        /// <summary>
        /// External user32 method to get the scroll position of a control
        /// </summary>
        /// <param name="hWnd">The window handle of the control.</param>
        /// <param name="n"><see cref="SB_HORZ"/> or <see cref="SB_VERT"/></param>
        /// <returns>The scroll position of the given control, in pixels when getting
        /// the horizontal position, in listview rows when getting the vertical
        /// position.</returns>
        [DllImport("user32.dll", EntryPoint = "GetScrollPos")]
        static extern int GetScrollPos(IntPtr hWnd, int n);

        /// <summary>
        /// External user32 method to disable or re-enable drawing in a control.
        /// </summary>
        /// <param name="Handle">The window handle of the control</param>
        /// <returns>true if successful; false otherwise. Probably.</returns>
        [DllImport("user32.dll")]
        static extern bool LockWindowUpdate(IntPtr Handle);

        /// <summary>
        /// Delegate used to update the listview while retaining its current
        /// scroll position.
        /// Note that the listview is not drawn while within this method, so
        /// flicker will be reduced.
        /// </summary>
        /// <param name="lv">The listview to be updated.</param>
        public delegate void ListViewUpdater(ListView lv);

        /// <summary>
        /// Gets or sets the current scroll position of this listview.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point ScrollPosition
        {
            get
            {
                if (!IsHandleCreated)
                    return Point.Empty;

                int hscroll = GetScrollPos(this.Handle, SB_HORZ);
                int vscroll = GetScrollPos(this.Handle, SB_VERT); // comes back in rows, not pixels.
                return new Point(hscroll, vscroll * ItemHeight);
            }
            set
            {
                if (IsHandleCreated)
                    APIsUser32.SendMessage(this.Handle, LVM_SCROLL, (IntPtr)value.X, (IntPtr)value.Y);
            }
        }

        /// <summary>
        /// Updates the listview, ensuring that the current scroll position
        /// is retained and the the listview is not drawn until the listview
        /// has been updated.
        /// </summary>
        /// <param name="doUpdate">The delegate which is responsible for
        /// updating the listview.</param>
        public void UpdateListView(ListViewUpdater doUpdate)
        {
            // Disable painting
            LockWindowUpdate(this.Handle);

            // Save the current scroll position
            Point p = this.ScrollPosition;

            // Perform the update
            doUpdate(this);

            // Restore the scroll position
            this.ScrollPosition = p;

            // And re-enable painting
            LockWindowUpdate(IntPtr.Zero);
        }
    }
}
