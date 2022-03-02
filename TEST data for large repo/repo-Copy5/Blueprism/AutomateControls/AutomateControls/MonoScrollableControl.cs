// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//  Peter Bartok    pbartok@novell.com
//


// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AutomateControls
{
    //[Designer ("MonoSWF.Design.ScrollableControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
    public class MonoScrollableControl : Control
    {
        #region Local Variables
        private bool hscroll_visible;
        private bool vscroll_visible;
        private bool force_hscroll_visible;
        private bool force_vscroll_visible;
        private bool auto_scroll;
        private Size auto_scroll_margin;
        private Size auto_scroll_min_size;
        protected Point scroll_position;
        private DockPaddingEdges dock_padding;
        private AutomateControls.SizeGrip sizegrip;
        private HScrollBar hscrollbar;
        private VScrollBar vscrollbar;
        private Size canvas_size = Size.Empty;
        private Rectangle display_rectangle = Rectangle.Empty;
        #endregion  // Local Variables

        [TypeConverter(typeof(MonoScrollableControl.DockPaddingEdgesConverter))]
        #region Subclass DockPaddingEdges
        public class DockPaddingEdges : ICloneable
        {
            #region DockPaddingEdges Local Variables
            private int all;
            private int left;
            private int right;
            private int top;
            private int bottom;
            private Control owner;
            #endregion  // DockPaddingEdges Local Variables

            #region DockPaddingEdges Constructor
            internal DockPaddingEdges(Control owner)
            {
                all = 0;
                left = 0;
                right = 0;
                top = 0;
                bottom = 0;
                this.owner = owner;
            }
            #endregion  // DockPaddingEdges Constructor

            #region DockPaddingEdges Public Instance Properties
            [RefreshProperties(RefreshProperties.All)]
            public int All
            {
                get
                {
                    return all;
                }

                set
                {
                    all = value;
                    left = value;
                    right = value;
                    top = value;
                    bottom = value;

                    owner.PerformLayout();
                }
            }

            [RefreshProperties(RefreshProperties.All)]
            public int Bottom
            {
                get
                {
                    return bottom;
                }

                set
                {
                    bottom = value;
                    all = 0;

                    owner.PerformLayout();
                }
            }

            [RefreshProperties(RefreshProperties.All)]
            public int Left
            {
                get
                {
                    return left;
                }

                set
                {
                    left = value;
                    all = 0;

                    owner.PerformLayout();
                }
            }

            [RefreshProperties(RefreshProperties.All)]
            public int Right
            {
                get
                {
                    return right;
                }

                set
                {
                    right = value;
                    all = 0;

                    owner.PerformLayout();
                }
            }

            [RefreshProperties(RefreshProperties.All)]
            public int Top
            {
                get
                {
                    return top;
                }

                set
                {
                    top = value;
                    all = 0;

                    owner.PerformLayout();
                }
            }
            #endregion  // DockPaddingEdges Public Instance Properties

            // Public Instance Methods
            public override bool Equals(object other)
            {
                if (!(other is DockPaddingEdges))
                {
                    return false;
                }

                if ((this.all == ((DockPaddingEdges)other).all) && (this.left == ((DockPaddingEdges)other).left) &&
                    (this.right == ((DockPaddingEdges)other).right) && (this.top == ((DockPaddingEdges)other).top) &&
                    (this.bottom == ((DockPaddingEdges)other).bottom))
                {
                    return true;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return all * top * bottom * right * left;
            }

            public override string ToString()
            {
                return "All = " + all.ToString() + " Top = " + top.ToString() + " Left = " + left.ToString() + " Bottom = " + bottom.ToString() + " Right = " + right.ToString();
            }

            internal void Scale(float dx, float dy)
            {
                left = (int)(left * dx);
                right = (int)(right * dx);
                top = (int)(top * dy);
                bottom = (int)(bottom * dy);
            }

            object ICloneable.Clone()
            {
                DockPaddingEdges padding_edge;

                padding_edge = new DockPaddingEdges(owner);

                padding_edge.all = all;
                padding_edge.left = left;
                padding_edge.right = right;
                padding_edge.top = top;
                padding_edge.bottom = bottom;

                return padding_edge;
            }
        }
        #endregion  // Subclass DockPaddingEdges

        #region Subclass DockPaddingEdgesConverter
        public class DockPaddingEdgesConverter : System.ComponentModel.TypeConverter
        {
            // Public Constructors
            public DockPaddingEdgesConverter()
            {
            }

            // Public Instance Methods
            public override PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(DockPaddingEdges), attributes);
            }

            public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context)
            {
                return true;
            }
        }
        #endregion  // Subclass DockPaddingEdgesConverter

        #region Public Constructors
        public MonoScrollableControl()
        {
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, false);
            auto_scroll = false;
            hscroll_visible = false;
            vscroll_visible = false;
            force_hscroll_visible = false;
            force_vscroll_visible = false;
            auto_scroll_margin = new Size(0, 0);
            auto_scroll_min_size = new Size(0, 0);
            scroll_position = new Point(0, 0);
            dock_padding = new DockPaddingEdges(this);
            SizeChanged += new EventHandler(Recalculate);
            VisibleChanged += new EventHandler(Recalculate);
        }
        #endregion  // Public Constructors

        #region Protected Static Fields
        protected const int ScrollStateAutoScrolling = 1;
        protected const int ScrollStateFullDrag = 16;
        protected const int ScrollStateHScrollVisible = 2;
        protected const int ScrollStateUserHasScrolled = 8;
        protected const int ScrollStateVScrollVisible = 4;
        #endregion  // Protected Static Fields

        #region Public Instance Properties
        [DefaultValue(false)]
        [Localizable(true)]
        //[MWFCategory("Layout")]
        public virtual bool AutoScroll
        {
            get
            {
                return auto_scroll;
            }

            set
            {
                bool VScrollWasVisible = vscroll_visible;
                bool HScrollWasVisible = hscroll_visible;

                if (auto_scroll == value)
                {
                    return;
                }

                auto_scroll = value;
                if (!auto_scroll)
                {
                    SuspendLayout();

                    Controls.Remove(hscrollbar);
                    hscrollbar.Dispose();
                    hscrollbar = null;
                    hscroll_visible = false;

                    Controls.Remove(vscrollbar);
                    vscrollbar.Dispose();
                    vscrollbar = null;
                    vscroll_visible = false;

                    Controls.Remove(sizegrip);
                    sizegrip.Dispose();
                    sizegrip = null;

                    ResumeLayout();
                }
                else
                {
                    SuspendLayout();

                    hscrollbar = new HScrollBar();
                    hscrollbar.Visible = false;
                    hscrollbar.ValueChanged += new EventHandler(HandleScrollValueChanged);
                    hscrollbar.Scroll += new ScrollEventHandler(HandleHorizScrollBar);
                    hscrollbar.Height = SystemInformation.HorizontalScrollBarHeight;
                    this.Controls.Add(hscrollbar);
                    hscrollbar.BringToFront();

                    vscrollbar = new VScrollBar();
                    vscrollbar.Visible = false;
                    vscrollbar.ValueChanged += new EventHandler(HandleScrollValueChanged);
                    vscrollbar.Scroll += new ScrollEventHandler(HandleVertScrollBar);
                    vscrollbar.Width = SystemInformation.VerticalScrollBarWidth;
                    this.Controls.Add(vscrollbar);
                    vscrollbar.BringToFront();

                    sizegrip = new SizeGrip();
                    sizegrip.Visible = false;
                    sizegrip.IsEnabled = false;
                    sizegrip.ShowGrip = false;
                    this.Controls.Add(sizegrip);

                    ResumeLayout();
                }

                // Raise event if scrollbar visibility change
                if (vscroll_visible != VScrollWasVisible)
                {
                    if (this.VScrollChanged != null)
                    {
                        this.VScrollChanged(this, EventArgs.Empty);
                    }
                }
                if (hscroll_visible != HScrollWasVisible)
                {
                    if (this.HScrollChanged != null)
                    {
                        this.HScrollChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        [Localizable(true)]
        //[MWFCategory("Layout")]
        public Size AutoScrollMargin
        {
            get
            {
                return auto_scroll_margin;
            }

            set
            {
                if (value.Width < 0)
                {
                    throw new ArgumentException("Width is assigned less than 0", "value.Width");
                }

                if (value.Height < 0)
                {
                    throw new ArgumentException("Height is assigned less than 0", "value.Height");
                }

                auto_scroll_margin = value;
            }
        }

        [Localizable(true)]
        //[MWFCategory("Layout")]
        public Size AutoScrollMinSize
        {
            get
            {
                return auto_scroll_min_size;
            }

            set
            {
                auto_scroll_min_size = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point AutoScrollPosition
        {
            get
            {
                return new Point(-scroll_position.X, -scroll_position.Y);
            }

            set
            {
                if ((value.X != hscrollbar.Value) || (value.Y != vscrollbar.Value))
                {
                    int shift_x;
                    int shift_y;

                    shift_x = 0;
                    shift_y = 0;
                    if (hscroll_visible)
                    {
                        shift_x = value.X - hscrollbar.Value;
                    }

                    if (vscroll_visible)
                    {
                        shift_y = value.Y - vscrollbar.Value;
                    }

                    ScrollWindow(shift_x, shift_y);

                    if (hscroll_visible)
                    {
                        hscrollbar.Value = value.X;
                    }

                    if (vscroll_visible)
                    {
                        vscrollbar.Value = value.Y;
                    }

                }
            }
        }

        /// <summary>
        /// Restores the window position so that it matches 
        /// the current scroll position
        /// </summary>
        public void RestoreScroll()
        {
            ScrollWindow(hscrollbar.Value,vscrollbar.Value);
        }

        private int m_ScrollIncrement;
        private bool m_ScrollIncrementIsUserDefined;
        /// <summary>
        /// The number of pixels by which to scroll the
        /// control vertically, when the mouse wheel is 
        /// scrolled.
        /// </summary>
        /// <remarks>A value of zero will cause the default
        /// value to be used. This default value is the height
        /// of the control, meaning that 'one page' will be
        /// scrolled with a single mouse wheel change.</remarks>
        public int MouseScrollIncrement
        {
            get
            {
                return m_ScrollIncrement;
            }
            set
            {
                m_ScrollIncrement = value;
                m_ScrollIncrementIsUserDefined = (m_ScrollIncrement != 0);
            }
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                if (!auto_scroll)
                {
                    return base.DisplayRectangle;
                }

                int width;
                int height;
                Rectangle BaseRect = base.DisplayRectangle;

                    width = BaseRect.Width;
                    if (vscroll_visible)
                    {
                        width -= vscrollbar.Width;
                    }

                height = BaseRect.Height;
                    if (hscroll_visible)
                    {
                        height -= hscrollbar.Height;
                    }

                display_rectangle.X = -scroll_position.X + dock_padding.Left;
                display_rectangle.Y = -scroll_position.Y + dock_padding.Top;
                display_rectangle.Width = Math.Max(auto_scroll_min_size.Width, width) - dock_padding.Left - dock_padding.Right;
                display_rectangle.Height = Math.Max(auto_scroll_min_size.Height, height) - dock_padding.Top - dock_padding.Bottom;

                return display_rectangle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Localizable(true)]
        //[MWFCategory("Layout")]
        public DockPaddingEdges DockPadding
        {
            get
            {
                return dock_padding;
            }
        }
        #endregion  // Public Instance Properties

        #region Protected Instance Methods
        protected override CreateParams CreateParams
        {
            get
            {
                return base.CreateParams;
            }
        }

        protected bool HScroll
        {
            get
            {
                return hscroll_visible;
            }

            set
            {
                if (hscroll_visible != value)
                {
                    force_hscroll_visible = value;
                    Recalculate(this, EventArgs.Empty);

                    // Raise event for scrollbar visibility change
                    if (this.HScrollChanged != null)
                    {
                        this.HScrollChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public event System.EventHandler VScrollChanged;
        public event System.EventHandler HScrollChanged;

        protected bool VScroll
        {
            get
            {
                return vscroll_visible;
            }

            set
            {
                if (vscroll_visible != value)
                {
                    force_vscroll_visible = value;
                    Recalculate(this, EventArgs.Empty);

                    // Raise event for scrollbar visibility change
                    if (this.VScrollChanged != null)
                    {
                        this.VScrollChanged(this, EventArgs.Empty);
                    }
                }
            }
        }
        #endregion  // Protected Instance Methods

        #region Public Instance Methods
        public void ScrollControlIntoView(Control activeControl)
        {

            if (!AutoScroll || (!hscroll_visible && !vscroll_visible))
            {
                return;
            }

            if (!Contains(activeControl))
            {
                return;
            }

            // Translate into coords relative to us
            Rectangle activeControlLocalBounds = activeControl.Bounds;
            if (activeControl.Parent != this)
            {
                activeControlLocalBounds.Location = this.PointToClient(activeControl.PointToScreen(Point.Empty));
            }

            this.ScrollRectIntoView(activeControlLocalBounds);
        }

/// <summary>
/// Scrolls the view so that the specified rectangle is visible,
/// or at least the best possible part of it is visible.
/// </summary>
/// <param name="Rect">The rectangle to be made visible, in coordinates
/// expressed in terms of the local viewport. Eg an x coordinate of 3 corresponds
/// to 103 if the current AutoScrollPosition reads -103.</param>
        public void ScrollRectIntoView(Rectangle Rect)
        {

            // Bail if the rect is already fully visible
            Rectangle EffectiveViewArea = new Rectangle(Point.Empty, this.DisplayRectangle.Size);
            if (EffectiveViewArea.Contains(Rect))
                return;

            Rectangle TempRect = EffectiveViewArea;
            TempRect.Intersect(Rect);


            // Adjust scrollbar the minimum necessary whilst making
            // that as much of control as possible is visible, favouring the top of the control
            Point NewScrollPos = scroll_position;
            if ((TempRect.Height < Rect.Height) && (TempRect.Height < EffectiveViewArea.Height)) // If intersection already fills frame height then scrolling won't help
            {
                NewScrollPos.Y = Math.Max(NewScrollPos.Y, scroll_position.Y + Rect.Bottom - EffectiveViewArea.Height);
                NewScrollPos.Y = Math.Min(NewScrollPos.Y, scroll_position.Y + Rect.Top);
            }
            if ((TempRect.Width < Rect.Width) && (TempRect.Width < EffectiveViewArea.Width)) // If intersection already fills width then scrolling won't help
            {
                NewScrollPos.X = Math.Max(NewScrollPos.X, scroll_position.X + Rect.Right - EffectiveViewArea.Width);
                NewScrollPos.X = Math.Min(NewScrollPos.X, scroll_position.X + Rect.Left);
            }

            this.SetDisplayRectLocation(NewScrollPos.X, NewScrollPos.Y);
        }
        
        public void SetAutoScrollMargin(int x, int y)
        {
            if (x < 0)
            {
                x = 0;
            }

            if (y < 0)
            {
                y = 0;
            }

            auto_scroll_margin = new Size(x, y);
            Recalculate(this, EventArgs.Empty);
        }
        #endregion  // Public Instance Methods

        #region Protected Instance Methods
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void AdjustFormScrollbars(bool displayScrollbars)
        {
            Recalculate(this, EventArgs.Empty);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected bool GetScrollState(int bit)
        {
            // Internal MS
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (this.m_LayoutSuspendCount == 0)
            {
                CalculateCanvasSize();

                AdjustFormScrollbars(AutoScroll);   // Dunno what the logic is. Passing AutoScroll seems to match MS behaviour
                base.OnLayout(levent);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (vscroll_visible)
            {
                // Decide by how much to scroll
                int scrollincrement = this.MouseScrollIncrement;
                if (!this.m_ScrollIncrementIsUserDefined)
                    scrollincrement = vscrollbar.LargeChange;

                // Determine direction of scroll
                if (e.Delta > 0)
                    scrollincrement *= -1;
                
                // Use safe method which stops you scrolling too far
                this.SetDisplayRectLocation(hscrollbar.Value, vscrollbar.Value + scrollincrement);
            }
            base.OnMouseWheel(e);
        }

        public void DoMouseWheel(MouseEventArgs e)
        {
            OnMouseWheel(e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
            {
                PerformLayout();
            }
            base.OnVisibleChanged(e);
        }

        protected override void ScaleCore(float dx, float dy)
        {
            dock_padding.Scale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        public void SetDisplayRectLocation(int x, int y)
        {
            hscrollbar.Value = Math.Min(hscrollbar.Maximum - hscrollbar.LargeChange, Math.Max(hscrollbar.Minimum, x));
            vscrollbar.Value = Math.Min(vscrollbar.Maximum - vscrollbar.LargeChange, Math.Max(vscrollbar.Minimum, y));
        }

       
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_SETREDRAW = 0xB;

        private int m_LayoutSuspendCount;
        public void SuspendLayoutForReal()
        {
            m_LayoutSuspendCount += 1;
            SendMessage(new HandleRef(this,this.Handle), WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero); // Suspends painting on this control
        }

        public void ResumeLayoutForReal(bool DoLayout)
        {
            this.m_LayoutSuspendCount = Math.Max(0, this.m_LayoutSuspendCount - 1);

            if (this.m_LayoutSuspendCount == 0)
            {
                this.OnLayout(new LayoutEventArgs(this, "Size"));
                SendMessage(new HandleRef(this, this.Handle), WM_SETREDRAW, new IntPtr(1), IntPtr.Zero); // Allows painting to resume
                base.Invalidate(true);
            }
        }

        /// <summary>
        /// Provides wrapper for the protected
        /// DoubleBuffered property
        /// </summary>
        public bool IsDoubleBuffered
        {
            get { return this.DoubleBuffered; }
            set { this.DoubleBuffered = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
        }
        #endregion  // Protected Instance Methods

        #region Internal & Private Methods
        private Size Canvas
        {
            get
            {
                if (!canvas_size.IsEmpty)
                {
                    return canvas_size;
                }
                CalculateCanvasSize();
                return canvas_size;
            }
        }

        private void CalculateCanvasSize()
        {
            Control child;
            int num_of_children;
            int width;
            int height;
            int extra_width;
            int extra_height;

            num_of_children = Controls.Count;
            width = 0;
            height = 0;
            extra_width = dock_padding.Right;
            extra_height = dock_padding.Bottom;

            for (int i = 0; i < num_of_children; i++)
            {
                child = Controls[i];
                if (child.Dock == DockStyle.Right)
                {
                    extra_width += child.Width;
                }
                else if (child.Dock == DockStyle.Bottom)
                {
                    extra_height += child.Height;
                }
            }

            for (int i = 0; i < num_of_children; i++)
            {
                child = Controls[i];

                if (!(child is ScrollBar) && !(child is SizeGrip))
                {
                    switch (child.Dock)
                    {
                        case DockStyle.Left:
                            {
                                if ((Controls[i].Right + extra_width) > width)
                                {
                                    width = Controls[i].Right + extra_width;
                                }
                                continue;
                            }

                        case DockStyle.Top:
                            {
                                if ((child.Bottom + extra_height) > height)
                                {
                                    height = child.Bottom + extra_height;
                                }
                                continue;
                            }

                        case DockStyle.Fill:
                        case DockStyle.Right:
                        case DockStyle.Bottom:
                            {
                                continue;
                            }

                        default:
                            {
                                AnchorStyles anchor;

                                anchor = child.Anchor;

                                if (((anchor & AnchorStyles.Left) != 0) && ((anchor & AnchorStyles.Right) == 0))
                                {
                                    if ((Controls[i].Right + extra_width) > width)
                                    {
                                        width = Controls[i].Right + extra_width;
                                    }
                                }

                                if (((anchor & AnchorStyles.Top) != 0) || ((anchor & AnchorStyles.Bottom) == 0))
                                {
                                    if ((child.Bottom + extra_height) > height)
                                    {
                                        height = child.Bottom + extra_height;
                                    }
                                }
                                continue;
                            }
                    }
                }
            }
            width += scroll_position.X;
            height += scroll_position.Y;

            canvas_size.Width = width;
            canvas_size.Height = height;
        }

        private void Recalculate(object sender, EventArgs e)
        {
            if (!auto_scroll && !force_hscroll_visible && !force_vscroll_visible)
            {
                return;
            }

            Size canvas = canvas_size;
            Size client = ClientSize;

            canvas.Width += auto_scroll_margin.Width;
            canvas.Height += auto_scroll_margin.Height;

            int right_edge = client.Width;
            int bottom_edge = client.Height;
            int prev_right_edge;
            int prev_bottom_edge;

            bool VScrollWasVisible = vscroll_visible;
            bool HScrollWasVisible = hscroll_visible;

            do
            {
                prev_right_edge = right_edge;
                prev_bottom_edge = bottom_edge;

                if ((force_hscroll_visible || canvas.Width > right_edge) && client.Width > 0)
                {
                    hscroll_visible = true;
                    bottom_edge = client.Height - SystemInformation.HorizontalScrollBarHeight;
                }
                else
                {
                    hscroll_visible = false;
                    bottom_edge = client.Height;
                }

                if ((force_vscroll_visible || canvas.Height > bottom_edge) && client.Height > 0)
                {
                    vscroll_visible = true;
                    right_edge = client.Width - SystemInformation.VerticalScrollBarWidth;
                }
                else
                {
                    vscroll_visible = false;
                    right_edge = client.Width;
                }

            } while (right_edge != prev_right_edge || bottom_edge != prev_bottom_edge);



            if (hscroll_visible)
            {
                // Calculate scrollbar parameters
                hscrollbar.Left = dock_padding.Left;
                hscrollbar.Top = client.Height - SystemInformation.HorizontalScrollBarHeight - dock_padding.Bottom;
                hscrollbar.BringToFront();
                hscrollbar.LargeChange = (right_edge <= 5 ? 10 : right_edge);
                hscrollbar.SmallChange = 5;
                hscrollbar.Maximum = canvas.Width - 1;

                // If scrollbar was previously visible then make sure
                // existing value is not too big for new Maximum and Scrollbar
                hscrollbar.Value = Math.Min(hscrollbar.Value, hscrollbar.Maximum - hscrollbar.LargeChange + 1);
                HandleHorizScrollBar(hscrollbar, new ScrollEventArgs(ScrollEventType.ThumbPosition, hscrollbar.Value));
            }
            else
            {
                this.ScrollWindow(-scroll_position.X, 0);
                HandleHorizScrollBar(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, 0));
            }



            if (vscroll_visible)
            {
                // Calculate scrollbar parameters
                vscrollbar.Left = client.Width - SystemInformation.VerticalScrollBarWidth - dock_padding.Right;
                vscrollbar.Top = dock_padding.Top;
                vscrollbar.BringToFront();
                vscrollbar.LargeChange = (bottom_edge <= 5 ? 10 : bottom_edge);
                vscrollbar.SmallChange = 5;
                vscrollbar.Maximum = canvas.Height - 1;

                // If scrollbar was previously visible then make sure
                // existing value is not too big for new Maximum and Scrollbar
                vscrollbar.Value = Math.Min(vscrollbar.Value, vscrollbar.Maximum - vscrollbar.LargeChange + 1);
                HandleVertScrollBar(vscrollbar, new ScrollEventArgs(ScrollEventType.ThumbPosition, vscrollbar.Value));
            }
            else
            {
                // If the scrollbar was previously visible then
                // we need to return scroll position to zero
                if (scroll_position.Y != 0)
                {
                    this.ScrollWindow(0, -scroll_position.Y);
                    HandleVertScrollBar(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, 0));
                }
            }

            if (hscroll_visible && vscroll_visible)
            {
                hscrollbar.Width = ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth - dock_padding.Left - dock_padding.Right;
                vscrollbar.Height = ClientRectangle.Height - SystemInformation.HorizontalScrollBarHeight - dock_padding.Top - dock_padding.Bottom;

                sizegrip.Left = hscrollbar.Right;
                sizegrip.Top = vscrollbar.Bottom;
                sizegrip.Width = this.ClientRectangle.Width - dock_padding.Right - sizegrip.Left;
                sizegrip.Height = this.ClientRectangle.Height - dock_padding.Bottom - sizegrip.Top;
                sizegrip.BringToFront();

                hscrollbar.Visible = true;
                vscrollbar.Visible = true;

                sizegrip.Visible = true;
            }
            else
            {
                sizegrip.Visible = false;
                if (hscroll_visible)
                {
                    hscrollbar.Width = ClientRectangle.Width - dock_padding.Left - dock_padding.Right;
                    hscrollbar.Visible = true;
                }
                else
                {
                    hscrollbar.Visible = false;
                }

                if (vscroll_visible)
                {
                    vscrollbar.Height = ClientRectangle.Height - dock_padding.Top - dock_padding.Bottom;
                    vscrollbar.Visible = true;
                }
                else
                {
                    vscrollbar.Visible = false;
                }
            }

            // Raise event if scrollbar visibility change
            if (vscroll_visible != VScrollWasVisible)
            {
                if (this.VScrollChanged != null)
                {
                    this.VScrollChanged(this, EventArgs.Empty);
                }
            }
            if (hscroll_visible != HScrollWasVisible)
            {
                if (this.HScrollChanged != null)
                {
                    this.HScrollChanged(this, EventArgs.Empty);
                }
            }
        }

        private void HandleScrollValueChanged(object sender, EventArgs e)
        {
            if (sender == vscrollbar)
            {
                ScrollWindow(0, vscrollbar.Value - scroll_position.Y);
            }
            else
            {
                ScrollWindow(hscrollbar.Value - scroll_position.X, 0);
            }
        }

        private void HandleVertScrollBar(object sender, ScrollEventArgs e)
        {
            if (VerticalScroll != null)
            {
                VerticalScroll(this, e);
            }
        }

        private void HandleHorizScrollBar(object sender, ScrollEventArgs e)
        {
            if (HorizontalScroll != null)
            {
                HorizontalScroll(this, e);
            }
        }

        public event ScrollEventHandler VerticalScroll;
        public event ScrollEventHandler HorizontalScroll;

        private void ScrollWindow(int XOffset, int YOffset)
        {
            int num_of_children;

            if (XOffset == 0 && YOffset == 0)
            {
                return;
            }

            SuspendLayout();

            num_of_children = Controls.Count;

            for (int i = 0; i < num_of_children; i++)
            {
                if (!(Controls[i] is ScrollBar) && !(Controls[i] is SizeGrip))
                {
                    Controls[i].Left -= XOffset;
                    Controls[i].Top -= YOffset;
                }
                // Is this faster? Controls[i].Location -= new Size(XOffset, YOffset);
            }

            scroll_position.X += XOffset;
            scroll_position.Y += YOffset;

            // Should we call XplatUI.ScrollWindow??? If so, we need to position our windows by other means above
            // Since we're already causing a redraw above
            Invalidate(false);
            ResumeLayout();

        }
        #endregion  // Internal & Private Methods

    }
}
