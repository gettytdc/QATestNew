using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing.Drawing2D;

namespace AutomateControls
{
    /// <summary>
    /// Control which provides an auto scrolling picture box.
    /// </summary>
    public partial class ScrollablePictureBox : UserControl
    {
        #region - Member Variables / Event Definitions -

        /// <summary>
        /// Event fired when the picture is to be painted
        /// </summary>
        [Category("Appearance")]
        public event PaintEventHandler PicturePaint;

        // The image being painted within this picture box
        private Image _rawImage;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty scrollable picture box
        /// </summary>
        public ScrollablePictureBox()
        {
            InitializeComponent();
            this.SetStyle(
                ControlStyles.Selectable| ControlStyles.OptimizedDoubleBuffer 
                | ControlStyles.AllPaintingInWmPaint, true);
            this.TabStop = true;
            this.BorderStyle = BorderStyle.None;
            picbox.Visible = false;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The border style provided by this picture box. Only here to suppress the
        /// property from the forms designer
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        /// <summary>
        /// The raw image held in the picture box - this is the base image
        /// without any scaling or image alteration performed on it.
        /// </summary>
        protected Image RawImage { get { return _rawImage; } }

        /// <summary>
        /// The image held on the picture box, including any scaling / image
        /// alteration which has been performed on the image.
        /// </summary>
        public Image Image
        {
            get { return picbox.Image; }
            set
            {
                _rawImage = value;
                AutoScrollMinSize = (value != null ? value.Size : Size.Empty);
                picbox.Image = value;
                if (value == null)
                {
                    picbox.Visible = false;
                }
                else
                {
                    picbox.Height = value.Height;
                    picbox.Width = value.Width;
                    picbox.Visible = true;
                }
            }
        }

        #endregion

        #region - Picture Box Event Handlers -

        /// <summary>
        /// Handles a paint event on the picture box by bubbling it to this control's
        /// 'PicturePaint' event
        /// </summary>
        private void picbox_Paint(object sender, PaintEventArgs e)
        {
            OnPicturePaint(e);
        }

        /// <summary>
        /// Handles a mousemove event on the picture box by bubbling it to this
        /// control
        /// </summary>
        private void picbox_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        /// <summary>
        /// Handles a mouseup event on the picture box by bubbling it to this control
        /// </summary>
        private void picbox_MouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        /// <summary>
        /// Handles a mousedown event on the picture box by bubbling it to this
        /// control
        /// </summary>
        private void picbox_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        /// <summary>
        /// Handles the picture box being clicked by passing the event onto this
        /// control
        /// </summary>
        private void picbox_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        #endregion

        #region - Overriding/Virtual Event Handlers -

        /// <summary>
        /// Fires a <see cref="PicturePaint"/> event on this control
        /// </summary>
        protected virtual void OnPicturePaint(PaintEventArgs e)
        {
            if (PicturePaint != null)
                PicturePaint(this, e);
        }

        /// <summary>
        /// Handles the mousewheel event on this control
        /// </summary>
        /// <param name="e">The mouse event args detailing the event.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            ScrollPicture(Control.ModifierKeys == Keys.Shift
                ? ScrollBars.Horizontal : ScrollBars.Vertical, e.Delta);

            if (e is HandledMouseEventArgs)
                ((HandledMouseEventArgs)e).Handled = true;

        }

        /// <summary>
        /// Handles this control being clicked by attaining focus
        /// </summary>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus();
        }

        /// <summary>
        /// Handles the painting of this control. This only paints the background of
        /// the control, leaving the picture box to paint over this background where
        /// appropriate.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = this.ClientRectangle;
            rect.Size = new Size(1200, 1200);

            using (Brush b = new LinearGradientBrush(rect,
                Color.FromArgb(0xdd, 0xee, 0xff), Color.White, 23.0f))
            {
                e.Graphics.FillRectangle(b, rect);
            }

            // fill any remaining with white
            Rectangle dispRect = this.DisplayRectangle;
            if (rect.Width < dispRect.Width)
            {
                using (Brush b = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle(b, new Rectangle(
                        rect.Width, 0, dispRect.Width - rect.Width, dispRect.Height));
                }
            }
            Border3DStyle style = Focused ? Border3DStyle.Bump : Border3DStyle.Etched;
            ControlPaint.DrawBorder3D(e.Graphics, dispRect, style);
            base.OnPaint(e);
        }

        /// <summary>
        /// Handles the mouse entering this control
        /// </summary>
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            Invalidate();
        }

        /// <summary>
        /// Handles the mouse leaving this control
        /// </summary>
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            Invalidate();
        }

        /// <summary>
        /// Handles this control being resized.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Invalidates the picture box wrapped by this control
        /// </summary>
        public void InvalidatePicture()
        {
            picbox.Invalidate();
        }

        /// <summary>
        /// Scrolls the picture within the picture box by the given delta value -
        /// assumed to be that gleaned from mouse wheel events.
        /// </summary>
        /// <param name="which">The scrollbars to scroll</param>
        /// <param name="delta">The delta by which the picture should be scrolled.
        /// </param>
        protected void ScrollPicture(ScrollBars which, int delta)
        {
            delta /= 3;
            bool horiz = (which == ScrollBars.Horizontal);

            // largely nabbed from ScrollableControl (though why on earth MS
            // couldn't have put it into a method and made it callable I
            // do not know).
            Rectangle rect = base.ClientRectangle;
            Rectangle displayRect = base.DisplayRectangle;
            int posn, extra;
            if (horiz)
            {
                posn = -displayRect.X;
                extra = -(rect.Width - displayRect.Width);
            }
            else
            {
                posn = -displayRect.Y;
                extra = -(rect.Height - displayRect.Height);
            }
            posn = Math.Min(Math.Max(posn - delta, 0), extra);
            if (horiz)
                SetDisplayRectLocation(-posn, displayRect.Y);
            else
                SetDisplayRectLocation(displayRect.X, -posn);

            MethodInfo syncScrollbarsMethod = typeof(ScrollableControl).GetMethod(
                       "SyncScrollbars", BindingFlags.NonPublic | BindingFlags.Instance);
            syncScrollbarsMethod.Invoke(this, new object[] { this.AutoScroll });
        }

        #endregion

    }
}
