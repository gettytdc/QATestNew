using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using AutomateControls.WindowsSupport;
using BluePrism.Server.Domain.Models;

namespace AutomateControls
{
    /// <summary>
    /// A number of Windows utility methods.
    /// </summary>
    public static class WinUtil
    {
        #region - P/Invoke -

        /// <summary>
        /// Sends a windows message with the a boolean WPARAM value the specified
        /// window handle
        /// </summary>
        /// <param name="hWnd">A window handle to which the message should be sent
        /// </param>
        /// <param name="wMsg">The message ID</param>
        /// <param name="wParam">The WPARAM of the message</param>
        /// <param name="lParam">The LPARAM of the message</param>
        /// <returns>You know, I'm not entirely sure.</returns>
        /// <remarks>This is currently only used within this class to send a
        /// <see cref="WindowsMessage.WM_SETREDRAW"/> message.</remarks>
        [DllImport("user32.dll", SetLastError=true)]
        private static extern int SendMessage(
            IntPtr hWnd, int wMsg, bool wParam, int lParam);

        /// <summary>
        /// Sends a windows message
        /// </summary>
        /// <param name="hWnd">A window handle to which the message should be sent
        /// </param>
        /// <param name="wMsg">The message ID</param>
        /// <param name="wParam">The WPARAM of the message</param>
        /// <param name="lParam">The LPARAM of the message</param>
        /// <returns>You know, I'm not entirely sure.</returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SendMessage(
            IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region - Suspend/Resume Drawing -

        /// <summary>
        /// Class to wrap the suspend / resume drawing mechanism into an IDisposable
        /// so that a using block can be used in place of a 'suspend/try/finally'
        /// block.
        /// </summary>
        private class DrawingLocker : IDisposable
        {
            // The control on which drawing should be locked
            private Control _ctl;

            /// <summary>
            /// Creates a new drawing locker for the given control
            /// </summary>
            /// <param name="ctl">The control for which drawing should be suspended
            /// while this locker object remains undisposed.</param>
            /// <exception cref="ArgumentNullException">If the given control is null.
            /// </exception>
            /// <exception cref="InvalidStateException">If the control's handle has
            /// not been created.</exception>
            public DrawingLocker(Control ctl)
            {
                if (ctl == null) throw new ArgumentNullException(nameof(ctl));
                _ctl = ctl;
                WinUtil.SuspendDrawing(_ctl);
            }

            /// <summary>
            /// Ensures that the control whose drawing is locked by this object is
            /// freed when this object is garbage collected
            /// </summary>
            ~DrawingLocker()
            {
                Dispose();
            }

            #region IDisposable Members

            /// <summary>
            /// Disposes of this locker, resuming the drawing on the registered
            /// control
            /// </summary>
            public void Dispose()
            {
                WinUtil.TryResumeDrawing(_ctl);
            }

            #endregion
        }

        /// <summary>
        /// Locks the drawing of the given control, returning a disposable instance
        /// which can be disposed of to resume the drawing.
        /// </summary>
        /// <param name="ctl">The control to lock drawing on</param>
        /// <returns>An IDisposable on which <see cref="IDisposable.Dispose"/> should
        /// be called to resume drawing on the specified control. Typically, this
        /// would go into a <c>using</c> block to ensure that drawing state is
        /// resumed, regardless of any errors which may occur.</returns>
        /// <exception cref="ArgumentNullException">If the given control is null.
        /// </exception>
        /// <exception cref="InvalidStateException">If the control's handle has
        /// not been created.</exception>
        public static IDisposable LockDrawing(Control ctl)
        {
            return new DrawingLocker(ctl);
        }

        /// <summary>
        /// Enables or disables drawing of the given control.
        /// This works by sending the Win32 message WM_SETREDRAW to the control with a value
        /// of False (disable redrawing) or True (enable redrawing). It can be used to
        /// inhibit the 'jumble of controls' look of a dynamically added panel to a control.
        /// </summary>
        /// <param name="ctl">The control on which drawing should be enabled / disabled.
        /// </param>
        /// <param name="on">True to enable drawing on the control. This will force a 
        /// <see cref="Control.Refresh()">Refresh()</see> on the control to ensure that
        /// the control is repainted correctly; false to disable drawing on the control.
        /// </param>
        /// <exception cref="ArgumentNullException">If the given control is null.
        /// </exception>
        /// <exception cref="InvalidStateException">If the control's handle has not
        /// been created.</exception>
        public static void SetDrawingEnabled(Control ctl, bool on)
        {
            if (ctl == null)
                throw new ArgumentNullException(nameof(ctl));

            if (!ctl.IsHandleCreated)
            {
                throw new InvalidStateException(
                    "Control '{0}' has no handle. Cannot set drawing mode",
                    ctl.Name);
            }

            SendMessage(ctl.Handle, (int)WindowsMessage.WM_SETREDRAW, on, 0);
        }

