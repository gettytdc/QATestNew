using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AutomateControls.WindowsSupport;
using BluePrism.Images;
using Win32 = BluePrism.BPCoreLib.modWin32;
using WMsg = AutomateControls.WindowsSupport.WindowsMessage;


namespace AutomateControls
{
    /// <summary>
    /// TextBox which supports the embedding of icons within its client
    /// area. Two images are supported - near and far images, which are
    /// embedded before and after the text editing area, respectively.
    /// 
    /// Alternative images can be specified for the case when the cursor
    /// is hovering over one of the image areas, and events exist for
    /// capturing when the user has entered, left, moved or clicked on
    /// one of the images.
    /// 
    /// This extends GuidanceTextBox, so guidance can be set in the
    /// textbox as necessary.
    /// </summary>
    public class IconTextBox : GuidanceTextBox
    {
        #region - Published Events -

        /// <summary>
        /// Event fired when the mouse enters the area in which the near image
        /// is being displayed
        /// </summary>
        [Category("Mouse"),
         Description("Occurs when the mouse enters the near image bounds")]
        public event EventHandler NearImageMouseEnter;

        /// <summary>
        /// Event fired when the mouse leaves the are in which the near image
        /// is being displayed
        /// </summary>
        [Category("Mouse"),
         Description("Occurs when the mouse leaves the near image bounds")]
        public event EventHandler NearImageMouseLeave;

        /// <summary>
        /// Event fired when the user clicks on the near image.
        /// </summary>
        [Category("Mouse"),
         Description("Occurs when the mouse clicks within the near image bounds")]
        public event MouseEventHandler NearImageMouseClick;

        /// <summary>
        /// Event fired when the mouse enters the area in which the far image
        /// is being displayed
        /// </summary>
        [Category("Mouse"),
         Description("Occurs when the mouse enters the far image bounds")]
        public event EventHandler FarImageMouseEnter;

        /// <summary>
        /// Event fired when the mouse leaves the are in which the far image
        /// is being displayed
        /// </summary>
        [Category("Mouse"),
         Description("Occurs when the mouse leaves the far image bounds")]
        public event EventHandler FarImageMouseLeave;

        /// <summary>
        /// Event fired when the user clicks on the far image.
        /// </summary>
        [Category("Mouse"),
         Description("Occurs when the mouse clicks within the far image bounds")]
        public event MouseEventHandler FarImageMouseClick;

        #endregion

        #region - Class Scope Declarations -

        /// <summary>
        /// The offset to apply to the cursor location when displaying a tooltip
        /// </summary>
        private static readonly Size TipOffset = new Size(16, 16);

        /// <summary>
        /// Enumeration of the mouseover states.
        /// </summary>
        private enum MouseOverState
        {
            /// <summary>
            /// Mouse is not over any image
            /// </summary>
            None,

            /// <summary>
            /// Mouse is over the near image
            /// </summary>
            Near,

            /// <summary>
            /// Mouse is over the far image
            /// </summary>
            Far
        }


        #endregion

        #region - Member Variables -

        // The list of images supported in this texbox.
        private ImageList _images = null;

        // The default image index for the near image
        private int _nearImageDefault = -1;

        // The hover image index for the near image
        private int _nearImageHover = -1;

        // The default image index for the far image
        private int _farImageDefault = -1;

        // The hover image index for the near image
        private int _farImageHover = -1;

        // The mouseover state - ie. which image (if any) is being hovered over
        private MouseOverState _state;

        // The tooltip text for the near image in the textbox
        private string _nearTip;

        // The tooltip text for the far image in the textbox
        private string _farTip;

        // The tooltip, created when any tooltip text is set
        private ToolTip _tip;

        // Flag to indicate that the hand cursor should always be shown when the
        // user is hovering over the near image
        private bool _handOnHoverNear;

        // Flag to indicate that the hand cursor should always be shown when the
        // user is hovering over the far image
        private bool _handOnHoverFar;

        #endregion

        #region - Private Properties -

