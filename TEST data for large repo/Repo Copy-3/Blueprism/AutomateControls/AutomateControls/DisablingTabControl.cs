using System;
using System.Windows.Forms.VisualStyles;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace AutomateControls
{
    /// <summary>
    /// Tab control which supports disabled tab pages.
    /// </summary>
    /// <remarks>A handy 'how to use visual styles renderer' tutorial is up at:
    /// http://msdn.microsoft.com/en-us/library/system.windows.forms.tabrenderer%28v=vs.80%29.aspx
    /// </remarks>
    public class DisablingTabControl : TabControl
    {
        public Dictionary<TabPage, int> TabItemHorizontalPositions { get; private set; } = new Dictionary<TabPage, int>();

        /// <summary>
        /// Creates a new disabling tab control.
        /// </summary>
        public DisablingTabControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Handles the selecting event, ensuring that the tab that is being selected
        /// is actually enabled. Cancels the event otherwise.
        /// </summary>
        /// <param name="e">The args detailing the event.</param>
        protected override void OnSelecting(TabControlCancelEventArgs e)
        {
            if (!e.TabPage.Enabled)
                e.Cancel = true;
            else
                base.OnSelecting(e);
        }

        public bool DrawBorder
        {
            get;
            set;
        }

        /// <summary>
        /// Handles the drawing of the tab control.
        /// </summary>
        /// <param name="e">The args detailing the draw event.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Control p = Parent;
            if (p != null)
                e.Graphics.Clear(p.BackColor);

            if ((TabCount <= 0) || (SelectedTab == null)) return;

            //Draw a border around TabPage
            if (DrawBorder)
            {
                Rectangle rect = SelectedTab.Bounds;
                rect.Inflate(3, 3);
                rect.Offset(1, 0);
                if (Application.RenderWithVisualStyles)
                {
                    VisualStyleRenderer render = new VisualStyleRenderer(VisualStyleElement.Tab.Pane.Normal);
                    render.DrawBackground(e.Graphics, rect);
                }
                else
                {
                    ControlPaint.DrawButton(e.Graphics, rect, ButtonState.Normal);
                }
            }

             //Draw the Tabs
            for (int index = 0; index < TabCount; index++)
            {
                DrawItemEventArgs ed = new DrawItemEventArgs(e.Graphics, this.Font, Rectangle.Empty, index, DrawItemState.None);
                DrawTabItem(ed);
            }
        }

        /// <summary>
        /// Handles the drawing of the tab
        /// </summary>
        /// <param name="e">The args detailing the draw event.</param>
        private void DrawTabItem(DrawItemEventArgs e)
        {
            TabPage page = TabPages[e.Index];
            bool selected = object.ReferenceEquals(SelectedTab, page);

            Rectangle rect = GetTabRect(e.Index);
            Rectangle rect2 = rect;
            rect2.Offset(-1, -1);
            rect2.Inflate(2, 2);

            Rectangle txtRect = rect;

            if (Application.RenderWithVisualStyles)
            {
                VisualStyleRenderer render = default(VisualStyleRenderer);
                if (selected)
                {
                    render = new VisualStyleRenderer(VisualStyleElement.Tab.TabItem.Hot);
                    rect.Height += 2;
                    rect.Y -= 1;
                    txtRect.Height -= 1;
                }
                else
                {
                    if (!page.Enabled)
                    {
                        render = new VisualStyleRenderer(VisualStyleElement.Tab.TabItem.Disabled);
                    }
                    else if (rect.Contains(PointToClient(Cursor.Position)))
                    {
                        render = new VisualStyleRenderer(VisualStyleElement.Tab.TabItem.Hot);
                    }
                    else
                    {
                        render = new VisualStyleRenderer(VisualStyleElement.Tab.TabItem.Normal);
                    }
                    txtRect.Y += 2;
                    txtRect.Height -= 2;
                }
                render.DrawBackground(e.Graphics, rect);
            }
            else
            {

                if (selected)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Control, rect);
                    ControlPaint.DrawBorder3D(e.Graphics, rect, Border3DStyle.RaisedInner, Border3DSide.Top | Border3DSide.Left);

                    ControlPaint.DrawBorder3D(e.Graphics, rect, Border3DStyle.Raised, Border3DSide.Right);
                }
                else
                {
                    e.Graphics.FillRectangle(SystemBrushes.Control, rect);

                    Rectangle rect3 = rect;
                    if (e.Index == 0) rect3.Offset(2, 1); else rect3.Offset(0, 1);
                    ControlPaint.DrawBorder3D(e.Graphics, rect3, Border3DStyle.RaisedInner, Border3DSide.Top | Border3DSide.Left);

                    if (e.Index != SelectedIndex - 1)
                        ControlPaint.DrawBorder3D(e.Graphics, rect, Border3DStyle.Raised, Border3DSide.Right);
                    ControlPaint.DrawBorder3D(e.Graphics, rect, Border3DStyle.Sunken, Border3DSide.Bottom);
                    txtRect.Y += 2;
                    txtRect.Height -= 2;
                }
            }

            StringFormat fmt = new StringFormat();
            fmt.Alignment = StringAlignment.Center;
            fmt.LineAlignment = StringAlignment.Center;

            // rect changes for the text -
            if (!page.Enabled)
            {
                ControlPaint.DrawStringDisabled(e.Graphics, page.Text, page.Font, page.BackColor, txtRect, fmt);
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(page.ForeColor))
                {
                    e.Graphics.DrawString(page.Text, page.Font, brush, txtRect, fmt);
                }
            }

            TabItemHorizontalPositions[page] = txtRect.X;
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Invalidate();
        }
    }
}
