using System;
using System.Text;

using System.Windows.Forms;
using System.Windows.Forms.Layout;
using System.ComponentModel;
using System.Drawing;

namespace AutomateControls
{
    /// <summary>
    /// A panel which lays out all of its controls in a single column,
    /// set to occupy the full width accorded to the panel.
    /// </summary>
    public class FullWidthFlowLayoutPanel : Panel
    {
        // The standard amount of wheel delta that is used for a single 'click'
        // of the mousewheel.
        private const int StandardWheelDelta = 120;

        // The size of this control the last time it was laid out
        // Note - unused at the moment due to refreshing issues.
        private Size _cachedPreferredSize;

        // The padding to allow for the vertical scrollbar in this control
        private int _scrollPadding;

        // The delta to use for a single registered mousewheel turn - default
        // in windows is 120, which quite often for these panels is way too large.
        private int _wheelDelta;

        /// <summary>
        /// Creates a new panel.
        /// </summary>
        public FullWidthFlowLayoutPanel()
        {
            _wheelDelta = StandardWheelDelta;
            _scrollPadding = 0;
            _cachedPreferredSize = Size.Empty;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.DoubleBuffered = true;
        }

        /// <summary>
        /// The cached preferred size - set by the layout manager when the
        /// control is laid out. This is necessary since GetPreferredSize()
        /// always seems to be passed a size with zero width, and the client
        /// and display rectangles are empty at the point that is called,
        /// so this control cannot know how much width it has available to
        /// it, which is awkward, since width is the primary attribute
        /// governing the resultant preferred height of child controls and,
        /// ultimately, this control.
        /// </summary>
        protected internal Size CachedPreferredSize
        {
            set { _cachedPreferredSize = value; }
        }

        /// <summary>
        /// Gets the engine responsible for laying out the child controls
        /// of this panel.
        /// <seealso cref="FullWidthFlowLayoutEngine"/>
        /// </summary>
        public override LayoutEngine LayoutEngine
        {
            get { return FullWidthFlowLayoutEngine.Instance; }
        }

        /// <summary>
        /// The padding required to cater for the vertical scrollbar, helping
        /// ensure that the horizontal scrollbar is never seen.
        /// </summary>
        internal int ScrollPadding
        {
            get { return _scrollPadding; }
        }

        /// <summary>
        /// The delta magnitude to use for mouse wheel events - ie. the number of
        /// pixels by which this panel should scroll for a single mousewheel turn
        /// event. The amount of delta which generates an event is 120 - ie. scroll
        /// by 120 pixels, which for smaller panels is too much.
        /// </summary>
        [DisplayName("Mouse Wheel Delta Magnitude"), DefaultValue(120),
         Description("The number of pixels to scroll by for a single mousewheel turn")]
        internal int MouseWheelDeltaMagnitude
        {
            get { return _wheelDelta; }
            set { _wheelDelta = Math.Max(Math.Abs(value), 10); }
        }

        /// <summary>
        /// Tone down the scrolling for the flow layout panel. There's almost
        /// </summary>
        /// <param name="e">The original args detailing the mouse event.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Find out how may turns the wheel has registered
            int turns = e.Delta / StandardWheelDelta;

            // use our 'single wheel delta' amount to get the actual number of
            // pixels we should scroll by.
            int delta = turns * _wheelDelta;

