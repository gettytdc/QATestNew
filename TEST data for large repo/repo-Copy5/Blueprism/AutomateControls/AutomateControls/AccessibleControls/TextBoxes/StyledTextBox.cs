using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutomateControls.Textboxes
{
    public partial class StyledTextBox : TextBox
    {
        private Color _originalBackColor;
        private Color _originalForeColor;
        private Color _originalBorderColor;
        private Color _borderColor = Color.Empty;
        private int _borderWidth = 1;
        private bool _working;
        
        public StyledTextBox()
        {
            InitializeComponent();

            Enter += OnFocus;
            Leave += OnFocusLost;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
        }

        /// <summary>
        /// If this control is focused relative to the form but the window itself is unfocused.
        /// </summary>
        private bool FocusedAndWindowUnfocused
           => !this.HasWindowFocus() && BorderColor == ColourScheme.BluePrismControls.FocusColor;

        private void OnFocus(object sender, EventArgs e)
        {
            if (ReadOnly)
                return;

            ChangeBorder(ColourScheme.BluePrismControls.FocusColor, 3);
            BackColor = _originalBackColor;
            ForeColor = _originalForeColor;
        }

        private void OnFocusLost(object sender, EventArgs e)
        {
            ChangeBorder(_originalBorderColor, 1);
        }

        private void ChangeBorder(Color borderColor, int borderWidth)
        {
            _working = true;
            SuspendLayout();
            BorderColor = borderColor;
            _borderWidth = borderWidth;
            Redraw();
            ResumeLayout();
            _working = false;
        }

        private void Redraw()
        {
            if (BorderStyle == BorderStyle.None) return;
            RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero,
                RDW_FRAME | RDW_IUPDATENOW | RDW_INVALIDATE);
        }
        private void OnMouseEnter(object sender, EventArgs e)
        {
            if (!Enabled || ReadOnly || Focused || FocusedAndWindowUnfocused) return;
            _working = true;
            BackColor = ColourScheme.BluePrismControls.HoverColor;
            ForeColor = Color.Black;
            _working = false;
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (!Enabled || ReadOnly) return;
            _working = true;
            BackColor = _originalBackColor;
            ForeColor = _originalForeColor;
            _working = false;
        }

        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (!_working)
                    _originalBorderColor = value;
                _borderColor = value;

            }
        }
        #region "definitions and imports for WndProc"
        private const int WM_NCPAINT = 0x85;
        private const uint RDW_INVALIDATE = 0x1;
        private const uint RDW_IUPDATENOW = 0x100;
        private const uint RDW_FRAME = 0x400;

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprc, IntPtr hrgn, uint flags);
        #endregion

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCPAINT && BorderStyle == BorderStyle.Fixed3D)
            {
                var hdc = GetWindowDC(this.Handle);
                using (var g = Graphics.FromHdcInternal(hdc))
                    ControlPaint.DrawBorder(g, new Rectangle(0, 0, Size.Width, Size.Height),
                        BorderColor, _borderWidth, ButtonBorderStyle.Solid,
                        BorderColor, _borderWidth, ButtonBorderStyle.Solid,
                        BorderColor, _borderWidth, ButtonBorderStyle.Solid,
                        BorderColor, _borderWidth, ButtonBorderStyle.Solid);
                ReleaseDC(this.Handle, hdc);
            }
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Redraw();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (!_working)
                _originalBackColor = BackColor;
        }
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            if (!_working)
                _originalForeColor = ForeColor;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!Enabled)
            {
                BackColor = _originalBackColor;
                ForeColor = _originalForeColor;
            }
        }

        protected override void OnBorderStyleChanged(EventArgs e)
        {
            base.OnBorderStyleChanged(e);
            if (BorderStyle == BorderStyle.None || BorderStyle == BorderStyle.Fixed3D) return;
            BorderStyle = BorderStyle.Fixed3D;
        }
 
    }
}
