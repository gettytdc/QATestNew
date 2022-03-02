using System.ComponentModel;
using System.Diagnostics;
using WixSharp;

namespace BluePrism.Setup.Dialogs
{
    /// <summary>
    /// The standard Exit dialog
    /// </summary>
    public partial class ExitDialog : BaseDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExitDialog"/> class.
        /// </summary>
        public ExitDialog()
        {
            HideBackButton();
            InitializeComponent();
        }

        private void ExitDialog_Load(object sender, System.EventArgs e)
        {
            //resize the subtitle
            var helpers = new Helpers(Shell);
            helpers.ApplySubtitleAppearance(Subtitle);

            if (helpers.IsPatchUpgrade(Runtime.ProductVersion))
            {
                Subtitle.Text = Properties.Resources.UpdateCompleteSubtitle;
                Subtitle.Height = 64;
            }
        }

        private void NextButton_Click(object sender, System.EventArgs e)
        {
            string automateSPath = Runtime.InstallDir.PathCombine("automates.exe");
            string automatePath = Runtime.InstallDir.PathCombine("automate.exe");

            try
            {
                if (System.IO.File.Exists(automateSPath))
                    Process.Start(automateSPath);
                else if (System.IO.File.Exists(automatePath))
                    Process.Start(automatePath);
            }
            catch (Win32Exception)
            {
                //Do nothing, probably caused by a system error that the user will have dealt with already
                ;
            }



            Shell.Exit();
        }
    }
}