            // Pass on the message to the base class with the modified delta.
            base.OnMouseWheel(new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, delta));
        }

        /// <summary>
        /// Adjusts the scroll bars on the container based on the current control
        /// positions and the control currently selected.
        /// This override just ensures that the scroll padding is set or unset
        /// depending on whether the control should <paramref
        /// name="displayScrollBars">display scroll bars</paramref> or not.
        /// </summary>
        /// <param name="displayScrollbars">true to show the scroll bars;
        /// otherwise, false.</param>
        protected override void AdjustFormScrollbars(bool displayScrollbars)
        {
            _scrollPadding = (displayScrollbars ? SystemInformation.VerticalScrollBarWidth : 0);
            base.AdjustFormScrollbars(displayScrollbars);
        }

        /// <summary>
        /// Gets the preferred size for this panel.
        /// If it has been laid out, the preferred size will be the size of
        /// the panel the last time the layout manager laid it out.
        /// Otherwise, it is calculated from the given proposed size.
        /// </summary>
        /// <param name="proposedSize">The proposed size for the control.
        /// </param>
        /// <returns>The size that this panel would prefer to be.</returns>
        /// <remarks>"proposedSize" seems to always have a width of 0 (at least
        /// when this panel is Fill-docked) which renders this method a bit
        /// useless since the width is the primary factor used to determine how
        /// high the panel would prefer to be. This is why the cached size is
        /// used the way it is.</remarks>
        public override Size GetPreferredSize(Size proposedSize)
        {
            // It would be nice to use the cached preferred size, but it needs
            // to be a bit cleverer than this, or the panel will collapse to
            // zero height until a Size event is fired - or something which
            // resets the cached size, anyway.
            //if (_cachedPreferredSize != Size.Empty)
            //{
            //    return _cachedPreferredSize;
            //}
            
            // Given the width, go through the controls to get the preferred height for
            // each of them.
            if (proposedSize.Width > _scrollPadding)
            {
                proposedSize.Width -= _scrollPadding;
            }

            Rectangle rect = DisplayRectangle;
            if (rect.Width > _scrollPadding)
                rect.Width -= _scrollPadding;

            StringBuilder sb = new StringBuilder();
            int height = Padding.Vertical;
            sb.Append(height);
            foreach (Control ctl in Controls)
            {
                // Only applies to visible controls.
                if (ctl.Visible)
                {
                    Size sz = proposedSize;
                    if (sz.Width >= ctl.Margin.Horizontal)
                        sz.Width -= ctl.Margin.Horizontal;
                    height += ctl.Margin.Vertical + ctl.Padding.Vertical +
                        Math.Max(ctl.GetPreferredSize(sz).Height, ctl.Height);
                }
            }
            return new Size(proposedSize.Width, height);
        }
    }

    /// <summary>
    /// The layout engine which handles the laying out of the controls in a
    /// <see cref="FullWidthFlowLayoutPanel"/>.
    /// </summary>
    internal class FullWidthFlowLayoutEngine : LayoutEngine
    {
        /// <summary>
        /// The single instance of the layout engine.
        /// </summary>
        public static readonly LayoutEngine Instance = new FullWidthFlowLayoutEngine();

        /// <summary>
        /// Lays out the given container.
        /// </summary>
        /// <param name="container">The container to layout. This should be an
        /// instance of <see cref="FullWidthFlowLayoutPanel"/></param>
        /// <param name="layoutEventArgs">The event args detailing the layout
        /// event.</param>
        /// <returns>Flag indicating whether or not the container's parent should
        /// perform layout as a result of this layout.</returns>
        public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
        {
            FullWidthFlowLayoutPanel parent = container as FullWidthFlowLayoutPanel;
            if (parent == null)
                throw new InvalidOperationException("Cannot layout: " + container);

            // Use DisplayRectangle so that parent.Padding is honored
            Rectangle rect = parent.DisplayRectangle;
            if (rect.Width > parent.ScrollPadding)
                rect.Width -= parent.ScrollPadding;

            Point nextControlLocation = rect.Location;

            foreach (Control ctl in parent.Controls) // sortedControls.Values)
            {
                // Only apply layout to visible controls.
                if (!ctl.Visible)
                    continue;

                // Respect the margin of the control:
                // shift over the left and the top.
                nextControlLocation.Offset(ctl.Margin.Left, ctl.Margin.Top);

                // Set the location of the control.
                ctl.Location = nextControlLocation;

                // set the size of the control to its preferred size, forcing the width to
                // the display width of the container
                Size dispSize = rect.Size;
                if (dispSize.Width >= ctl.Margin.Horizontal)
                    dispSize.Width -= ctl.Margin.Horizontal;

                Size pref = ctl.GetPreferredSize(dispSize);
                if (pref.Height == 0) pref.Height = ctl.Height;
                pref.Width = rect.Width - ctl.Margin.Horizontal;
                ctl.Size = pref;
                // Move X back to the display rectangle origin.
                nextControlLocation.X = rect.X;

                // Increment Y by the height of the control
                // and the bottom margin.
                nextControlLocation.Y += ctl.Height;
            }
            parent.CachedPreferredSize = new Size(rect.Width, nextControlLocation.Y);

            // Optional: Return whether or not the container's
            // parent should perform layout as a result of this
            // layout. Some layout engines return the value of
            // the container's AutoSize property.
            return true;
        }
    }
}
