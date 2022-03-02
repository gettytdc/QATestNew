using BluePrism.CharMatching.Properties;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using AutomateControls.Forms;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Statically held configuration for character matching.
    /// </summary>
    public static class FontConfig
    {
        // The default directory to use as the search directory for font files
        private static string s_defaultDir =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // The specified directory to use to look for font files
        private static string s_dir;

        /// <summary>
        /// The font directory to search for user fonts. User fonts override the
        /// behaviour of existing fonts, where a name conflict exists, or add new
        /// font functionality.
        /// </summary>
        /// <remarks>If no specific directory is set, this will return the directory
        /// in which this DLL resides.</remarks>
        public static string Directory
        {
            get { return (string.IsNullOrEmpty(s_dir) ? s_defaultDir : s_dir); }
            set { s_dir = (value ?? "").Trim(); }
        }

        /// <summary>
        /// Validates the environment for running general character matching
        /// functionality, prompting the user if the environment is invalid
        /// in some way.
        /// </summary>
        /// <param name="owner">The owner control for the validation check - this
        /// is used as the owner of any windows prompts which must be shown to
        /// the user. If it is null, no windows prompts will be given.</param>
        /// <returns>True if the environment is valid and the character
        /// matching can continue; False otherwise.</returns>
        public static bool ValidateEnvironment(Control owner)
        {
            // Handle font smoothing being enabled by telling the user to disable
            // it; We offer to open the control panel applet with which font
            // smoothing can be disabled.
            if (SystemInformation.IsFontSmoothingEnabled)
            {
                if (owner == null)
                    return false;

                var res = BPMessageBox.ShowDialog(
                    Resources.FontsCannotBeProperlyGeneratedBecauseFontSmoothingIsCurrentlyEnabledOnThisSyste,
                    Resources.FontSmoothingEnabled,
                    MessageBoxButtons.YesNo);

                if (res != DialogResult.Yes)
                    return false;

                // From http://vlaurie.com/computers2/Articles/control.htm
                // We can open the System Control Panel applet and go straight
                // into the 'Advanced' tab using 'control sysdm.cpl,,3'
                try
                {
                    using (Process proc = Process.Start(
                        Path.Combine(Environment.SystemDirectory, "control.exe"),
                        "sysdm.cpl,,3"))
                    {
                        // All we want to do is open it, once that's done we can
                        // lose all ties to the process, so just implicitly Dispose()
                        // and return.
                        return false;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(owner, string.Format(
                        Resources.AnErrorOccurredTryingToOpenTheControlPanel0,
                        e.Message), Resources.Error, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }

            }

            return true;
        }

    }
}
