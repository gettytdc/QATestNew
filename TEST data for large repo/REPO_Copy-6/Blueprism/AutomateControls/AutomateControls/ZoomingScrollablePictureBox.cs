using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using BluePrism.BPCoreLib.Collections;

namespace AutomateControls
{
    /// <summary>
    /// Control which contains a picture box which can zoom and scroll its picture
    /// </summary>
    public class ZoomingScrollablePictureBox : ScrollablePictureBox
    {
        #region - Class-scope declarations -

        private const int WM_SCROLL = 276; // Horizontal scroll 
        private const int SB_LINELEFT = 0; // Scrolls one cell left 
        private const int SB_LINERIGHT = 1; // Scrolls one line right
        private const int SB_LEFT = 6; // Scrolls one cell left 
        private const int SB_RIGHT = 7; // Scrolls one line right

        private const float ZOOMSTEP = 0.2f;
        private const float MINZOOM = 0.1f;
        private const float MAXZOOM = 5;

        private static readonly IList<float> DefaultZoomLevels = GetReadOnly.IList(new float[]{
            0.1f, 0.25f, 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 2f, 2.25f, 2.5f, 3f, 4f, 5f
        });

        [Category("Appearance")]
        public event ZoomLevelChangeHandler ZoomLevelChanged;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        
        #endregion

        #region - Private Member Variables -

        // The control that this control stole focus from when the mouse entered it
        private Control _stolenFocusFrom;

        // True to grab focus on mouse enter events and return it on mouse leave
        private bool _grab;

        // The zoom factor to use for this control
        private float _factor;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new zooming scrollable picture box, not currently zoomed in.
        /// </summary>
        public ZoomingScrollablePictureBox()
        {
            _factor = 1.0f;
            picbox.MouseEnter += HandlePicboxMouseEnter;
            picbox.MouseLeave += HandlePicboxMouseLeave;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The percentage representing the zoom level for this picture box.
        /// </summary>
        [Browsable(true), DefaultValue(100)]
        public int ZoomPercent
        {
            get
            {
                return (int)(100.0 * ZoomFactor);
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "ZoomPercent must be positive");
                this.ZoomFactor = (float) value / 100.0f;
            }
        }

        /// <summary>
        /// The zoom factor currently set for this picture box.
        /// </summary>
        [Browsable(false)]
        public float ZoomFactor
        {
            get { return _factor; }
            set
            {
                if (value == _factor) // no change
                    return;
                else if (value < MINZOOM)
                    value = MINZOOM;
                else if (value > MAXZOOM)
                    value = MAXZOOM;

                if (RawImage != null)
                {
                    picbox.Width = (int) (RawImage.Width * value);
                    picbox.Height = (int) (RawImage.Height * value);
                    AutoScrollMinSize = picbox.Size;
                    picbox.SizeMode = PictureBoxSizeMode.StretchImage;
                    Invalidate();
                }
                _factor = value;
                OnZoomLevelChanged(new ZoomLevelEventArgs(value));
            }
        }

        /// <summary>
        /// Flag indicating the behaviour of this control when the mouse enters it.
        /// If set to true, this control will grab focus when the mouse enters this
        /// control, and return it to the control which previously had focus when the
        /// mouse leaves. This is to enable scrolling / zooming with the mouse wheel
        /// without first having to click on the control to get focus.
        /// </summary>
        [Browsable(true), DefaultValue(false), 
         Category("Behavior"), DisplayName("Grab Focus on MouseEnter"), 
         Description("Grabs the focus on the picture box when the mouse enters the control, "+
         "releasing it to the previous focus owner when the mouse leaves")]
        public bool GrabFocusOnMouseEnter
        {
            get { return _grab; }
            set { _grab = value; }
        }

        #endregion

        #region - Mouse Event Handlers

        /// <summary>
        /// Handles the mouse entering this control by grabbing the focus (if grabbing
        /// is enabled)
        /// </summary>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            GrabFocus();
        }

