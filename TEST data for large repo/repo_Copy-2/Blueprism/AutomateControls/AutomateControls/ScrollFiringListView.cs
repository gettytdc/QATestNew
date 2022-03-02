using System;
using System.Windows.Forms;
using AutomateControls.WindowsSupport;

namespace AutomateControls
{
    /// <summary>
    /// Listview which fires a <see cref="Scroll"/> event when it is scrolled
    /// by the user.
    /// </summary>
    public class ScrollFiringListView : ScrollHandlingListView
    {
        /// <summary>
        /// Event fired when the list view is scrolled.
        /// </summary>
        public event ScrollEventHandler Scroll;

        /// <summary>
        /// Event handler for this list view being scrolled.
        /// </summary>
        /// <param name="e">The args detailing the scroll event</param>
        protected virtual void OnScroll(ScrollEventArgs e)
        {
            ScrollEventHandler handler = this.Scroll;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Event handler for this list being resized.
        /// Ensures that the scroll position is updated since the resize may have
        /// caused it to be altered.
        /// </summary>
        /// <param name="e">The event args detailing the resize.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, ScrollPosition.X));
        }

        /// <summary>
        /// Override of the Windows Process method which checks for scroll events
        /// and ensures they are fired by this listview.
        /// </summary>
        /// <param name="m">The message to be processed.</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WindowsMessage.WM_VSCROLL || m.Msg == WindowsMessage.WM_HSCROLL)
            {
                int hiword = (0xffff & (m.WParam.ToInt32() >> 16));
                int loword = (m.WParam.ToInt32() & 0xffff);

                ScrollEventType type = (ScrollEventType)loword;
                switch (type)
                {
                    case ScrollEventType.ThumbTrack:
                        OnScroll(new ScrollEventArgs(type, hiword));
                        break;

                    default:
                        OnScroll(new ScrollEventArgs(type, ScrollPosition.X));
                        break;
                }
            }
        }
    }
}
