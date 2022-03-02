using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Runtime.InteropServices;
using BluePrism.BPCoreLib;
using NLog;


namespace AutomateControls.Forms
{
    public partial class HelpButtonForm : AutomateForm, IHelp
    {
        private static readonly ILogger NLog = LogManager.GetCurrentClassLogger();
        private enum Style
        {
            windows10,
            windows8,
            windows7,
            aeroGlass, //Just used to signify glass effects.
            windowsXP,
            windows2000
        }
        Form helpButton;
        private bool helpButtonHover = false;
        private bool helpButtonDown = false;
        private Style windowStyle;

        public HelpButtonForm()
        {
            InitializeComponent();
            this.KeyPreview = true;

            if (Application.RenderWithVisualStyles)
            {
                if (IsWindows10OrNewer())
                {
                    windowStyle = Style.windows10;
                }
                else if (IsWindows8OrNewer())
                {
                    windowStyle = Style.windows8;
                }
                else if (IsWindows7OrNewer())
                {
                    if (DwmIsCompositionEnabled())
                        windowStyle = Style.aeroGlass;
                    else
                        windowStyle = Style.windows7;
                }
                else
                    windowStyle = Style.windowsXP;

            }
            else
            {
                windowStyle = Style.windows2000;
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);

        private bool DwmIsCompositionEnabled()
        {
            bool enabled = false;
            try
            {
                DwmIsCompositionEnabled(out enabled);
            }
            catch (Exception ex)
            {
                NLog.Error(ex);
            }
            return enabled;
        }

        private bool IsWindows10OrNewer()
        {
            Version win10version = new Version(10, 0);
            if (BPUtil.GetOSVersion() >= win10version)
                return true;
            return false;
        }

        private bool IsWindows8OrNewer()
        {
            Version win8version = new Version(6, 2, 9200, 0);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version >= win8version)
            {
                return true;
            }
            return false;
        }

        private bool IsWindows7OrNewer()
        {
            Version win7version = new Version(6, 1);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version >= win7version)
            {
                return true;
            }
            return false;
        }

        private bool helpButtonNeeded()
        {
            return HelpButton && (MinimizeBox || MaximizeBox);
        }

        private int titleBarHeight()
        {
            Point p = this.PointToScreen(Point.Empty);
            return p.Y - this.Location.Y;
        }

        private int windowBorderWidth()
        {
            Point p = this.PointToScreen(Point.Empty);
            return p.X - this.Location.X;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode && helpButtonNeeded())
            {
                helpButton = new Form();
                helpButton.Font = new Font("Webdings", 9.25f, FontStyle.Regular);
                switch(windowStyle)
                {
                    case Style.windows8:
                        helpButton.ForeColor = Color.FromArgb(54, 101, 179); //Magic highlight blue color win8
                        break;
                    case Style.windows10:
                        helpButton.ForeColor = Color.FromArgb(229, 229, 229); //Magic highlight color win10
                        helpButton.Font = new Font("Segoe UI", 12.45f, FontStyle.Regular);
                        break;
                }
                helpButton.BackColor = SystemColors.ActiveCaption;
                helpButton.FormBorderStyle = FormBorderStyle.None;
                helpButton.StartPosition = FormStartPosition.Manual;
                helpButton.ShowInTaskbar = false;
                helpButton.AllowTransparency = true;
                helpButton.TransparencyKey = helpButton.BackColor;
                helpButton.MouseDown += new MouseEventHandler(buttonMouseDown);
                helpButton.MouseUp += new MouseEventHandler(buttonMouseUp);
                helpButton.MouseEnter += new EventHandler(buttonMouseEnter);
                helpButton.MouseLeave += new EventHandler(buttonMouseLeave);
                helpButton.Paint += new PaintEventHandler(paintButton);

                helpButton.Show(this);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            updatePosition();
        }