        /// <summary>
        /// Gets the inner margins of this TextBox control
        /// </summary>
        /// <returns>The value of the inner margins for this control.</returns>
        private Padding InnerMargins
        {
            get
            {
                var margin = Win32.SendMessage(Handle, WMsg.EM_GETMARGINS, 0, 0).ToInt32();
                return new Padding(margin & 0xFFFF, 0, margin >> 16, 0);
            }
            set
            {
                var margin = (value.Left & 0xFFFF) | (value.Right << 16);
                Win32.SendMessage(Handle, WMsg.EM_SETMARGINS, WMsg.EC_LEFTMARGIN | WMsg.EC_RIGHTMARGIN, margin);
            }
        }

        /// <summary>
        /// The margin in the textbox to accommodate the near image
        /// </summary>
        private Rectangle NearImageRectangle
        {
            get
            {
                if (!IsNearImageSet())
                {
                    return Rectangle.Empty;
                }

                return (IsRightToLeft
                    ? GetRightImageRectangle(_images.ImageSize)
                    : GetLeftImageRectangle(_images.ImageSize)
                );
            }
        }

        /// <summary>
        /// The margin in the textbox to accomodate the far image
        /// </summary>
        private Rectangle FarImageRectangle
        {
            get
            {
                if (!IsFarImageSet())
                {
                    return Rectangle.Empty;
                }

                return (IsRightToLeft
                    ? GetLeftImageRectangle(_images.ImageSize)
                    : GetRightImageRectangle(_images.ImageSize)
                );
            }
        }

        /// <summary>
        /// Whether this textbox is currently configured as right to left.
        /// This will default to False if the control is currently set to
        /// <see cref="RightToLeft.Inherit"/> and no control in the hierarchy
        /// has a concrete RightToLeft value set.
        /// </summary>
        private bool IsRightToLeft
        {
            get
            {
                Control c = this;
                RightToLeft rtl = this.RightToLeft;
                while (c != null && rtl == RightToLeft.Inherit)
                {
                    c = c.Parent;
                    rtl = c.RightToLeft;
                }
                // The only 'RightToLeft is on' value is Yes - if it's No or
                // it's Inherit and we ran out of parents to inherit from, we
                // assume that it is not right to left (since that is the default)
                return (rtl == RightToLeft.Yes);
            }
        }

        /// <summary>
        /// The state regarding the image which is currently being hovered over.
        /// </summary>
        private MouseOverState ImageMouseState
        {
            get { return _state; }
            set
            {
                // If there's no change, ignore the call
                if (_state == value)
                {
                    return;
                }

                // Make the change first.
                MouseOverState prevState = _state;
                _state = value;

                // Handle the events in the state change
                switch (prevState)
                {
                    case MouseOverState.Far: OnFarImageMouseLeave(EventArgs.Empty); break;
                    case MouseOverState.Near: OnNearImageMouseLeave(EventArgs.Empty); break;
                }
                switch (value)
                {
                    case MouseOverState.Far: OnFarImageMouseEnter(EventArgs.Empty); break;
                    case MouseOverState.Near: OnNearImageMouseEnter(EventArgs.Empty); break;
                }
            }
        }

        #endregion

        #region - Public Properties -

        /// <summary>
        /// The list of images to use in the margins of this textbox.
        /// </summary>
        [Description("The list of images to use for the hosted image")]
        public ImageList Images
        {
            get { return _images; }
            set { _images = value; UpdateMargins(); }
        }

        /// <summary>
        /// The near image index currently in use
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(null),
         Description("The icon to display at the near end of the textbox")]
        public int NearImage
        {
            get { return (_state == MouseOverState.Near ? NearImageHover : NearImageDefault); }
        }

