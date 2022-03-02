using AutomateControls.Properties;
using System;
using System.Windows.Forms;
using BluePrism.Server.Domain.Models;
using System.Runtime.InteropServices;

namespace AutomateControls
{
    /// <summary>
    /// Extension methods for UI/Windows use
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Checks if a drag event args indicates that a key state is set.
        /// </summary>
        /// <param name="e">The drag event args to test</param>
        /// <param name="state">The state to check for</param>
        /// <returns>true if the drag event args indicates that
        /// <paramref name="state"/> is set.</returns>
        public static bool HasKeyState(this DragEventArgs e, DragKeyState state)
        {
            return ((DragKeyState)e.KeyState).HasFlag(state);
        }

        /// <summary>
        /// Checks if the window that owns this control is the current foreground window.
        /// </summary>
        /// <param name="control">The control which belongs to the window to be checked.</param>
        public static bool HasWindowFocus(this Control control)
        {
            return control.Invoke(() =>
            {
                try
                {
                    // check if the current process id is equal to the process id of the foreground window
                    var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                    GetWindowThreadProcessId(GetForegroundWindow(), out int foregroundPID);

                    return foregroundPID == pid;
                }
                catch { return false; }
            });
        }

        #region - Control.Invoke() Methods -

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void Invoke(this Control c, Action a)
        {
            c.Invoke((Delegate)a);
        }

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void Invoke<T1>(this Control c, Action<T1> a)
        {
            c.Invoke((Delegate)a);
        }

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void Invoke<T1, T2>(this Control c, Action<T1, T2> a)
        {
            c.Invoke((Delegate)a);
        }

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void Invoke<T1, T2, T3>(this Control c, Action<T1, T2, T3> a)
        {
            c.Invoke((Delegate)a);
        }

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void Invoke<T1, T2, T3, T4>(
            this Control c, Action<T1, T2, T3, T4> a)
        {
            c.Invoke((Delegate)a);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult Invoke<TResult>(this Control c, Func<TResult> a)
        {
            return (TResult)c.Invoke((Delegate)a);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="f">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult Invoke<T, TResult>(this Control c, Func<T, TResult> f)
        {
            return (TResult)c.Invoke((Delegate)f);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="f">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult Invoke<T1, T2, TResult>(
            this Control c, Func<T1, T2, TResult> f)
        {
            return (TResult)c.Invoke((Delegate)f);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="f">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult Invoke<T1, T2, T3, TResult>(
            this Control c, Func<T1, T2, T3, TResult> f)
        {
            return (TResult)c.Invoke((Delegate)f);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="f">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult Invoke<T1, T2, T3, T4, TResult>(
            this Control c, Func<T1, T2, T3, T4, TResult> f)
        {
            return (TResult)c.Invoke((Delegate)f);
        }

        #endregion

        #region - Control.BeginInvoke() Methods -

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void BeginInvoke(this Control c, Action a)
        {
            c.BeginInvoke((Delegate)a);
        }

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void BeginInvoke<T1>(this Control c, Action<T1> a)
        {
            c.BeginInvoke((Delegate)a);
        }

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void BeginInvoke<T1, T2>(this Control c, Action<T1, T2> a)
        {
            c.BeginInvoke((Delegate)a);
        }

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void BeginInvoke<T1, T2, T3>(this Control c, Action<T1, T2, T3> a)
        {
            c.BeginInvoke((Delegate)a);
        }

        /// <summary>
        /// Invokes an action (typically a lambda) on a control
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The action to invoke.</param>
        public static void BeginInvoke<T1, T2, T3, T4>(
            this Control c, Action<T1, T2, T3, T4> a)
        {
            c.BeginInvoke((Delegate)a);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="a">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult BeginInvoke<TResult>(this Control c, Func<TResult> a)
        {
            return (TResult)c.BeginInvoke((Delegate)a);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="f">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult BeginInvoke<T, TResult>(this Control c, Func<T, TResult> f)
        {
            return (TResult)c.BeginInvoke((Delegate)f);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="f">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult BeginInvoke<T1, T2, TResult>(
            this Control c, Func<T1, T2, TResult> f)
        {
            return (TResult)c.BeginInvoke((Delegate)f);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="f">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult BeginInvoke<T1, T2, T3, TResult>(
            this Control c, Func<T1, T2, T3, TResult> f)
        {
            return (TResult)c.BeginInvoke((Delegate)f);
        }

        /// <summary>
        /// Invokes a function (typically a lambda) on a control; returns the result
        /// </summary>
        /// <param name="c">The control on which to invoke the action</param>
        /// <param name="f">The function to invoke.</param>
        /// <returns>The result from the function call after being invoked on the
        /// control.</returns>
        /// <typeparam name="TResult">The result type of the function call - ie. the
        /// type of the expected response</typeparam>
        public static TResult BeginInvoke<T1, T2, T3, T4, TResult>(
            this Control c, Func<T1, T2, T3, T4, TResult> f)
        {
            return (TResult)c.BeginInvoke((Delegate)f);
        }

        #endregion

        #region - DataGridView Methods -

        /// <summary>
        /// Gets the string value at a specified cell location.
        /// </summary>
        /// <param name="this">The DataGridView on which the cell resides.</param>
        /// <param name="rowIndex">The 0-based index of the row in which the cell
        /// resides.</param>
        /// <param name="colIndex">The 0-based index of the column in which the cell
        /// resides.</param>
        /// <returns>The value of the cell at the specified location, cast into a
        /// string.</returns>
        /// <exception cref="InvalidCastException">If the value at the specified
        /// location could not be cast into a string</exception>
        /// <exception cref="ArgumentOutOfRangeException">If
        /// <paramref name="rowIndex"/> or <paramref name="colIndex"/> were negative
        /// or beyond the end of the row or column respectively.</exception>
        public static string GetStringValue(
            this DataGridView @this, int rowIndex, int colIndex)
        {
            return @this.Rows[rowIndex].GetStringValue(colIndex);
        }

        /// <summary>
        /// Gets the string value at a specified cell location.
        /// </summary>
        /// <param name="this">The DataGridViewRow in which the cell resides.</param>
        /// <param name="column">The column in which the cell resides.</param>
        /// <returns>The value of the cell at the specified location, cast into a
        /// string.</returns>
        /// <exception cref="InvalidCastException">If the value at the specified
        /// location could not be cast into a string</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="column"/> is
        /// null.</exception>
        /// <exception cref="InvalidArgumentException">If the column is from a
        /// different <see cref="DataGridView"/> to the row.</exception>
        public static string GetStringValue(
            this DataGridViewRow @this, DataGridViewColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (column.DataGridView != @this.DataGridView)
                throw new InvalidArgumentException(
                    Resources.Extensions_GivenRowAndColumnAreFromDifferentDataGridViews);

            return @this.GetStringValue(column.Index);
        }

        /// <summary>
        /// Gets the string value at a specified cell location.
        /// </summary>
        /// <param name="this">The DataGridViewRow in which the cell resides.</param>
        /// <param name="colIndex">The 0-based index of the column in which the cell
        /// resides.</param>
        /// <returns>The value of the cell at the specified location, cast into a
        /// string.</returns>
        /// <exception cref="InvalidCastException">If the value at the specified
        /// location could not be cast into a string</exception>
        /// <exception cref="ArgumentOutOfRangeException">If
        /// <paramref name="colIndex"/> was negative or beyond the end of the column.
        /// </exception>
        public static string GetStringValue(
            this DataGridViewRow @this, int colIndex)
        {
            return (string)@this.Cells[colIndex].Value;            
        }

        #endregion

        #region - Extern Windows Functions - 

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        #endregion
    }
}
