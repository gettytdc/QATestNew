using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BluePrism.Setup.Controls
{
    public partial class BluePrismReadOnlyRichTextBox : RichTextBox
    {
        const int WM_SETFOCUS = 0x0007;
        const int WM_KILLFOCUS = 0x0008;

        public BluePrismReadOnlyRichTextBox()
        {
            InitializeComponent();
            this.BorderStyle = BorderStyle.None;
            this.AccessibleRole = AccessibleRole.StaticText;
            BackColor = Color.White;
            ReadOnly = true;
            Font = new Font("Segoe UI", 16, FontStyle.Regular, GraphicsUnit.Pixel);
            Cursor = Cursors.Default;
        }
        private Padding _padding;
        public Padding TextPadding
        {
            get { return _padding; }
            set
            {
                _padding = value;
                SetPadding(_padding);
            }
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value;
        }

        private bool _selectionEnabled;

        public bool SelectionEnabled
        {
            get
            {
                return _selectionEnabled;
            }
            set
            {
                _selectionEnabled = value;
            }
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SETFOCUS && !SelectionEnabled)
                m.Msg = WM_KILLFOCUS;

            base.WndProc(ref m);
        }
        private bool _contentCopied;
        public bool ContentsCopied
        {
            get
            {
                return _contentCopied;
            }
            set
            {
                _contentCopied = value;
                Refresh();
            }
        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (_contentCopied)
            {
                pe.Graphics.DrawImageUnscaled(Properties.Resources.copy, new Rectangle(new Point(0,0), Properties.Resources.copy.Size));
            }
            base.OnPaint(pe);
        }

        private const int EM_SETRECT = 0xB3;

        [DllImport(@"User32.dll", EntryPoint = @"SendMessage", CharSet = CharSet.Auto)]
        private static extern int SendMessageRefRect(IntPtr hWnd, uint msg, int wParam, ref RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;

            private RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom)
            {
            }
        }

        public void SetPadding(Padding padding)
        {
            var rect = new Rectangle(padding.Left, padding.Top, ClientSize.Width - padding.Left - padding.Right, ClientSize.Height - padding.Top - padding.Bottom);
            RECT rc = new RECT(rect);
            SendMessageRefRect(Handle, EM_SETRECT, 0, ref rc);
        }
    }
}
