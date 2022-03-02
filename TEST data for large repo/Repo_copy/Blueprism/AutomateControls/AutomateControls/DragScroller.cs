using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BluePrism.BPCoreLib;
using Win32 = BluePrism.BPCoreLib.modWin32;
using WindowMessages = BluePrism.BPCoreLib.modWin32.WindowMessages;
using SB = BluePrism.BPCoreLib.modWin32.SB;
using System.Drawing;

namespace AutomateControls
{
    /// <summary>
    /// A component which monitors the dragging on a control and sends scroll
    /// messages to that control if the user's drag position indicates that they want
    /// to scroll the control in a particular direction.
    /// </summary>
    public partial class DragScroller : Component
    {
        #region - Member Variables -

        // The control whose dragging this component is monitoring.
        private Control _ctl;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new drag scroller within the given container.
        /// </summary>
        /// <param name="container">The container to hold this component.</param>
        public DragScroller(IContainer container)
            : this()
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            container.Add(this);
        }

        /// <summary>
        /// Creates a new, independent drag scroller component.
        /// </summary>
        public DragScroller()
        {
            InitializeComponent();
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Assigns this drag scroller to the given control. A drag scroller
        /// component looks after a single control.
        /// </summary>
        [Browsable(true), Category("Behavior"), Description(
            "The control to monitor the scrolling on behalf of")]
        public Control Control
        {
            get { return _ctl; }
            set
            {
                if (value == _ctl) return;
                RemoveHandlers(_ctl);
                _ctl = value;
                AddHandlers(_ctl);
            }
        }

        /// <summary>
        /// The scroll direction to use for the hovering while dragging.
        /// </summary>
        private Direction HoverScrollDirection
        {
            get { return BPUtil.IfNull(timerScroll.Tag, Direction.None); }
            set
            {
                Direction currDirn = HoverScrollDirection;
                if (value == currDirn)
                    return;
                timerScroll.Tag = value;
                timerScroll.Stop();
                if (value != Direction.None)
                    timerScroll.Start();
            }
        }

        #endregion

        #region - Event Handlers -

        /// <summary>
        /// Handles a drag event occurring over the assigned control
        /// </summary>
        private void HandleDragOver(object sender, DragEventArgs e)
        {
            Debug.Assert(object.ReferenceEquals(sender, _ctl));
            HoverScrollDirection = GetScrollDirection(_ctl, e.X, e.Y);
        }

        /// <summary>
        /// Handles the drag operation leaving or dropping on the target control
        /// </summary>
        private void HandleDragLeaveOrDrop(object sender, EventArgs e)
        {
            HoverScrollDirection = Direction.None;
        }

        /// <summary>
        /// Handles the scroll timer ticking over, sending the scroll message(s) in
        /// the appropriate direction if any is set.
        /// </summary>
        /// <param name="sender">The source of the event (ie. the timer)</param>
        /// <param name="e">The args detailing the event</param>
        private void HandleTimerTick(object sender, EventArgs e)
        {
            Direction dirn = HoverScrollDirection;
            if (dirn == Direction.None)
                return;
            HandleRef href = new HandleRef(_ctl, _ctl.Handle);
            if (dirn.HasFlag(Direction.Top))
                SendMessage(href, WindowMessages.WM_VSCROLL, SB.SB_LINEUP, 0);
            if (dirn.HasFlag(Direction.Bottom))
                SendMessage(href, WindowMessages.WM_VSCROLL, SB.SB_LINEDOWN, 0);
            if (dirn.HasFlag(Direction.Left))
                SendMessage(href, WindowMessages.WM_HSCROLL, SB.SB_LINELEFT, 0);
            if (dirn.HasFlag(Direction.Right))
                SendMessage(href, WindowMessages.WM_HSCROLL, SB.SB_LINERIGHT, 0);
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Small wrapper around SendMessage() which means the calls can just use
        /// enums without explicit casts
        /// </summary>
        /// <param name="href">The HandleRef to the HWND to send the message to.
        /// </param>
        /// <param name="msg">The message to send</param>
        /// <param name="wParam">The wide param to send - in this context, this is
        /// the scroll submessage to send.</param>
        /// <param name="lParam">Generally not used in this context, typically 0.
        /// </param>
        /// <returns>The response from the send message call</returns>
        private int SendMessage(
            HandleRef href, WindowMessages msg, SB wParam, int lParam)
        {
            return Win32.SendMessage(href, msg, (int)wParam, lParam);
        }

        /// <summary>
        /// Adds the necessary event handlers to the given control
        /// </summary>
        /// <param name="c">The control on which the handlers should registered. If
        /// null, nothing is added.</param>
        private void AddHandlers(Control c)
        {
            if (c == null) return;
            c.DragOver += HandleDragOver;
            c.DragLeave += HandleDragLeaveOrDrop;
            c.DragDrop += HandleDragLeaveOrDrop;
        }

        /// <summary>
        /// Removes the event handlers set on the given control by this component.
        /// </summary>
        /// <param name="c">The control on which the handlers should be deregistered.
        /// If null, nothing is removed.</param>
        private void RemoveHandlers(Control c)
        {
            if (c == null) return;
            c.DragOver -= HandleDragOver;
            c.DragLeave -= HandleDragLeaveOrDrop;
            c.DragDrop -= HandleDragLeaveOrDrop;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed;
        /// otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Control = null;
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the scroll direction that is implied with the cursor at a given
        /// location. Primarily used when dragging into a scrollable control; the
        /// user does not have the ability to use scrollbars, so some form of
        /// auto-scrolling is necessary.
        /// Note that the areas of the control which are treated as areas where
        /// scrolling is implied by the location are at the edges (or beyond) of the
        /// control - those edges being defined as 1/16 of the width/height of the
        /// control, but no smaller than 16px and no larger than 32px. If the
        /// location is outside the client area of the control, it is considered to
        /// imply a scroll direction on the side(s) of the control on which it is out
        /// of bounds (although it should be noted that the DragLeave handler ensures
        /// that the scrolling is not continued after a drag target has left the
        /// treeview).
        /// </summary>
        /// <param name="c">The control on which the scroll direction is required for
        /// a given location.</param>
        /// <param name="screenx">The screen-relative x co-ordinate of the cursor
        /// from which to draw an implied scroll direction.</param>
        /// <param name="screeny">The screen-relative y co-ordinate of the cursor
        /// from which to draw an implied scroll direction.</param>
        /// <returns>The direction that a user might wish to scroll this control by
        /// holding the mouse in the specified location.</returns>
        private Direction GetScrollDirection(Control c, int screenx, int screeny)
        {
            Point locn = c.PointToClient(new Point(screenx, screeny));
            Direction dirn = Direction.None;
            Rectangle rect = c.ClientRectangle;

            // Define the size of the gutters (ie. the width in from the edges that a
            // location triggers a scroll requirement): constrain them to range 16:32
            Size gutter = new Size(
                (rect.Width / 16).Bound(16, 32), (rect.Height / 16).Bound(16, 32));

            // Prefer left over right (if the gutters overlap)
            if (locn.X < rect.X + gutter.Width)
                dirn = dirn.SetFlags(Direction.Left);
            else if (locn.X > rect.Right - gutter.Width)
                dirn = dirn.SetFlags(Direction.Right);

            // Similarly prefer top over bottom
            if (locn.Y < rect.Y + gutter.Height)
                dirn = dirn.SetFlags(Direction.Top);
            else if (locn.Y > rect.Bottom - gutter.Height)
                dirn = dirn.SetFlags(Direction.Bottom);

            return dirn;
        }

        #endregion

    }
}
