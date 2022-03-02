using System;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Drawing;

namespace AutomateControls
{
    public class TabControlEx : TabControl
    {
        public TabControlEx()
            : base()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        }

        /// <summary>
        /// Types of background fill.
        /// </summary>
        public enum BackGroundStyles
        {
            /// <summary>
            /// A linear gradient fill, from left to right
            /// across the control.
            /// </summary>
            LinearGradientFill,
            /// <summary>
            /// A solid color fill throughout the background
            /// of the control.
            /// </summary>
            BlockFill
        }

        /// <summary>
        /// private member to store public property BackGoundStyle
        /// </summary>
        private BackGroundStyles mBackGroundStyle;
        /// <summary>
        /// Describes how the background area
        /// behind the tabs is rendered.
        /// </summary>
        /// <remarks>When set to LinearGradientFill, the properties
        /// BackGroundGradientColorLeft and BackGroundGradientColorRight
        /// should be set to determine the gradient colours.
        /// 
        /// When set to BlockFill, the control BackColor is
        /// used.</remarks>
        public BackGroundStyles BackGroundStyle
        {
            get
            {
                return this.mBackGroundStyle;
            }
            set
            {
                this.mBackGroundStyle = value;
            }
        }

        /// <summary>
        /// Private member to store public property
        /// BackGroundGradientColorLeft.
        /// </summary>
        private Color mBackGroundGradientColorLeft;
        /// <summary>
        /// The left hand colour to use in the background
        /// gradient fill.
        /// </summary>
        /// <remarks>Relevant only when the BackGroundStyle
        /// is set to LinearGradientFill.</remarks>
        public Color BackGroundGradientColorLeft
        {
            get
            {
                return this.mBackGroundGradientColorLeft;
            }
            set
            {
                this.mBackGroundGradientColorLeft = value;
            }
        }

