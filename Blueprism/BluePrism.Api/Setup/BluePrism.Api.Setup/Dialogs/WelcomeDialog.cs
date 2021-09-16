namespace BluePrism.Api.Setup.Dialogs
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using BluePrism.Api.Setup;
    using BluePrism.Api.Setup.Common;
    using global::WixSharp.UI.Forms;

    /// <summary>
    /// The standard Welcome dialog
    /// </summary>
    public partial class WelcomeDialog : ManagedForm // change ManagedForm->Form if you want to show it in designer
    {
        private bool isLoaded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeDialog"/> class.
        /// </summary>
        public WelcomeDialog()
        {
            InitializeComponent();
        }

        void WelcomeDialog_Load(object sender, EventArgs e)
        {
            var sortedCultures = LanguageConstants.SupportedLanguages.Select(x => new CultureInfo(x)).OrderBy(x => x.NativeName).ToArray();
            _languageCombo.Items.AddRange(sortedCultures);

            _languageCombo.DisplayMember = "NativeName";

            _languageCombo.SelectedIndex = sortedCultures
                .Select((x, i) => (Culture: x, Index: i))
                .SingleOrDefault(x => x.Culture.Name.Equals(Runtime.Session[MsiProperties.Locale], StringComparison.OrdinalIgnoreCase))
                .Index;

            image.Image = Runtime.Session.GetResourceBitmap("dialog");

            isLoaded = true;
            ResetLayout();
        }

        void ResetLayout()
        {
            // The form controls are properly anchored and will be correctly resized on parent form
            // resizing. However the initial sizing by WinForm runtime doesn't a do good job with DPI
            // other than 96. Thus manual resizing is the only reliable option apart from going WPF.

            var bHeight = (int)(next.Height * 2.3);

            var upShift = bHeight - bottomPanel.Height;
            bottomPanel.Top -= upShift;
            bottomPanel.Height = bHeight;

            imgPanel.Height = this.ClientRectangle.Height - bottomPanel.Height;
            float ratio = (float)image.Image.Width / (float)image.Image.Height;
            image.Width = (int)(image.Height * ratio);

            textPanel.Left = image.Right + 5;
            textPanel.Width = (bottomPanel.Width - image.Width) - 10;
        }

        void cancel_Click(object sender, EventArgs e)
        {
            Shell.Cancel();
        }

        void next_Click(object sender, EventArgs e)
        {
            Shell.GoNext();
        }

        void back_Click(object sender, EventArgs e)
        {
            Shell.GoPrev();
        }

        private void _languageCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedLocale = ((CultureInfo)_languageCombo.SelectedItem).Name;

            Runtime.Session[MsiProperties.Locale] = selectedLocale;

            var culture = new CultureInfo(selectedLocale);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            if (isLoaded)
            {
                InitializeComponent();
                Shell.GoTo(0);
            }
        }
    }
}