        /// <summary>
        /// Suspends drawing on the given control.
        /// This will suspend drawing until a corresponding <see cref="ResumeDrawing"/>
        /// call is made.
        /// </summary>
        /// <param name="ctl">The control on which drawing should be suspended. This
        /// must have a window handle associated with it.</param>
        /// <exception cref="ArgumentNullException">If the given control is null.
        /// </exception>
        /// <exception cref="InvalidStateException">If the control's handle has not
        /// been created.</exception>
        public static void SuspendDrawing(Control ctl)
        {
            SetDrawingEnabled(ctl, false);
        }

        /// <summary>
        /// Resumes drawing on the given control.
        /// This will resume drawing after a corresponding <see cref="SuspendDrawing"/>
        /// call has been made.
        /// </summary>
        /// <param name="ctl">The control on which drawing should be resumed. This
        /// must have a window handle associated with it.</param>
        /// <exception cref="ArgumentNullException">If the given control is null.
        /// </exception>
        /// <exception cref="ArgumentException">If the control's handle has not been
        /// created.</exception>
        public static void ResumeDrawing(Control ctl)
        {
            SetDrawingEnabled(ctl, true);
            // Force a redraw to ensure that the control is painted again.
            ctl.Refresh();
        }

        /// <summary>
        /// Tries to suspend drawing on the given control, reporting success or
        /// failure but not raising an exception if failure occurs.
        /// </summary>
        /// <param name="ctl">The control on which drawing should be resumed.</param>
        /// <returns>true if the operation completed successfully; false otherwise.
        /// </returns>
        public static bool TrySuspendDrawing(Control ctl)
        {
            try
            {
                SuspendDrawing(ctl);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Tries to resume drawing on the given control, reporting success or
        /// failure but not raising an exception if failure occurs.
        /// </summary>
        /// <param name="ctl">The control on which drawing should be resumed.</param>
        /// <returns>true if the operation completed successfully; false otherwise.
        /// </returns>
        public static bool TryResumeDrawing(Control ctl)
        {
            try
            {
                ResumeDrawing(ctl);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region - FindControls -

        /// <summary>
        /// Recursively finds all controls of the specified type below the given root
        /// </summary>
        /// <typeparam name="T">The type of control to search for</typeparam>
        /// <param name="root">The root control under which to search - note that the
        /// root control is not itself tested to see if it is the required type.
        /// </param>
        /// <returns>An enumeration of the found controls beneath the given control.
        /// </returns>
        public static IEnumerable<T> FindControls<T>(Control root) where T : Control
        {
            return FindControls<T>(root, true);
        }

        /// <summary>
        /// Finds all controls of the specified type below the given root, recursing
        /// as specified.
        /// </summary>
        /// <typeparam name="T">The type of control to search for</typeparam>
        /// <param name="root">The root control under which to search - note that the
        /// root control is not itself tested to see if it is the required type.
        /// </param>
        /// <param name="recurse">True to recurse through all descendant controls.
        /// </param>
        /// <returns>An enumeration of the found controls beneath the given control.
        /// </returns>
        public static IEnumerable<T> FindControls<T>(
            Control root, bool recurse) where T:Control
        {
            return FindControls(root, recurse, new List<T>());
        }

        /// <summary>
        /// Finds all controls of the specified type below the given root, recursing
        /// as specified and adds them to the given collection, which is then
        /// returned.
        /// </summary>
        /// <typeparam name="T">The type of control to search for</typeparam>
        /// <param name="root">The root control under which to search - note that the
        /// root control is not itself tested to see if it is the required type.
        /// </param>
        /// <param name="recurse">True to recurse through all descendant controls.
        /// </param>
        /// <param name="coll">The collection into which the controls should be added
        /// </param>
        /// <returns>An enumeration of the found controls beneath the given control.
        /// </returns>
        public static ICollection<T> FindControls<T>(
            Control root, bool recurse, ICollection<T> coll) where T:Control
        {
            foreach (Control ctl in root.Controls)
            {
                if (ctl is T)
                    coll.Add((T)ctl);
                if (recurse)
                    FindControls(ctl, true, coll);
            }
            return coll;
        }

        #endregion

        #region - Suspend/Resume All Layout -

        /// <summary>
        /// Suspends layout on the given control and recursively on all its
        /// descendents.
        /// </summary>
        /// <param name="ctl">The control on which layout should be suspended</param>
        public static void SuspendAllLayout(Control ctl)
        {
            ctl.SuspendLayout();
            foreach (Control child in ctl.Controls)
            {
                SuspendAllLayout(child);
            }
        }

        /// <summary>
        /// Resumes layout on the given control and recursively on all its
        /// descendents, optionally forcing a layout after it has resumed.
        /// </summary>
        /// <param name="ctl">The control on which all layout should be resumed.
        /// </param>
        /// <param name="performLayout">True to perform a layout operation
        /// immediately after resuming layout on a control</param>
        public static void ResumeAllLayout(Control ctl, bool performLayout)
        {
            foreach (Control child in ctl.Controls)
            {
                ResumeAllLayout(child, performLayout);
            }
            ctl.ResumeLayout(performLayout);
        }

        #endregion

    }
}
