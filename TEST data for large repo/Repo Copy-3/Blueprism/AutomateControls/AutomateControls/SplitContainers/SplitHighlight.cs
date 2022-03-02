using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutomateControls.SplitContainers
{
    public class SplitHighlight : Form
    {
        public SplitHighlight()
        {
            Opacity = 0;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
        }
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            Hide();
        }
        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(int hWnd, int hWndInsertAfter,
             int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public void ShowInactiveTopmost()
        {
            ShowWindow(Handle, SW_SHOWNOACTIVATE);
            SetWindowPos(Handle.ToInt32(), HWND_TOPMOST,Left, Top, Width, Height, SWP_NOACTIVATE);
            Opacity = 1;
        }
    }
}
