using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.Textboxes
{
    public partial class ReadOnlyTextBox : TextBox 
    {
        const int WM_SETFOCUS = 0x0007;
        const int WM_KILLFOCUS = 0x0008;
        public ReadOnlyTextBox()
        {
            InitializeComponent();
            this.BorderStyle = BorderStyle.None;
            this.AccessibleRole = AccessibleRole.StaticText;
            BackColor = Color.White;
            ReadOnly = true;
            Font = new Font("Segoe UI", 16, FontStyle.Regular, GraphicsUnit.Pixel);
            Cursor = Cursors.Default;
        }
     
        private bool _selectionEnabled;

        public bool SelectionEnabled { get { return _selectionEnabled; } set { _selectionEnabled = value; } }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SETFOCUS && !SelectionEnabled)
                m.Msg = WM_KILLFOCUS;

            base.WndProc(ref m);
        }
        public override bool AutoSize { get => false; }
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