        /// <summary>
        /// Handles the mouse leaving this control by returning focus to the control
        /// which previously had it before this control grabbed it.
        /// </summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            ReleaseFocus();
        }

        /// <summary>
        /// Handles the mouse entering the contained picture box by bubbling the event
        /// to this control
        /// </summary>
        private void HandlePicboxMouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        /// <summary>
        /// Handles the mouse leaving the contained picture box by bubbling the event
        /// to this control
        /// </summary>
        private void HandlePicboxMouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        /// <summary>
        /// Handles the mousewheel event occuring in this control
        /// </summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                Zoom(e.Delta);
                if (e is HandledMouseEventArgs)
                    ((HandledMouseEventArgs)e).Handled = true;
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        #endregion
        
        #region - Focusing Methods -

        /// <summary>
        /// Grabs the focus to this control if grabbing is enabled, it is not
        /// currently focused and the picture box contains an image.
        /// The control which currently has focus is saved in order to release focus
        /// to it after the mouse leaves.
        /// </summary>
        protected void GrabFocus()
        {
            if (_grab && !Focused && RawImage != null)
            {
                // Save the currently focused control
                _stolenFocusFrom = null;
                Form f = TopLevelControl as Form;
                if (f != null)
                    _stolenFocusFrom = f.ActiveControl;

                Focus();
            }
        }

        /// <summary>
        /// Releases the focus from this control, returning it to the saved control
        /// if grabbing is enabled, the picture box contains an image, this control
        /// is currently focused and we have saved the control from which we grabbed
        /// the focus.
        /// </summary>
        protected void ReleaseFocus()
        {
            if (_grab && _stolenFocusFrom != null && RawImage != null && Focused)
                _stolenFocusFrom.Focus();
            _stolenFocusFrom = null;
        }

        #endregion

        #region - Zooming Methods -

        /// <summary>
        /// Raises the zoom level changed event for this control
        /// </summary>
        /// <param name="e">The arguments detailing the event.</param>
        protected virtual void OnZoomLevelChanged(ZoomLevelEventArgs e)
        {
            if (ZoomLevelChanged != null)
                ZoomLevelChanged(this, e);
        }

        /// <summary>
        /// Zooms this control by the given amount.
        /// </summary>
        /// <param name="delta">The amount by which this control should be zoomed -
        /// this is expected to be direct from a mouse wheel event, where a single
        /// 'click' of the mousewheel gives a delta value of 120.</param>
        private void Zoom(int delta)
        {
            // If there are no steps, don't zoom
            int steps = delta / 120;
            if (steps == 0)
                return;

            // find where we are in our default steps
            int[] between = new int[] { -1, -1 };
            for (int i = 0; i < DefaultZoomLevels.Count; i++)
            {
                // [0] should hold the last index for which the factor
                // exceeded the default zoom level value
                if (_factor >= DefaultZoomLevels[i])
                    between[0] = i;
                // [1] should hold the first index for which the factor
                // no longer exceeds the default zoom level value
                if (_factor <= DefaultZoomLevels[i])
                {
                    between[1] = i;
                    // If we've found this, then by definition we should have
                    // found [0], so we might as well break out of the loop now.
                    break;
                }
            }

            // If we're going up, go from the bottom index, otherwise go
            // from the top index.
            int ind = between[steps > 0 ? 0 : 1];

            // if the step we're applying would take us beyond the default levels,
            // just ignore it - we're at the limit already
            if ((steps < 0 && ind == 0) ||
                (steps > 0 && ind == DefaultZoomLevels.Count - 1))
                return;

            // if the index we're going from is -1 (ie. the current factor is
            // beyond the range of the default zoom levels), just set the new
            // factor to the top / bottom of the range of zoom levels as appropriate
            if (ind < 0)
            {
                ZoomFactor =
                    DefaultZoomLevels[steps > 0 ? 0 : DefaultZoomLevels.Count - 1];
            }
            // Otherwise, add the steps, boundary check and limit to the boundaries
            // and continue
            else
            {
                ind += steps;
                if (ind < 0)
                    ind = 0;
                else if (ind >= DefaultZoomLevels.Count)
                    ind = DefaultZoomLevels.Count - 1;
                ZoomFactor = DefaultZoomLevels[ind];
            }
        }

        #endregion

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZoomingScrollablePictureBox));
            ((System.ComponentModel.ISupportInitialize)(this.picbox)).BeginInit();
            this.SuspendLayout();
            // 
            // ZoomingScrollablePictureBox
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "ZoomingScrollablePictureBox";
            ((System.ComponentModel.ISupportInitialize)(this.picbox)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

