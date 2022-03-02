/*
 * Copyright (c) 2011, wyDay
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of
 * conditions and the following disclaimer.
 * 
 * Redistributions in binary form must reproduce the above copyright notice, this list
 * of conditions and the following disclaimer in the documentation and/or other
 * materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 * 
 * Get the latest version of SplitButton at: http://wyday.com/splitbutton/
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

// need to specify this since there's a ContentAlignment in .VisualStyles too
using ContentAlignment = System.Drawing.ContentAlignment;

namespace AutomateControls
{
    /// <summary>
    /// A button with an associated context menu.
    /// 
    /// Modified version of:
    /// http://wyday.com/blog/2007/splitbutton-in-c-passing-the-aic-test/
    /// 
    /// Itself cribbed from:
    /// http://blogs.msdn.com/b/jfoscoding/archive/2005/11/10/491523.aspx
    /// 
    /// The licence is basically the BSD licence - the full text from the site is
    /// given above, and it will be implemented into the help file under bug 6265
    /// </summary>
    public class SplitButton : Buttons.StandardStyledButton
    {
        #region - Testing Form / Main method -

        [STAThread]
        public static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            using (Form f = new Form())
            {
                f.Controls.Add(new TextBox());
                SplitButton b = new SplitButton();
                ContextMenuStrip strip = new ContextMenuStrip();
                strip.Items.Add(new ToolStripMenuItem("Cut", null, null, Keys.Control | Keys.X));
                strip.Items.Add(new ToolStripMenuItem("Copy", null, null, Keys.Control | Keys.C));
                strip.Items.Add(new ToolStripMenuItem("Paste", null, null, Keys.Control | Keys.X));
                b.ContextMenuStrip = strip;
                b.Text = "Clipboard";
                b.Location = new Point(20, 20);
                b.Click += Clicked;
                f.Controls.Add(b);
                f.Size = new Size(400, 300);
                f.ShowDialog();
            }
        }

        private static void Clicked(object sender, EventArgs e)
        {
            MessageBox.Show("Enclickened!");
        }

        #endregion

        #region - Member Variables -

        // The width of the split section - ie. the section with the drop down arrow
        private const int SplitSectionWidth = 18;

        // The state of this button
        private PushButtonState _state;

        // flag indicating that the next context menu open should be ignored - this
        // kicks in if the user dismisses the current context menu by clicking on
        // the button.
        private bool _skipNextOpen;

        // flag indicating if the menu is currently visible
        private bool _menuVisible;

        // flag to always set the split as visible - redundant if visual styles aren't
        // enable (since the split is always visible then)
        private bool _alwaysShowSplit;

        // The context menu strip to display in this split button on a drop down event
        private ContextMenuStrip _menustrip;

        // Flag indicating if the mouse is currently registered as being inside
        // the bounds of the button (making the button state 'hot')
        private bool _mouseIn;

        // Flag to say always show the menu even if main button body is clicked
        private bool _dropDownOnClick;
        private string _originalText;

        #endregion

        #region - Constructors -

        public SplitButton()
        {
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The context menu strip set for this button.
        /// This override just ensures that the button's ContextMenuStrip property
        /// makes sense in this context. Generally, the <see cref="SplitMenuStrip"/>
        /// property would be used as that is exposed to the designer.
        /// </summary>
        [Browsable(false)]
        public override ContextMenuStrip ContextMenuStrip
        {
            get { return SplitMenuStrip; }
            set { SplitMenuStrip = value; }
        }

        /// <summary>
        /// Always show the split line between text and arrow
        /// </summary>
        [DefaultValue(false)]
        public bool AlwaysShowSplit
        {
            get { return _alwaysShowSplit; }
            set { _alwaysShowSplit = value; }
        }

        /// <summary>
        /// Always drop down when the button is clicked
        /// </summary>
        [Browsable(true), DefaultValue(false),
         Description(
             "Always drop down the context menu on a button click, even if " +
             "not clicked on the drop down arrow")]
        public bool DropDownOnButtonClick
        {
            get { return _dropDownOnClick; }
            set { _dropDownOnClick = value; }
        }

        /// <summary>
        /// The SplitMenu context strip for this split button
        /// </summary>
        [DefaultValue(null)]
        public ContextMenuStrip SplitMenuStrip
        {
            get
            {
                return _menustrip;
            }
            set
            {
                if (_menustrip == value)
                    return;

                //remove the event handlers for the old SplitMenuStrip
                if (_menustrip != null)
                {
                    _menustrip.Closing -= SplitMenuStrip_Closing;
                    _menustrip.Opening -= SplitMenuStrip_Opening;
                }

                //add the event handlers for the new SplitMenuStrip
                if (value != null)
                {
                    value.Closing += SplitMenuStrip_Closing;
                    value.Opening += SplitMenuStrip_Opening;
                }

                _menustrip = value;
                UpdateUI();
            }
        }

        /// <summary>
        /// Flag to indicate if this button has a populated split menu assigned to it
        /// </summary>
        private bool HasSplitMenu
        {
            get { return (_menustrip != null && _menustrip.Items.Count > 0); }
        }

        private PushButtonState State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// The drop down rectangle for this split button
        /// </summary>
        private Rectangle DropDownRectangle
        {
            get
            {
                Rectangle bounds = ClientRectangle;
                return new Rectangle(bounds.Right - SplitSectionWidth, 0,
                    SplitSectionWidth - SystemInformation.Border3DSize.Width, bounds.Height);
            }
        }

        /// <summary>
        /// Whether the button is currently pressed or not
        /// </summary>
        protected bool IsButtonPressed
        {
            get { return State == PushButtonState.Pressed; }
        }

        /// <summary>
        /// Whether the button is currently 'hot' or not
        /// </summary>
        protected bool IsButtonHot
        {
            get { return State == PushButtonState.Hot; }
        }

        /// <summary>
        /// Whether the button is currently disabled or not
        /// </summary>
        protected bool IsButtonDisabled
        {
            get { return State == PushButtonState.Disabled; }
        }

        #endregion Properties

        #region - Overriding Methods -

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData.Equals(Keys.Down) && HasSplitMenu)
                return true;

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs ke)
        {
            base.OnKeyDown(ke);
            if (HasSplitMenu)
            {
                if (ke.KeyCode == Keys.Down && !_menuVisible)
                {
                    ShowContextMenuStrip();
                }
                else if (ke.KeyCode == Keys.Space && ke.Modifiers == Keys.None)
                {
                    State = PushButtonState.Pressed;
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs ke)
        {
            base.OnKeyUp(ke);
            if (ke.KeyCode == Keys.Space)
            {
                if (MouseButtons == MouseButtons.None)
                {
                    State = PushButtonState.Normal;
                }
            }
            else if (ke.KeyCode == Keys.Apps)
            {
                if (MouseButtons == MouseButtons.None && !_menuVisible)
                {
                    ShowContextMenuStrip();
                }
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            State = (Enabled ? PushButtonState.Normal : PushButtonState.Disabled);

            base.OnEnabledChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {

            base.OnGotFocus(e);
            if (HasSplitMenu && !IsButtonPressed && !IsButtonDisabled)
            {
                State = (_mouseIn ? PushButtonState.Hot : PushButtonState.Default);
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (HasSplitMenu && !IsButtonPressed && !IsButtonDisabled)
            {
                State = (_mouseIn ? PushButtonState.Hot : PushButtonState.Normal);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!HasSplitMenu)
                return;

            _mouseIn = true;

            if (!IsButtonPressed && !IsButtonDisabled)
            {
                State = PushButtonState.Hot;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!HasSplitMenu)
                return;

            _mouseIn = false;

            if (!IsButtonPressed && !IsButtonDisabled)
            {
                State = Focused ? PushButtonState.Default : PushButtonState.Normal;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!HasSplitMenu)
                return;
            
            // handle ContextMenu re-clicking the drop-down region to close the menu
            if (HasSplitMenu && e.Button == MouseButtons.Left && !_mouseIn)
                _skipNextOpen = true;

            if (DropDownRectangle.Contains(e.Location) &&
                !_menuVisible && e.Button == MouseButtons.Left)
            {
                ShowContextMenuStrip();
            }
            else
            {
                State = PushButtonState.Pressed;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!HasSplitMenu)
            {
                base.OnMouseUp(e);
                return;
            }

            // if the right button was released inside the button, or
            // the left button was released there and 'DropDownOnButtonClick' is true
            if ((e.Button == MouseButtons.Right || _dropDownOnClick)
                && ClientRectangle.Contains(e.Location) && !_menuVisible)
            {
                ShowContextMenuStrip();
            }
            else if (!HasSplitMenu || !_menuVisible)
            {
                UpdateState();
                if (ClientRectangle.Contains(e.Location) &&
                    !DropDownRectangle.Contains(e.Location))
                {
                    OnClick(EventArgs.Empty);
                }
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (!string.IsNullOrEmpty(Text))
            {
                _originalText = Text;
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (!HasSplitMenu)
            {
                Text = _originalText;
                return;
            }
            Text = string.Empty;

            Graphics g = pe.Graphics;
            Rectangle bounds = ClientRectangle;
            
            // Create a rectangle for the inset bounds - ie. a rectangle defining the
            // 'inner' client area inside the border of the control
            Rectangle insetBounds = bounds;
            int inset = 2 * SystemInformation.Border3DSize.Width;
            insetBounds.Inflate(-inset, -inset);
            
            bool rtl = (RightToLeft == RightToLeft.Yes);

            // calculate the current dropdown rectangle.
            Rectangle dropRect = DropDownRectangle;
            Rectangle focusRect = insetBounds;
            focusRect.Width -= dropRect.Width;
            focusRect.Inflate(1, 1);

            // if we're going right to left, invert the drop / focus rects
            if (rtl)
            {
                dropRect.X = bounds.X + 1;
                focusRect.X = dropRect.Right;
            }

            // if we're drawing the split line
            if (_alwaysShowSplit || !Application.RenderWithVisualStyles
                || State == PushButtonState.Hot || State == PushButtonState.Pressed)
            {
                // rtl = shadow on left, split on right; ltr = vice versa
                int shadowx = (rtl
                    ? bounds.Left + SplitSectionWidth
                    : bounds.Right - SplitSectionWidth);
                int splitx = shadowx + (rtl ? 1 : -1);
                g.DrawLine(SystemPens.ButtonShadow,
                    shadowx, insetBounds.Top, shadowx, insetBounds.Bottom);
                g.DrawLine(SystemPens.ButtonFace,
                    splitx, insetBounds.Top, splitx, insetBounds.Bottom);
            }

            // Draw an arrow in the correct location
            PaintArrow(g, dropRect);

            //paint the image and text in the "button" part of the splitButton
            PaintTextandImage(g, new Rectangle(
                0, 0, bounds.Width - SplitSectionWidth, bounds.Height));
            
            // draw the focus rectangle.
            if (State != PushButtonState.Pressed && Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(g, focusRect);
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size pref = base.GetPreferredSize(proposedSize);

            //autosize correctly for splitbuttons
            string txt = _originalText;
            if (HasSplitMenu && !string.IsNullOrEmpty(txt))
            {
                int txtWidth = TextRenderer.MeasureText(_originalText, Font).Width;

                if (txtWidth + SplitSectionWidth > pref.Width)
                {
                    int padding = SystemInformation.Border3DSize.Width * 4;
                    return pref + new Size(SplitSectionWidth + padding, 0);
                }
            }

            return pref;
        }

        #endregion

        #region - Menu Event Handlers -

        void SplitMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (DesignMode) return;

            _menuVisible = true;
        }

        void SplitMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (DesignMode) return;

            _menuVisible = false;

            UpdateState();

            if (e.CloseReason == ToolStripDropDownCloseReason.AppClicked)
            {
                _skipNextOpen = (DropDownRectangle.Contains(PointToClient(Cursor.Position))) && MouseButtons == MouseButtons.Left;
            }
        }

        void SplitMenu_Popup(object sender, EventArgs e)
        {
            if (DesignMode) return;

            _menuVisible = true;
        }

        #endregion

        #region - Paint Methods -

        private void PaintTextandImage(Graphics g, Rectangle bounds)
        {
            // Figure out where our text and image should go
            Rectangle textRect;
            Rectangle imgRect;

            // If we dont' use mnemonic, set formatFlag to NoPrefix as this will show ampersand.
            TextFormatFlags flags = TextFormatFlags.Default;
            if (!UseMnemonic)
                flags |= TextFormatFlags.NoPrefix;
            else if (!ShowKeyboardCues)
                flags |= TextFormatFlags.HidePrefix;

            CalculateButtonTextAndImageLayout(flags, ref bounds, out textRect, out imgRect);

            //draw the image
            if (Image != null)
            {
                if (Enabled)
                    g.DrawImage(Image, imgRect.X, imgRect.Y, Image.Width, Image.Height);
                else
                    ControlPaint.DrawImageDisabled(g, Image, imgRect.X, imgRect.Y, BackColor);
            }

            //draw the text
            if (!string.IsNullOrEmpty(_originalText))
            {
                if (Enabled)
                    TextRenderer.DrawText(g, _originalText, Font, textRect, ForeColor, flags);
                else
                    ControlPaint.DrawStringDisabled(g, _originalText, Font, BackColor, textRect, flags);
            }
        }

        private void PaintArrow(Graphics g, Rectangle dropDownRect)
        {
            Point middle = new Point(
                Convert.ToInt32(dropDownRect.Left + dropDownRect.Width / 2),
                Convert.ToInt32(dropDownRect.Top + dropDownRect.Height / 2));

            //if the width is odd - favor pushing it over one pixel right.
            middle.X += (dropDownRect.Width % 2);

            Point[] arrow = new Point[] { new Point(middle.X - 2, middle.Y - 1), new Point(middle.X + 3, middle.Y - 1), new Point(middle.X, middle.Y + 2) };

            if (Enabled)
                g.FillPolygon(SystemBrushes.ControlText, arrow);
            else
                g.FillPolygon(SystemBrushes.ButtonShadow, arrow);
        }

        #endregion

        #region - Button Layout Calculation Methods -

        //The following layout functions were taken from Mono's Windows.Forms 
        //implementation, specifically "ThemeWin32Classic.cs", 
        //then modified to fit the context of this splitButton

        private void CalculateButtonTextAndImageLayout(TextFormatFlags flags,
            ref Rectangle contentRect, out Rectangle textRect, out Rectangle imgRect)
        {
            Size textSize = TextRenderer.MeasureText(_originalText, Font, contentRect.Size, flags);
            Size imgSize = Image == null ? Size.Empty : Image.Size;

            textRect = Rectangle.Empty;
            imgRect = Rectangle.Empty;

            switch (TextImageRelation)
            {
                case TextImageRelation.Overlay:
                    // Overlay is easy, text always goes here
                    textRect = OverlayObjectRect(ref contentRect, ref textSize, TextAlign);

                    // Offset on Windows 98 style when button is pressed
                    if (_state == PushButtonState.Pressed && !Application.RenderWithVisualStyles)
                        textRect.Offset(1, 1);

                    // Image is dependent on ImageAlign
                    if (Image != null)
                        imgRect = OverlayObjectRect(ref contentRect, ref imgSize, ImageAlign);

                    break;
                case TextImageRelation.ImageAboveText:
                    contentRect.Inflate(-4, -4);
                    LayoutTextAboveOrBelowImage(contentRect, false, textSize, imgSize, out textRect, out imgRect);
                    break;
                case TextImageRelation.TextAboveImage:
                    contentRect.Inflate(-4, -4);
                    LayoutTextAboveOrBelowImage(contentRect, true, textSize, imgSize, out textRect, out imgRect);
                    break;
                case TextImageRelation.ImageBeforeText:
                    contentRect.Inflate(-4, -4);
                    LayoutTextBeforeOrAfterImage(contentRect, false, textSize, imgSize, out textRect, out imgRect);
                    break;
                case TextImageRelation.TextBeforeImage:
                    contentRect.Inflate(-4, -4);
                    LayoutTextBeforeOrAfterImage(contentRect, true, textSize, imgSize, out textRect, out imgRect);
                    break;
            }
        }

        /// <summary>
        /// Gets the 'centre' for an object of the given size within a container of
        /// the given size. This is actually slightly off centre, just to ensure that
        /// it is consistent with the (internal, unable to call or modify / override
        /// directly) way in which standard Button objects are drawn
        /// </summary>
        /// <param name="containerSize">The size of the container</param>
        /// <param name="objSize">The size of the object</param>
        /// <returns>The centre point to use for drawing the object</returns>
        private static int GetCentre(int containerSize, int objSize)
        {
            return ((containerSize - objSize) / 2) - 1;
        }

        private static Rectangle OverlayObjectRect(ref Rectangle container,
            ref Size sizeOfObject, ContentAlignment alignment)
        {
            int x, y;

            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                    x = 4;
                    y = 4;
                    break;
                case ContentAlignment.TopCenter:
                    x = GetCentre(container.Width, sizeOfObject.Width);
                    y = 4;
                    break;
                case ContentAlignment.TopRight:
                    x = container.Width - sizeOfObject.Width - 4;
                    y = 4;
                    break;
                case ContentAlignment.MiddleLeft:
                    x = 4;
                    y = GetCentre(container.Height, sizeOfObject.Height);
                    break;
                case ContentAlignment.MiddleCenter:
                    x = GetCentre(container.Width, sizeOfObject.Width);
                    y = GetCentre(container.Height, sizeOfObject.Height);
                    break;
                case ContentAlignment.MiddleRight:
                    x = container.Width - sizeOfObject.Width - 4;
                    y = GetCentre(container.Height, sizeOfObject.Height);
                    break;
                case ContentAlignment.BottomLeft:
                    x = 4;
                    y = container.Height - sizeOfObject.Height - 4;
                    break;
                case ContentAlignment.BottomCenter:
                    x = GetCentre(container.Width, sizeOfObject.Width);
                    y = container.Height - sizeOfObject.Height - 4;
                    break;
                case ContentAlignment.BottomRight:
                    x = container.Width - sizeOfObject.Width - 4;
                    y = container.Height - sizeOfObject.Height - 4;
                    break;
                default:
                    x = 4;
                    y = 4;
                    break;
            }

            return new Rectangle(x, y, sizeOfObject.Width, sizeOfObject.Height);
        }

        private void LayoutTextBeforeOrAfterImage(Rectangle totalArea, bool textFirst, Size textSize, Size imageSize, out Rectangle textRect, out Rectangle imageRect)
        {
            int space = 0;  // Spacing between the Text and the Image
            int totalWidth = textSize.Width + space + imageSize.Width;

            if (!textFirst)
                space += 2;

            // If the text is too big, chop it down to the size we have available to it
            if (totalWidth > totalArea.Width)
            {
                textSize.Width = totalArea.Width - space - imageSize.Width;
                totalWidth = totalArea.Width;
            }

            int excess_width = totalArea.Width - totalWidth;
            int offset = 0;

            Rectangle final_text_rect;
            Rectangle final_image_rect;

            HorizontalAlignment h_text = GetHorizontalAlignment(TextAlign);
            HorizontalAlignment h_image = GetHorizontalAlignment(ImageAlign);

            if (h_image == HorizontalAlignment.Left)
                offset = 0;
            else if (h_image == HorizontalAlignment.Right && h_text == HorizontalAlignment.Right)
                offset = excess_width;
            else if (h_image == HorizontalAlignment.Center && (h_text == HorizontalAlignment.Left || h_text == HorizontalAlignment.Center))
                offset += excess_width / 3;
            else
                offset += 2 * (excess_width / 3);

            if (textFirst)
            {
                final_text_rect = new Rectangle(totalArea.Left + offset, AlignInRectangle(totalArea, textSize, TextAlign).Top, textSize.Width, textSize.Height);
                final_image_rect = new Rectangle(final_text_rect.Right + space, AlignInRectangle(totalArea, imageSize, ImageAlign).Top, imageSize.Width, imageSize.Height);
            }
            else
            {
                final_image_rect = new Rectangle(totalArea.Left + offset, AlignInRectangle(totalArea, imageSize, ImageAlign).Top, imageSize.Width, imageSize.Height);
                final_text_rect = new Rectangle(final_image_rect.Right + space, AlignInRectangle(totalArea, textSize, TextAlign).Top, textSize.Width, textSize.Height);
            }

            textRect = final_text_rect;
            imageRect = final_image_rect;
        }

        private void LayoutTextAboveOrBelowImage(Rectangle totalArea, bool textFirst, Size textSize, Size imageSize, out Rectangle textRect, out Rectangle imageRect)
        {
            int element_spacing = 0;    // Spacing between the Text and the Image
            int total_height = textSize.Height + element_spacing + imageSize.Height;

            if (textFirst)
                element_spacing += 2;

            if (textSize.Width > totalArea.Width)
                textSize.Width = totalArea.Width;

            // If the there isn't enough room and we're text first, cut out the image
            if (total_height > totalArea.Height && textFirst)
            {
                imageSize = Size.Empty;
                total_height = totalArea.Height;
            }

            int excess_height = totalArea.Height - total_height;
            int offset = 0;

            Rectangle final_text_rect;
            Rectangle final_image_rect;

            VerticalAlignment v_text = GetVerticalAlignment(TextAlign);
            VerticalAlignment v_image = GetVerticalAlignment(ImageAlign);

            if (v_image == VerticalAlignment.Top)
                offset = 0;
            else if (v_image == VerticalAlignment.Bottom && v_text == VerticalAlignment.Bottom)
                offset = excess_height;
            else if (v_image == VerticalAlignment.Center && (v_text == VerticalAlignment.Top || v_text == VerticalAlignment.Center))
                offset += excess_height / 3;
            else
                offset += 2 * (excess_height / 3);

            if (textFirst)
            {
                final_text_rect = new Rectangle(AlignInRectangle(totalArea, textSize, TextAlign).Left, totalArea.Top + offset, textSize.Width, textSize.Height);
                final_image_rect = new Rectangle(AlignInRectangle(totalArea, imageSize, ImageAlign).Left, final_text_rect.Bottom + element_spacing, imageSize.Width, imageSize.Height);
            }
            else
            {
                final_image_rect = new Rectangle(AlignInRectangle(totalArea, imageSize, ImageAlign).Left, totalArea.Top + offset, imageSize.Width, imageSize.Height);
                final_text_rect = new Rectangle(AlignInRectangle(totalArea, textSize, TextAlign).Left, final_image_rect.Bottom + element_spacing, textSize.Width, textSize.Height);

                if (final_text_rect.Bottom > totalArea.Bottom)
                    final_text_rect.Y = totalArea.Top;
            }

            textRect = final_text_rect;
            imageRect = final_image_rect;
        }

        private static HorizontalAlignment GetHorizontalAlignment(ContentAlignment align)
        {
            switch (align)
            {
                case ContentAlignment.BottomLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.TopLeft:
                    return HorizontalAlignment.Left;
                case ContentAlignment.BottomCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.TopCenter:
                    return HorizontalAlignment.Center;
                case ContentAlignment.BottomRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.TopRight:
                    return HorizontalAlignment.Right;
            }

            return HorizontalAlignment.Left;
        }

        private static VerticalAlignment GetVerticalAlignment(ContentAlignment align)
        {
            switch (align)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    return VerticalAlignment.Top;
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight:
                    return VerticalAlignment.Center;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    return VerticalAlignment.Bottom;
            }

            return VerticalAlignment.Top;
        }

        internal static Rectangle AlignInRectangle(Rectangle outer, Size inner, ContentAlignment align)
        {
            int x = 0;
            int y = 0;

            if (align == ContentAlignment.BottomLeft || align == ContentAlignment.MiddleLeft || align == ContentAlignment.TopLeft)
                x = outer.X;
            else if (align == ContentAlignment.BottomCenter || align == ContentAlignment.MiddleCenter || align == ContentAlignment.TopCenter)
                x = Math.Max(outer.X + ((outer.Width - inner.Width) / 2), outer.Left);
            else if (align == ContentAlignment.BottomRight || align == ContentAlignment.MiddleRight || align == ContentAlignment.TopRight)
                x = outer.Right - inner.Width;
            if (align == ContentAlignment.TopCenter || align == ContentAlignment.TopLeft || align == ContentAlignment.TopRight)
                y = outer.Y;
            else if (align == ContentAlignment.MiddleCenter || align == ContentAlignment.MiddleLeft || align == ContentAlignment.MiddleRight)
                y = outer.Y + (outer.Height - inner.Height) / 2;
            else if (align == ContentAlignment.BottomCenter || align == ContentAlignment.BottomRight || align == ContentAlignment.BottomLeft)
                y = outer.Bottom - inner.Height;

            return new Rectangle(x, y, Math.Min(inner.Width, outer.Width), Math.Min(inner.Height, outer.Height));
        }

        #endregion Button Layout Calculations

        #region - Other Methods -

        private void ShowContextMenuStrip()
        {
            if (_skipNextOpen)
            {
                // we were called because we're closing the context menu strip
                // when clicking the dropdown button.
                _skipNextOpen = false;
                return;
            }

            State = PushButtonState.Pressed;

            if (_menustrip != null)
                _menustrip.Show(this, new Point(0, Height), ToolStripDropDownDirection.BelowRight);
        }

        private void UpdateState()
        {
            if (Bounds.Contains(Parent.PointToClient(Cursor.Position)))
            {
                State = PushButtonState.Hot;
            }
            else if (Focused)
            {
                State = PushButtonState.Default;
            }
            else if (!Enabled)
            {
                State = PushButtonState.Disabled;
            }
            else
            {
                State = PushButtonState.Normal;
            }
        }

        private void UpdateUI()
        {
            Invalidate();
            if (Parent != null)
                Parent.PerformLayout();
        }

        #endregion

    }
}
