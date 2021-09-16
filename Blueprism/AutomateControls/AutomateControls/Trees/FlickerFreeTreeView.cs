using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices.APIs;
using AutomateControls.UIStructs;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AutomateControls.Trees
{
    /// <summary>
    /// A treeview which ignores requests to draw its background.
    /// </summary>
    /// <remarks>
    /// This idea is taken from :-
    /// http://dev.nomad-net.info/articles/double-buffered-tree-and-list-views
    /// The reason we couldn't use the last, rather more elegant, solution in the
    /// article is because that doesn't necessarily honour the font used in the
    /// treeview for particular nodes - specifically, it didn't set the category
    /// nodes in the schedule manager treeview to be bold.
    /// Otherwise, that seemed to work fine too.
    /// </remarks>
    public class FlickerFreeTreeView : TreeView
    {
        /// <summary>
        /// Event fired when the a node is right clicked in the tree view
        /// </summary>
        public event TreeNodeRightClickEventHandler RightClickNode;

        /// <summary>
        /// WM_PRINT flag taken from winuser.h
        /// </summary>
        private const int PRF_CLIENT = 0x00000004;
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;

        /// <summary>
        /// Buffered Graphics object to handle the double buffering in this
        /// treeview.
        /// </summary>
        private BufferedGraphics bg;

        /// <summary>
        /// Flag to enable or disable double clicks in this treeview
        /// </summary>
        private bool m_dblClickEnabled = true;

        /// <summary>
        /// Gets or sets whether double click is enabled in this treeview. If not
        /// enabled, where the doubleclick event would usually fire, an extra
        /// mousedown (WM_LBUTTONDOWN) event is passed instead.
        /// </summary>
        [Browsable(true), DefaultValue(true), Description("Enables or disables "+
            "double-clicking; replacing it with two clicks when disabled")]
        public bool DoubleClickEnabled
        {
            get { return m_dblClickEnabled; }
            set { m_dblClickEnabled = value; }
        }
        
        /// <summary>
        /// Handles the size of the treeview changing - ensures that the double
        /// buffering graphics object is removed on a size change (the size of the
        /// buffer matches the size of the control, so it must be refreshed when
        /// the control size changes)
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ClearBuffer();
        }

        /// <summary>
        /// The .NET TreeView class overrides the DoubleBuffered property and hides it. 
        /// This is used to put it back in and therefore reduce flickering on mouse hover.
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        /// <summary>
        /// Clears the buffered graphics object being used as a double buffer for
        /// this control.
        /// </summary>
        private void ClearBuffer()
        {
            if (bg != null)
            {
                bg.Dispose();
                bg = null;
            }
        }
        
        public  List<TreeNode> ExpandedNodes => GetExpandedNodes(Nodes);
        
        private static List<TreeNode> GetExpandedNodes(IEnumerable nodes)
        {
            var listOfExpandedNodes = new List<TreeNode>();
            foreach (TreeNode node in nodes)
            {
                if (node.IsExpanded)
                {
                    listOfExpandedNodes.Add(node);
                }

                if (node.Nodes.Count > 0)
                {
                    listOfExpandedNodes.AddRange(GetExpandedNodes(node.Nodes));
                }
            }

            return listOfExpandedNodes;
        }

        /// <summary>
        /// Handles the disposing of this treeview, in this case ensuring that the
        /// buffer being used to double buffer the painting is disposed of when
        /// this control is disposed of.
        /// </summary>
        /// <param name="disposing">True when disposing of this control explicitly,
        /// False when preparing for cleanup by the garbage collector</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearBuffer();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Override the mouse click event on a node, and raise a 
        /// <see cref="RightClickNode"/> event if the the right mouse button is
        /// clicked
        /// </summary>
        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                OnRightClickNode(new TreeNodeRightClickEventArgs(e.Node));

            base.OnNodeMouseClick(e);
        }

        /// <summary>
        /// Raises the <see cref="RightClickNode"/> event
        /// </summary>
        /// <param name="e">The args detailing the event</param>
        protected void OnRightClickNode(TreeNodeRightClickEventArgs e)
        {
            var handler = this.RightClickNode;
            if (handler != null)
                handler(this, e);
        }
        
        /// <summary>
        /// Handles the processing of windows messages - specifically the WM_ERASEBKGND
        /// message (to disable the erasing of the current view) and the WM_PAINT message
        /// (to draw the treeview into a buffer before drawing to the screen).
        /// </summary>
        /// <param name="m">The message to process</param>
        protected override void WndProc(ref Message m)
        {
            switch ((Msg)m.Msg)
            {
                case Msg.WM_ERASEBKGND: // Disable the background erasure - 
                    if (DoubleBuffered) break; // skip own-rolled code if in Vista+
                    m.Result = (IntPtr)1;
                    return;

                case Msg.WM_PAINT:
                    if (DoubleBuffered) break; // skip own-rolled code if in Vista+

                    // Prepare the painting structure and enter the paint phase,
                    // retrieving the ultimate target device context
                    APIsStructs.PAINTSTRUCT ps = new APIsStructs.PAINTSTRUCT();
                    IntPtr hdc = APIsUser32.BeginPaint(this.Handle, ref ps);
                    if (hdc == IntPtr.Zero)
                    {
                        Debug.Print(
                            "Failed to get a display device context to draw a treeview");
                        return;
                    }
                    try
                    {
                        // Create the buffer if we need to - it is created for the current
                        // size of the control - if that size changes, the buffer is refreshed
                        // to ensure it always has the right size (see OnSizeChanged override)
                        if (bg == null)
                        {
                            bg = BufferedGraphicsManager.Current.Allocate(hdc, ClientRectangle);
                            bg.Graphics.FillRectangle(Brushes.White, ClientRectangle);
                        }

                        // Get the device context for the buffer's graphics object -
                        // draw directly to that first, then render it to the ultimate
                        // device context - ie. the screen.
                        IntPtr bgHdc = bg.Graphics.GetHdc();
                        APIsUser32.SendMessage(
                            this.Handle, (int)Msg.WM_PRINT, bgHdc, (IntPtr)PRF_CLIENT);
                        bg.Graphics.ReleaseHdc(bgHdc);
                        bg.Render(hdc);
                    }
                    finally
                    {
                        // ensure the paint phase is committed regardless of result
                        APIsUser32.EndPaint(this.Handle, ref ps);
                    }
                    return;

                case Msg.WM_LBUTTONDBLCLK:
                    if (!m_dblClickEnabled)
                        m.Msg = (int)Msg.WM_LBUTTONDOWN;
                    // We still want to process the message, whether it's been
                    // converted or not, so drop through to the WndProc call below.
                    break;
                
            }

            // Not ERASEBKGND or PAINT? Let the base class deal with it
            base.WndProc(ref m);
        }
    }
}