        /// <summary>
        /// Private member to store public property
        /// BackGroundGradientColorLeft.
        /// </summary>
        private Color mBackGroundGradientColorRight;
        /// <summary>
        /// The right hand colour to use in the background
        /// gradient fill.
        /// </summary>
        /// <remarks>Relevant only when the BackGroundStyle
        /// is set to LinearGradientFill.</remarks>
        public Color BackGroundGradientColorRight
        {
            get
            {
                return this.mBackGroundGradientColorRight;
            }
            set
            {
                this.mBackGroundGradientColorRight = value;
            }
        }


        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawTabControl(e.Graphics);
        }


        /// <summary>
        /// Draws the tab control by drawing each tab one by one.
        /// </summary>
        /// <param name="g"></param>
        private void DrawTabControl(Graphics g)
        {
            if (this.Visible)
            {


                Rectangle TabArea = this.DisplayRectangle;
           

                // compensate for last pixel
                TabArea.Y = TabArea.Y + 1;
                TabArea.Width = TabArea.Width + 1;

                // compensate for border size
                int BorderSize = SystemInformation.Border3DSize.Width;
                TabArea.Inflate(BorderSize, BorderSize);

                // Draw tab background
                if (VisualStyleInformation.IsEnabledByUser)
                {
                    VisualStyleRenderer VSR = new VisualStyleRenderer(VisualStyleElement.Tab.Pane.Normal);
                    VSR.DrawBackground(g, TabArea);
                }

                // Draw the background behind the tabs according to
                // the style set in the BackGroundStyle property
                System.Drawing.Brush brush;
                switch (this.BackGroundStyle)
                {
                    
                    case BackGroundStyles.LinearGradientFill:
                        System.Drawing.PointF p1 = new PointF(0, 0);
                        System.Drawing.PointF p2 = new PointF(TabArea.Width, TabArea.Bottom);
                        brush = new System.Drawing.Drawing2D.LinearGradientBrush(p1, p2, BackGroundGradientColorLeft, BackGroundGradientColorRight);
                        break;
                    default:
                        brush = new SolidBrush(BackColor);
                        break;
                }
                g.FillRectangle(brush,0,0,TabArea.Width+5,TabArea.Top);
                

                // draw each tab in turn
                int i;
                for (i = 0; i < this.TabCount; i++)
                {
                    DrawTab(g, i);
                }
                // finally, redraw selected tab so it appears on top of the others
                DrawTab(g, this.SelectedIndex);
            }
        }



        /// <summary>
        /// Draws the tab with the specified index.
        /// </summary>
        /// <param name="g">The graphics object to use.</param>
        /// <param name="TabIndex">The index of the tab to draw.</param>
        private void DrawTab(Graphics g, int TabIndex)
        {
            Rectangle TabBounds = this.GetTabRect(TabIndex);
            RectangleF tabBoundsF = (RectangleF)TabBounds;
            TabPage TabPage = this.TabPages[TabIndex];

            bool TabIsSelected = (this.SelectedIndex == TabIndex);
            VisualStyleRenderer VSR;
            VisualStyleElement VSE;

            // decide size and appearance of tab depending on whether selected
            if (TabIsSelected)
            {
                TabBounds.Height = TabBounds.Height + 10;
                TabBounds.Y -= 1; // raises top of tab to make it stand out
                VSE = VisualStyleElement.Tab.TabItem.Pressed;
            }
            else
            {
                TabBounds.Y = TabBounds.Y + 1;
                VSE = VisualStyleElement.Tab.TabItem.Normal;
            }

            // Draw tab itself - either fancy or plain depending on visual style
            if (VisualStyleInformation.IsEnabledByUser)
            {
                VSR = new VisualStyleRenderer(VSE);
                VSR.DrawBackground(g, TabBounds);
                VSR.DrawEdge(g, TabBounds, Edges.Diagonal, EdgeStyle.Sunken, EdgeEffects.Flat);
            }
            else
            {
                // Increased height to get rid of bottom border
                TabBounds.Height = TabBounds.Height + 3;
                ControlPaint.DrawButton(g, TabBounds, ButtonState.Normal);
                TabBounds.Height = TabBounds.Height - 3;
            }

            // Draw image if needs be
            if (TabPage.ImageIndex >= 0)
            {
                if (!(ImageList == null))
                {
                    if (!(ImageList.Images[TabPage.ImageIndex] == null))
                    {
                        const int LeftMargin = 8;
                        const int RightMargin = 2;
                        Image img = ImageList.Images[TabPage.ImageIndex];
                        Rectangle ImageBounds = new Rectangle(TabBounds.X + LeftMargin, TabBounds.Y + 1, img.Width, img.Height);
                        Single ImageAdjustment = (Single)(LeftMargin + img.Width + RightMargin);
                        ImageBounds.Y += (int)(TabBounds.Height - img.Height) / 2;
                        tabBoundsF.X += ImageAdjustment;
                        tabBoundsF.Width -= ImageAdjustment;
                        g.DrawImage(img, ImageBounds);
                    }
                }
            }

            // Decide what backcolour to use, if any
            Color TabBackColor = Color.Empty;
            if (TabPage is TabPageEx)
            {
                if (TabIsSelected)
                {
                    TabBackColor = ((TabPageEx)TabPage).TabSelectedBackColor;
                }
                else
                {
                    TabBackColor = ((TabPageEx)TabPage).TabBackColor;
                }
            }

            // Draw the backcolour in, if needs be
            if (!TabBackColor.Equals(Color.Empty))
            {
                if (TabIsSelected)
                {
                    g.FillRectangle(new SolidBrush(TabBackColor), new Rectangle((int)(tabBoundsF.X + 2), (int)(tabBoundsF.Y + 4), (int)(tabBoundsF.Width - 4), (int)(tabBoundsF.Height - 3)));
                }
                else
                {
                    g.FillRectangle(new SolidBrush(TabBackColor), new Rectangle((int)(tabBoundsF.X + 1), (int)(tabBoundsF.Y + 4), (int)(tabBoundsF.Width - 3), (int)(tabBoundsF.Height - 3)));
                }
                g.DrawLine(new Pen(TabBackColor, 2), TabBounds.X + 2, TabBounds.Y + 2, TabBounds.Right - 2, TabBounds.Y + 2);
            }

            // Draw tab text
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            Brush br = new SolidBrush(TabPage.ForeColor);
            g.DrawString(TabPage.Text, Font, br, tabBoundsF, stringFormat);

        }

    }

}