        protected override void OnMove(EventArgs e)
        {
            updatePosition();
            base.OnMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            updatePosition();
            base.OnResize(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (helpButton != null && (windowStyle == Style.windows7 || windowStyle == Style.windowsXP))
            {
                helpButton.Opacity = 0.7;
                helpButton.Invalidate();
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (helpButton != null && (windowStyle == Style.windows7 || windowStyle == Style.windowsXP))
            {
                helpButton.Opacity = 1.0;
                helpButton.Invalidate();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.F1)
                OnF1KeyPressed(e);
        }

        protected virtual void OnF1KeyPressed(KeyEventArgs e)
        {
            this.OnHelpButtonClicked(new CancelEventArgs());
        }

        protected override void OnHelpButtonClicked(CancelEventArgs e)
        {
            try
            {
                OpenHelp();
            }
            finally
            {
                e.Cancel = true;
            }
        }

        protected override void OnMaximized(EventArgs e)
        {
            base.OnMaximized(e);
            helpButton.Invalidate();
        }

        protected override void OnRestored(EventArgs e)
        {
            base.OnRestored(e);
            helpButton.Invalidate();
        }

        private void updatePosition()
        {
            if (helpButton != null)
            {
                int bw = windowBorderWidth();
                int th = titleBarHeight();
                const int sp = 2; //Button spacing.
                const int bc = 4; //Button count

                switch (windowStyle)
                {
                    case Style.windows2000:
                        {
                            helpButton.Left = this.Right - 72;
                            helpButton.Top = this.Top + 6;
                            helpButton.Size = new Size(16, 14);
                        }
                        break;
                    case Style.windowsXP:
                        {
                            int s = th - bw - 5;
                            helpButton.Left = this.Right - ((bc * (s + sp)) + bw);
                            helpButton.Top = this.Top + (bw + sp);
                            helpButton.Size = new Size(s, s);
                        }
                        break;
                    case Style.aeroGlass:
                        {
                            int sy = th - bw - 2;
                            int sx = ((156 * th) / 100) - bw - 7;
                            helpButton.Left = this.Right - ((bc * (sx + sp)) + bw) + 3;
                            if (this.WindowState == FormWindowState.Maximized) {
                                helpButton.Top = this.Top + 7;
                                helpButton.Left -= 5;
                            } else {
                                helpButton.Top = this.Top;
                            }
                            helpButton.Size = new Size(sx, sy);
                        }
                        break;
                    case Style.windows7:
                        {
                            int sy = th - bw - 5;
                            int sx = ((156 * th) / 100) - bw - 7;
                            helpButton.Left = this.Right - ((bc * (sx + sp)) + bw);
                            helpButton.Top = this.Top + (bw + sp);
                            helpButton.Size = new Size(sx, sy);
                        }
                        break;
                    case Style.windows8:
                        {
                            if (this.WindowState == FormWindowState.Maximized)
                            {
                                helpButton.Left = this.Right - 136;
                                helpButton.Top = this.Top + 7;
                            }
                            else
                            {
                                helpButton.Left = this.Right - 132;
                                helpButton.Top = this.Top + 1;
                            }
                            helpButton.Size = new Size(26, 20);
                        }
                        break;
                    case Style.windows10:
                        {
                            if (this.WindowState == FormWindowState.Maximized)
                            {
                                helpButton.Left = this.Right - 191;
                                helpButton.Top = this.Top + 8;
                                helpButton.Size = new Size(45, 21);
                            }
                            else
                            {
                                helpButton.Left = this.Right - 191;
                                helpButton.Top = this.Top + 1;
                                helpButton.Size = new Size(45, 29);
                            }

                        }
                        break;

                }
            }
        }

        private void buttonMouseDown(object sender, MouseEventArgs e)
        {
            helpButtonDown = true;
            helpButton.Invalidate();

        }

        private void buttonMouseUp(object sender, MouseEventArgs e)
        {
            helpButtonDown = false;
            helpButton.Invalidate();
            this.OnHelpButtonClicked(new CancelEventArgs());
        }

        private void paintButton(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle r = e.ClipRectangle;
            g.Clear(helpButton.BackColor);

            Brush b = SystemBrushes.ControlText;
            switch (windowStyle)
            {
                case Style.windows2000:
                    {
                        if (windowStyle == Style.windows2000)
                            ControlPaint.DrawButton(g, r, helpButtonDown ? ButtonState.Pushed : ButtonState.Normal);
                    }
                    break;
                case Style.windowsXP:
                case Style.windows7:
                case Style.aeroGlass:
                    {
                        if (Application.RenderWithVisualStyles)
                        {
                            VisualStyleRenderer render = new VisualStyleRenderer(VisualStyleElement.Window.HelpButton.Normal);

                            if (helpButtonHover)
                                render = new VisualStyleRenderer(VisualStyleElement.Window.HelpButton.Hot);
                            if (helpButtonDown)
                                render = new VisualStyleRenderer(VisualStyleElement.Window.HelpButton.Pressed);

                            render.DrawBackground(g, r);
                            if (windowStyle == Style.aeroGlass)
                                g.DrawLine(Pens.Black, 0, 0, helpButton.Width, 0);
                        }
                    }
                    break;
                case Style.windows8:
                    {
                        if (helpButtonHover)
                        {
                            g.Clear(helpButton.ForeColor);
                            b = SystemBrushes.HighlightText;
                        }
                    }
                    break;
                case Style.windows10:
                    {
                        if (Form.ActiveForm != this)
                        {
                            b = new SolidBrush(Color.FromArgb(153, 153, 153));
                        }
                        if (helpButtonHover)
                        {
                            g.Clear(helpButton.ForeColor);
                            b = SystemBrushes.ControlText;
                        }

                    }
                    break;
            }

            //Draw the question mark for win8 and win2k
            if (windowStyle == Style.windows2000 || windowStyle == Style.windows8 || windowStyle == Style.windows10)
            {
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                    if (windowStyle == Style.windows10)
                    {
                        g.DrawString("?", helpButton.Font, b, r, sf);
                    }
                    else
                    {
                        // "s" is a question mark in webdings.
                        g.DrawString("s", helpButton.Font, b, r, sf);
                    }
                }
            }
        }

        private void buttonMouseEnter(object sender, EventArgs e)
        {
            helpButtonHover = true;
            helpButton.Invalidate();
        }

        private void buttonMouseLeave(object sender, EventArgs e)
        {
            helpButtonHover = false;
            helpButton.Invalidate();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (helpButton != null)
                helpButton.Close();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (helpButton != null) 
                helpButton.Visible = this.Visible;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if(helpButton != null)
                helpButton.Invalidate();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (helpButton != null)
                helpButton.Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCMOUSEMOVE = 0x00A0;
            switch (m.Msg)
            {
                case WM_NCMOUSEMOVE:
                    {
                        //LParam contains two 16bit values for x and y 
                        //Fortunately .Net Point structure handles this 
                        //in a constructor overload
                        Point p = new Point(m.LParam.ToInt32());
                        MouseEventArgs e = new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0);
                        OnNonClientMouseMove(e);
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        protected virtual void OnNonClientMouseMove(MouseEventArgs e)
        {
            if (helpButton != null)
            {
                if (helpButton.Bounds.Contains(e.Location))
                    buttonMouseEnter(this, e);
                else
                    buttonMouseLeave(this, e);
            }
        }

        #region IHelp Members

        public virtual string GetHelpFile()
        {
            return this.Name;
        }

        #endregion

        public virtual void OpenHelp()
        {
            HelpLauncher.ShowTopic(this);
        }
    }
}
