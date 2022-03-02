namespace BluePrism.Api.Setup.Dialogs
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Common;
    using global::WixSharp;
    using global::WixSharp.UI.Forms;

    /// <summary>
    /// The standard Maintenance Type dialog
    /// </summary>
    public partial class MaintenanceTypeDialog : ManagedForm
    {
        private bool _isLoaded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceTypeDialog"/> class.
        /// </summary>
        public MaintenanceTypeDialog()
        {
            InitializeComponent();
            label1.MakeTransparentOn(banner);
            label2.MakeTransparentOn(banner);
        }

        Type ProgressDialog
        {
            get
            {
                return Shell.Dialogs
                    .FirstOrDefault(d => d.GetInterfaces().Contains(typeof(IProgressDialog)));
            }
        }

        void change_Click(object sender, System.EventArgs e)
        {
            Runtime.Session["MODIFY_ACTION"] = "Change";
            Shell.GoNext();
        }

        void repair_Click(object sender, System.EventArgs e)
        {
            Runtime.Session["MODIFY_ACTION"] = "Repair";
            int index = Shell.Dialogs.IndexOf(ProgressDialog);
            if (index != -1)
                Shell.GoTo(index);
            else
                Shell.GoNext();
        }

        void remove_Click(object sender, System.EventArgs e)
        {
            Runtime.Session["REMOVE"] = "ALL";
            Runtime.Session["MODIFY_ACTION"] = "Remove";

            int index = Shell.Dialogs.IndexOf(ProgressDialog);
            if (index != -1)
                Shell.GoTo(index);
            else
                Shell.GoNext();
        }

        void back_Click(object sender, System.EventArgs e)
        {
            Shell.GoPrev();
        }

        void next_Click(object sender, System.EventArgs e)
        {
            Shell.GoNext();
        }

        void cancel_Click(object sender, System.EventArgs e)
        {
            Shell.Cancel();
        }

        void MaintenanceTypeDialog_Load(object sender, System.EventArgs e)
        {
            var sortedCultures = LanguageConstants.SupportedLanguages.Select(x => new CultureInfo(x)).OrderBy(x => x.NativeName).ToArray();
            _languageCombo.Items.AddRange(sortedCultures);

            _languageCombo.DisplayMember = "NativeName";

            _languageCombo.SelectedIndex = sortedCultures
                .Select((x, i) => (Culture: x, Index: i))
                .SingleOrDefault(x => x.Culture.Name.Equals(Runtime.Session[MsiProperties.Locale], StringComparison.OrdinalIgnoreCase))
                .Index;

            banner.Image = Runtime.Session.GetResourceBitmap("WixUI_Bmp_Banner");

            _isLoaded = true;
            ResetLayout();
        }

        void ResetLayout()
        {
            float ratio = (float)banner.Image.Width / (float)banner.Image.Height;
            topPanel.Height = (int)(banner.Width / ratio);

            var upShift = (int)(next.Height * 2.3) - topPanel.Height;
            bottomPanel.Top -= upShift;
            bottomPanel.Height += upShift;
        }

        private void _languageCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedLocale = ((CultureInfo)_languageCombo.SelectedItem).Name;

            Runtime.Session[MsiProperties.Locale] = selectedLocale;

            var culture = new CultureInfo(selectedLocale);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            if (_isLoaded)
            {
                InitializeComponent();
                Shell.GoTo(0);
            }
        }
    }
}