        /// <summary>
        /// The far image index currently in use
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(null),
         Description("The icon to display at the far end of the textbox")]
        public int FarImage
        {
            get { return (_state == MouseOverState.Far ? FarImageHover : FarImageDefault); }
        }

        /// <summary>
        /// The near image index to use by default - this is the image used if
        /// the near image is not being hovered over, or if no specific hover
        /// index is set for the near image.
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(-1), Description(
            "The index of the default image to use on the near side of the text box")]
        public int NearImageDefault
        {
            get { return _nearImageDefault; }
            set { SetImage(value, ref _nearImageDefault, NearImageRectangle); }
        }

        /// <summary>
        /// The far image index to use by default - this is the image used if
        /// the far image is not being hovered over, or if no specific hover
        /// index is set for the far image.
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(-1), Description(
            "The index of the default image to use on the far side of the text box")]
        public int FarImageDefault
        {
            get { return _farImageDefault; }
            set { SetImage(value, ref _farImageDefault, FarImageRectangle); }
        }

        /// <summary>
        /// The near image index to use when the image is hovered over by the
        /// mouse cursor. Setting this to a value other than -1 or the default
        /// near image index will cause the cursor to become a hand cursor
        /// when the image is being hovered over.
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(-1), Description(
            "The index of the image to use when the mouse is hovering over the "+
            "near image. A value of -1 will use the default image")]
        public int NearImageHover
        {
            get { return (_nearImageHover == -1 ? _nearImageDefault : _nearImageHover); }
            set { _nearImageHover = value; }
        }

        /// <summary>
        /// The far image index to use when the image is hovered over by the
        /// mouse cursor. Setting this to a value other than -1 or the default
        /// far image index will cause the cursor to become a hand cursor
        /// when the image is being hovered over.
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(-1), Description(
            "The index of the image to use when the mouse is hovering over the "+
            "far image. A value of -1 will use the default image")]
        public int FarImageHover
        {
            get { return (_farImageHover == -1 ? _farImageDefault : _farImageHover); }
            set { _farImageHover = value; }
        }

        /// <summary>
        /// The image key to use for the near image, by default
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NearImageDefaultKey
        {
            get { return Images.GetKey(NearImageDefault); }
            set { NearImageDefault = Images.GetIndex(value); }
        }

        /// <summary>
        /// The image key to use for the far image, by default
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FarImageDefaultKey
        {
            get { return Images.GetKey(FarImageDefault); }
            set { FarImageDefault = Images.GetIndex(value); }
        }

        /// <summary>
        /// The image key to use for the near image, when hovering
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NearImageHoverKey
        {
            get { return Images.GetKey(NearImageHover); }
            set { NearImageHover = Images.GetIndex(value); }
        }

        /// <summary>
        /// The image key to use for the far image, when hovering
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FarImageHoverKey
        {
            get { return Images.GetKey(FarImageHover); }
            set { FarImageHover = Images.GetIndex(value); }
        }

        /// <summary>
        /// Gets or sets whether the hand cursor should always be shown when the user
        /// hovers over the near image. By default, it will only be shown if the
        /// hover image is different to the default image.
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(false), Description(
            "Always show the hand cursor when hovering over near image")]
        public bool AlwaysShowHandOnNearHover
        {
            get { return _handOnHoverNear; }
            set { _handOnHoverNear = value; }
        }

        /// <summary>
        /// Gets or sets whether the hand cursor should always be shown when the user
        /// hovers over the far image. By default, it will only be shown if the
        /// hover image is different to the default image.
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(false), Description(
            "Always show the hand cursor when hovering over far image")]
        public bool AlwaysShowHandOnFarHover
        {
            get { return _handOnHoverFar; }
            set { _handOnHoverFar = value; }
        }

        /// <summary>
        /// Gets or sets the tooltip to display when the near icon is hovering
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue((string)null),
         Description("The tooltip to display when the near icon is hovered over")]
        public string NearTip
        {
            get { return _nearTip; }
            set
            {
                // normalise the value - empty string or just whitespace => null
                if (value != null)
                {
                    value = value.Trim();
                }

                if (value == "")
                {
                    value = null;
                }

                if (_nearTip != value)
                {
                    _nearTip = value;
                    CreateToolTip();
                }
            }
        }

        /// <summary>
        /// Gets or sets the tooltip to display when the far icon is hovering
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue((string)null),
         Description("The tooltip to display when the far icon is hovered over")]
        public string FarTip
        {
            get { return _farTip; }
            set
            {
                // normalise the value - empty string or just whitespace => null
                if (value != null)
                {
                    value = value.Trim();
                }

                if (value == "")
                {
                    value = null;
                }

                if (_farTip != value)
                {
                    _farTip = value;
                    CreateToolTip();
                }
            }
        }

        #endregion

        #region - Overriding Methods (Mostly Event Handlers) -

        /// <summary>
        /// Handles window messages, ensuring that the margins on the textbox
        /// are set correctly after a <see cref="WMsg.WM_SETFONT">WM_SETFONT</see>
        /// message and that the images are painted correctly after any of the
        /// various paint-causing messages is sent to this control
        /// </summary>
        /// <param name="m">The windows message to be processed</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch (m.Msg)
            {
                case WMsg.WM_SETFONT:
                    UpdateMargins();
                    break;

                // Any other case in the list below requires a repaint
                case WMsg.WM_PAINT:

                case WMsg.WM_SETFOCUS:
                case WMsg.WM_KILLFOCUS:

                case WMsg.WM_LBUTTONDOWN:
                case WMsg.WM_RBUTTONDOWN:
                case WMsg.WM_MBUTTONDOWN:

                case WMsg.WM_LBUTTONUP:
                case WMsg.WM_RBUTTONUP:
                case WMsg.WM_MBUTTONUP:

                case WMsg.WM_LBUTTONDBLCLK:
                case WMsg.WM_RBUTTONDBLCLK:
                case WMsg.WM_MBUTTONDBLCLK:

                case WMsg.WM_KEYDOWN:
                case WMsg.WM_CHAR:
                case WMsg.WM_KEYUP:

                    Repaint();
                    break;

                case WMsg.WM_MOUSEMOVE:
                    if (m.WParam != IntPtr.Zero)
                    {
                        Repaint();
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the window handle being created in this textbox, ensuring
        /// that the margins are set in the control appropriately.
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateMargins();
        }

        /// <summary>
        /// Handles the mouse leaving this control, ensuring that the image
        /// mouse state is set to not be hovering over either image.
        /// </summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            ImageMouseState = MouseOverState.None;
        }

        /// <summary>
        /// Handles the mouse moving over this control, by checking its
        /// location and setting the image mouse state accordingly.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (NearImageRectangle.Contains(e.Location))
            {
                ImageMouseState = MouseOverState.Near;
            }
            else if (FarImageRectangle.Contains(e.Location))
            {
                ImageMouseState = MouseOverState.Far;
            }
            else
            {
                ImageMouseState = MouseOverState.None;
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Handles mouse clicks on this textbox, ensuring that any clicks
        /// on either of the set images is converted into the appropriate
        /// OnXXXImageClick() event.
        /// </summary>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            switch (_state)
            {
                case MouseOverState.Far: OnFarImageClick(e); return;
                case MouseOverState.Near: OnNearImageClick(e); return;
            }
            base.OnMouseClick(e);
        }

        #endregion

        #region - Overridable Event Handler Methods -

        /// <summary>
        /// Handler for the mouse leaving the near image.
        /// </summary>
        protected virtual void OnNearImageMouseLeave(EventArgs e)
        {
            if (_handOnHoverNear || NearImageHover != NearImageDefault)
            {
                Invalidate(NearImageRectangle);
            }

            Cursor = Cursors.IBeam;
            if (_tip != null)
            {
                _tip.Hide(this);
            }

            FireEvent(NearImageMouseLeave, e);
        }

        /// <summary>
        /// Handler for the mouse entering the near image.
        /// </summary>
        protected virtual void OnNearImageMouseEnter(EventArgs e)
        {
            if (_handOnHoverNear || NearImageHover != NearImageDefault)
            {
                Invalidate(NearImageRectangle);
                Cursor = Cursors.Hand;
            }
            if (_tip != null && _nearTip != null)
            {
                _tip.Show(_nearTip, this, PointToClient(Cursor.Position) + TipOffset);
            }

            FireEvent(NearImageMouseEnter, e);
        }

        /// <summary>
        /// Handler for the mouse leaving the far image.
        /// </summary>
        protected virtual void OnFarImageMouseLeave(EventArgs e)
        {
            if (_handOnHoverFar || FarImageHover != FarImageDefault)
            {
                Invalidate(FarImageRectangle);
            }

            Cursor = Cursors.IBeam;
            if (_tip != null)
            {
                _tip.Hide(this);
            }

            FireEvent(FarImageMouseLeave, e);
        }

        /// <summary>
        /// Handler for the mouse entering the far image.
        /// </summary>
        protected virtual void OnFarImageMouseEnter(EventArgs e)
        {
            if (_handOnHoverFar || FarImageHover != FarImageDefault)
            {
                Invalidate(FarImageRectangle);
                Cursor = Cursors.Hand;
            }
            if (_tip != null && _farTip != null)
            {
                _tip.Show(_farTip, this, PointToClient(Cursor.Position + TipOffset));
            }

            FireEvent(FarImageMouseEnter, e);
        }

        /// <summary>
        /// Handler for the mouse click event on the near image.
        /// </summary>
        protected virtual void OnNearImageClick(MouseEventArgs e)
        {
            FireEvent(NearImageMouseClick, e);
        }

        /// <summary>
        /// Handler for the mouse click event on the far image.
        /// </summary>
        protected virtual void OnFarImageClick(MouseEventArgs e)
        {
            FireEvent(FarImageMouseClick, e);
        }

        #endregion

        #region - Appearance Update Methods -

        /// <summary>
        /// Sets the given image index to the specified value, invalidating the
        /// supplied rectangle area if necessary.
        /// </summary>
        /// <param name="value">The value to set the image index to.</param>
        /// <param name="imgIndex">The reference to the image index which should
        /// be set.</param>
        /// <param name="displayRect">The rectangle which may need invalidating
        /// if the image index is actually changing.</param>
        private void SetImage(int value, ref int imgIndex, Rectangle displayRect)
        {
            int curr = imgIndex;
            imgIndex = value;
            UpdateMargins();
            if (curr != value)
            {
                Invalidate(displayRect);
            }
        }

        /// <summary>
        /// Updates the margins on this textbox, catering for any images which
        /// are currently set within it.
        /// </summary>
        private void UpdateMargins()
        {
            if (IsHandleCreated && IsAnyImageSet())
            {
                var margin = InnerMargins;
                var saved = margin;

                var padding = _images.ImageSize.Width + 4;
                var rtl = IsRightToLeft;

                if (IsNearImageSet())
                {
                    if (rtl)
                    {
                        margin.Right = padding;
                    }
                    else
                    {
                        margin.Left = padding;
                    }
                }

                if (IsFarImageSet())
                {
                    if (rtl)
                    {
                        margin.Left = padding;
                    }
                    else
                    {
                        margin.Right = padding;
                    }
                }

                if (margin != saved)
                {
                    InnerMargins = margin;
                }
            }
        }

        /// <summary>
        /// Optionally paints the specified image within the given rectangle.
        /// </summary>
        /// <param name="g">The graphics context on which to paint the image.
        /// </param>
        /// <param name="isImageSet">flag indicating if this image is set or
        /// not. Since this method doesn't have the context to test whether the
        /// near or far image is set, the calling code must do so. Setting this
        /// to true paints the image, false paints </param>
        /// <param name="imageIndex"></param>
        /// <param name="r"></param>
        private void PaintImage(Graphics g, bool isImageSet, int imageIndex, Rectangle r)
        {
            if (_images == null)
            {
                return;
            }

            using (Brush b = new SolidBrush(BackColor))
            {
                g.FillRectangle(b, r);
            }

            if (isImageSet)
            {
                g.SetClip(r);
                g.DrawImageUnscaledAndClipped(_images.Images[imageIndex], r);
                g.ResetClip();
            }
        }

        /// <summary>
        /// Paints the control if necessary:
        /// </summary>
        private void Repaint()
        {
            // If we have an image to paint
            if (IsAnyImageSet())
            {
                using (DeviceContext dc = new DeviceContext(this.Handle))
                {
                    Graphics g = dc.Graphics;
                    PaintImage(g, IsNearImageSet(), NearImage, NearImageRectangle);
                    PaintImage(g, IsFarImageSet(), FarImage, FarImageRectangle);
                }
            }
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Creates the tooltip if one is necessary - this is one way for this
        /// control; it creates one if a tooltip string is set and only disposes of
        /// it when this control is disposed of itself.
        /// </summary>
        private void CreateToolTip()
        {
            // If we have no tooltip, ensure we want to keep it that way
            if (_tip == null && (_nearTip != null || _farTip != null))
            {
                _tip = new ToolTip();
            }
        }

        /// <summary>
        /// Fires the event on the given event handler using the specified args.
        /// </summary>
        /// <param name="handler">The handler for the event, or null if there is
        /// no handler for the event</param>
        /// <param name="e">The args to fire the event with</param>
        private void FireEvent(EventHandler handler, EventArgs e)
        {
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Fires the event on the given event handler using the specified args.
        /// </summary>
        /// <param name="handler">The handler for the event, or null if there is
        /// no handler for the event</param>
        /// <param name="e">The args to fire the event with</param>
        private void FireEvent(MouseEventHandler handler, MouseEventArgs e)
        {
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Checks if the given image index value is valid with the currently
        /// set image list.
        /// </summary>
        /// <param name="value">The value to check to see if it is a valid
        /// image index within the currently set image list value.</param>
        /// <returns>true if the given value represents an image in the
        /// currently set image list; false otherwise.</returns>
        private bool IsValidImageIndex(int value)
        {
            return (_images != null && value >= 0 && value < _images.Images.Count);
        }

        /// <summary>
        /// Checks if the near image is set in this textbox or not.
        /// </summary>
        /// <returns>True if the near image is set in this textbox; false
        /// otherwise.</returns>
        private bool IsNearImageSet()
        {
            return IsValidImageIndex(NearImage);
        }

        /// <summary>
        /// Checks if the far image is set in this textbox or not.
        /// </summary>
        /// <returns>True if the far image is set in this textbox; false
        /// otherwise.</returns>
        private bool IsFarImageSet()
        {
            return IsValidImageIndex(FarImage);
        }

        /// <summary>
        /// Checks if any image is set in this textbox or not.
        /// </summary>
        /// <returns>True if any image is set in this textbox; false
        /// otherwise.</returns>
        private bool IsAnyImageSet()
        {
            return (IsNearImageSet() || IsFarImageSet());
        }

        /// <summary>
        /// Gets the rectangle in which images of the given size can be
        /// drawn in the left of the textbox.
        /// </summary>
        /// <param name="imgSize">The image size</param>
        /// <returns>A rectangle in which images of the given size can
        /// be drawn in the left margin of the textbox.</returns>
        public Rectangle GetLeftImageRectangle(Size imgSize)
        {
            Rectangle r = ClientRectangle;
            return new Rectangle(new Point(2, r.Top + ((r.Height - imgSize.Height) / 2)), imgSize);
        }

        /// <summary>
        /// Gets the rectangle in which images of the given size can be
        /// drawn in the right of the textbox.
        /// </summary>
        /// <param name="imgSize">The image size</param>
        /// <returns>A rectangle in which images of the given size can
        /// be drawn in the right margin of the textbox.</returns>
        public Rectangle GetRightImageRectangle(Size imgSize)
        {
            Rectangle r = ClientRectangle;
            return new Rectangle(
                new Point(r.Right - imgSize.Width - 2, r.Top + ((r.Height - imgSize.Height) / 2)),
                imgSize
            );
        }

        #endregion

    }
}
