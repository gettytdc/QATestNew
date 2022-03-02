using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WixSharp.UI.Forms;

namespace BluePrism.Setup.Controls
{
    public class BluePrismTreeView : TreeView
    {
        #region " Hide the checkboxes for readonly nodes "

        private const int WM_LBUTTONDBLCLICK = 0x0203;

        public const int TVIF_STATE = 0x8;
        public const int TVIS_STATEIMAGEMASK = 0xF000;
        public const int TV_FIRST = 0x1100;
        public const int TVM_SETITEM = TV_FIRST + 63;


        [StructLayout(LayoutKind.Sequential)]
        public struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public void RemoveCheckbox(TreeNode n)
        {
            var tvi = new TVITEM
            {
                hItem = n.Handle,
                mask = TVIF_STATE,
                stateMask = TVIS_STATEIMAGEMASK,
                state = 0
            };
            var lparam = Marshal.AllocHGlobal(Marshal.SizeOf(tvi));
            Marshal.StructureToPtr(tvi, lparam, false);
            SendMessage(this.Handle, TVM_SETITEM, IntPtr.Zero, lparam);
        }

        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && this.SelectedNode is ReadOnlyTreeNode n && n.IsReadOnly)
            {
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }
        // Suppress double click
       
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLICK)
            {
                m.Result = IntPtr.Zero;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}
