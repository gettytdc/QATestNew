using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using NLog;

namespace AutomateControls
{
    /// <summary>
    /// Static class to provide some UI utilities. These would be extension methods
    /// and probably will be once we move up from .net 2.
    /// </summary>
    public static class UIUtil
    {
        private static readonly ILogger NLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the ancestor of the required type of the given control, or null if
        /// no such ancestor exists.
        /// </summary>
        /// <typeparam name="T">The type of ancestor required.</typeparam>
        /// <param name="ctl">The control for which the ancestor is required.
        /// </param>
        /// <returns>The first ancestor of the given control which has the type 'T',
        /// or null if no such ancestor was found</returns>
        /// <remarks>Note that if the control itself is an instance of the required
        /// type it is <em>not</em> returned by this method.</remarks>
        public static T GetAncestor<T>(this Control ctl) where T : class
        {
            while (ctl != null)
            {
                Control parent = ctl.Parent;
                T ancestor = parent as T;
                if (ancestor != null)
                    return ancestor;
                ctl = parent;
            }
            return null;
        }

        /// <summary>
        /// Finds the control which has focus on or underneath the given control.
        /// </summary>
        /// <param name="ctl">The control to search for the focused control</param>
        /// <returns>The focused control within the scope of the given control - ie.
        /// either the control itself or one of its descendants
        /// </returns>
        public static Control FindFocusedControl(this Control ctl)
        {
            if (ctl == null)
                return null;

            if (ctl.Focused)
                return ctl;

            // Shortcut out to ContainerControl version if it is a container
            ContainerControl cont = ctl as ContainerControl;
            if (cont != null)
                return FindFocusedControl(cont);

            // If the current control has no children there can be no focused control
            if (ctl.Controls.Count == 0)
                return null;

            // Go through each child control and do a depth-first search for the
            // focused control within each one.
            foreach (Control c in ctl.Controls)
            {
                Control deeperControl = FindFocusedControl(c);
                if (deeperControl != null)
                    return deeperControl;
            }
            return null;
        }

        /// <summary>
        /// Finds the active control recursively within the given container
        /// control.
        /// </summary>
        /// <param name="cont">The container for which the focused control is
        /// required.</param>
        /// <returns>The focused control within the given container or null if no
        /// focused control was found.</returns>
        public static Control FindFocusedControl(ContainerControl cont)
        {
            if (cont == null)
                throw new ArgumentNullException(nameof(cont));

            // Go through the active controls - these stop at container controls
            // such that for a Form -> SplitContainer -> Button, Form.ActiveControl
            // would be the SplitContainer and SplitContainer.ActiveControl would
            // be the button - we want to navigate down to the lowest container
            // and get the active control from that.
            Control active = cont.ActiveControl;
            while (active != null)
            {
                cont = active as ContainerControl;
                // If active control is not a container, then we've hit bottom
                // return 'active' as it is the focused control or null if the
                // container doesn't contain the focused control.
                if (cont == null)
                    return active;

                // Otherwise we've hit an intermediate container control - go deeper
                active = cont.ActiveControl;
            }

            // Shouldn't actually be able to get here... perhaps that should
            // be a while(true) loop.
            return null;
        }

