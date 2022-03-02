using AutomateControls.Properties;
using System.Windows.Forms;

namespace AutomateControls
{
    /// <summary>
    /// Straightforward error message box with a simple set of params as possible.
    /// </summary>
    public static class ErrorBox
    {
        /// <summary>
        /// Shows an error box with a specified message
        /// </summary>
        /// <param name="msg">The message to display in the error box</param>
        public static void Show(string msg)
        {
            Show(null, msg);
        }

        /// <summary>
        /// Shows an error box with a formatted message
        /// </summary>
        /// <param name="msg">The message to display in the error box</param>
        /// <param name="args">The arguments to insert into the message's
        /// placeholders</param>
        public static void Show(string msg, params object[] args)
        {
            Show(null, msg, args);
        }

        /// <summary>
        /// Shows an error box with a formatted message
        /// </summary>
        /// <param name="owner">The owner window to hold the messagebox</param>
        /// <param name="msg">The message to display in the error box</param>
        /// <param name="args">The arguments to insert into the message's
        /// placeholders</param>
        public static void Show(IWin32Window owner, string msg, params object[] args)
        {
            Show(owner, string.Format(msg, args));
        }

        /// <summary>
        /// Shows an error box with a specified message
        /// </summary>
        /// <param name="owner">The owner window to hold the messagebox</param>
        /// <param name="msg">The message to display in the error box</param>
        public static void Show(IWin32Window owner, string msg)
        {
            MessageBox.Show(owner, msg, Resources.ErrorBox_Error,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