        /// <summary>
        /// Checks if one bitmap represents the same image as another. Note that this
        /// requires the bitmaps to be the same size and have the same
        /// <see cref="Bitmap.PixelFormat">PixelFormat</see> value.
        /// </summary>
        /// <param name="img1">The image on which the match test is requested</param>
        /// <param name="img2">The image to test against this image</param>
        /// <returns>true to indicate that the two images match, false otherwise.
        /// </returns>
        public static bool Matches(this Bitmap img1, Bitmap img2)
        {
            if (object.ReferenceEquals(img1, img2))
                return true;
            if (img1 == null || img2 == null)
                return false;
            if (img1.Size != img2.Size || img1.PixelFormat != img2.PixelFormat)
                return false;

            unsafe
            {
                var rect = new Rectangle(new Point(0, 0), img1.Size);
                BitmapData data1 = null, data2 = null;

                try
                {
                    data1 = img1.LockBits(
                        rect, ImageLockMode.ReadOnly, img1.PixelFormat);
                    data2 = img2.LockBits(
                        rect, ImageLockMode.ReadOnly, img2.PixelFormat);
                    int* p1 = (int*)data1.Scan0.ToPointer();
                    int* p2 = (int*)data2.Scan0.ToPointer();

                    int bytesPerPix = Image.GetPixelFormatSize(img1.PixelFormat) / 8;
                    int count = img1.Height * (data1.Stride / bytesPerPix);

                    for (int i = 0; i < count; i++)
                    {
                        if (*p1++ != *p2++)
                            return false;
                    }
                    return true;
                }
                finally
                {
                    if (data2 != null)
                    {
                        try { img2.UnlockBits(data2); }
                        catch (Exception ex)
                        {
                            NLog.Error(ex);
                        }

                    }
                    if (data1 != null)
                    {
                        try { img1.UnlockBits(data1); }
                        catch (Exception ex)
                        {
                            NLog.Error(ex);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Checks if one bitmap represents the same image as another. Note that this
        /// requires the bitmaps to be the same size and have the same
        /// <see cref="Bitmap.PixelFormat">PixelFormat</see> value.
        /// </summary>
        /// <param name="img1">The image on which the match test is requested</param>
        /// <param name="img2">The image to test against this image; note that if
        /// this image is not an instance of <see cref="Bitmap"/>, then it will not
        /// match.</param>
        /// <returns>true to indicate that the two images match, false otherwise.
        /// </returns>
        public static bool Matches(this Bitmap img1, Image img2)
        {
            return Matches(img1, img2 as Bitmap);
        }

        /// <summary>
        /// Gets the root node for the given treenode, returning the given value if
        /// it has no parent nodes.
        /// </summary>
        /// <param name="n">The node for which the root, ie. the node in its
        /// ancestry which has no parent, is required.</param>
        /// <returns>The first node from this node up through its ancestry which has
        /// no parent node. This will be the given node if it is itself a root node.
        /// It will return null if null is passed.</returns>
        public static TreeNode GetRootNode(this TreeNode n)
        {
            if (n == null)
                return null;
            while (n.Parent != null)
                n = n.Parent;
            return n;
        }

        /// <summary>
        /// Gets the nearest ancestor in the tree above *or including* this node
        /// which satisfies a supplied predicate.
        /// </summary>
        /// <param name="n">The node to search from</param>
        /// <param name="pred">The predicate which determines the closest matched
        /// node to return</param>
        /// <returns>The first node found, starting from this node and working up
        /// through its ancestry, which satisfies the given predicate or null if no
        /// such node was found.</returns>
        public static TreeNode GetClosestMatch(
            this TreeNode n, Predicate<TreeNode> pred)
        {
            while (n != null)
            {
                if (pred(n))
                    return n;
                n = n.Parent;
            }
            return null;
        }

        /// <summary>
        /// Gets the nearest ancestor in the tree above this node which satisfies a
        /// supplied predicate.
        /// </summary>
        /// <param name="n">The node to search above</param>
        /// <param name="pred">The predicate which determines the closest ancestor
        /// to return</param>
        /// <returns>the first node in the ancestry above this node which satisfies
        /// the given predicate</returns>
        public static TreeNode GetClosestAncestor(
            this TreeNode n, Predicate<TreeNode> pred)
        {
            return (n == null ? null : n.Parent.GetClosestMatch(pred));
        }

        /// <summary>
        /// Checks if the current process is visual studio, which generally means
        /// that any controls are being executed by the visual studio designer, and
        /// not by the runtime Automate client. This works similarly to the protected
        /// <see cref="Control"/> property 'DesignMode', but it also works in
        /// constructors and paint event handlers.
        /// </summary>
        /// <returns>True if the current process is Visual Studio, false otherwise.
        /// </returns>
        public static bool IsInVisualStudio
        {
            get
            {
                using (Process proc = Process.GetCurrentProcess())
                {
                    return (proc.ProcessName == "devenv");
                }
            }
        }

        /// <summary>
        /// Translates the given content alignment value into a horizontal alignment
        /// value. Note that <see cref="HorizontalAlignment.Left"/> is effectively
        /// the default - eg. if you pass in
        /// <see cref="DataGridViewContentAlignment.None"/>, it will return
        /// <see cref="HorizontalAlignment.Left"/>.
        /// </summary>
        /// <param name="align">The content alignment value to translate</param>
        /// <returns>The corresponding horizontal alignment value</returns>
        /// <remarks>Note that this doesn't take into account the fact that the
        /// content alignment enum is valued as if it supported flags, mainly because
        /// I didn't know how eg. (TopRight | MiddleCenter) would translate into
        /// anything sensible.</remarks>
        public static HorizontalAlignment TranslateToHorizontal(
            this DataGridViewContentAlignment align)
        {
            switch (align)
            {
                case DataGridViewContentAlignment.TopRight:
                case DataGridViewContentAlignment.MiddleRight:
                case DataGridViewContentAlignment.BottomRight:
                    return HorizontalAlignment.Right;

                case DataGridViewContentAlignment.TopCenter:
                case DataGridViewContentAlignment.MiddleCenter:
                case DataGridViewContentAlignment.BottomCenter:
                    return HorizontalAlignment.Center;

                default:
                    return HorizontalAlignment.Left;
            }
        }

        /// <summary>
        /// Checks if the textbox's caret is at the end of the text in a textbox
        /// with no text selected, using the textbox's right-to-left property.
        /// </summary>
        /// <param name="box">The text box to check</param>
        /// <returns>true if the box has no selected text and the caret is sat at the
        /// end of the text; false otherwise.</returns>
        public static bool IsCaretAtEndOfText(this TextBox box)
        {
            return IsCaretAtEndOfText(box, box.RightToLeft);
        }

        /// <summary>
        /// Checks if the textbox's caret is at the end of the text in a textbox
        /// with no text selected.
        /// </summary>
        /// <param name="box">The text box to check</param>
        /// <param name="rtl">The right-to-left value to use</param>
        /// <returns>true if the box has no selected text and the caret is sat at the
        /// end of the text; false otherwise.</returns>
        public static bool IsCaretAtEndOfText(
            this TextBox box, RightToLeft rtl)
        {
            // If anything is selected, the caret is not (just) at the end of text
            if (box.SelectionLength != 0)
                return false;

            // Check for right-to-left text first
            if (rtl == RightToLeft.Yes)
                return (box.SelectionStart == 0);

            // Fall back on left-to-right for any other value
            return (box.SelectionStart == box.Text.Length);
        }

        /// <summary>
        /// Checks if the textbox's caret is at the start of the text in a textbox
        /// with no text selected, using the textbox's right-to-left property.
        /// </summary>
        /// <param name="box">The text box to check</param>
        /// <returns>true if the box has no selected text and the caret is sat at the
        /// start of the text; false otherwise.</returns>
        public static bool IsCaretAtStartOfText(this TextBox box)
        {
            return IsCaretAtStartOfText(box, box.RightToLeft);
        }

        /// <summary>
        /// Checks if the textbox's caret is at the start of the text in a textbox
        /// with no text selected.
        /// </summary>
        /// <param name="box">The text box to check</param>
        /// <param name="rtl">The right-to-left value to use</param>
        /// <returns>true if the box has no selected text and the caret is sat at the
        /// start of the text; false otherwise.</returns>
        public static bool IsCaretAtStartOfText(
            this TextBox box, RightToLeft rtl)
        {
            // If anything is selected, the caret is not (just) at the end of text
            if (box.SelectionLength != 0)
                return false;

            // Check for right-to-left text first
            if (rtl == RightToLeft.Yes)
                return (box.SelectionStart == box.Text.Length);

            // Fall back on left-to-right for any other value
            return (box.SelectionStart == 0);

        }
    }
}
