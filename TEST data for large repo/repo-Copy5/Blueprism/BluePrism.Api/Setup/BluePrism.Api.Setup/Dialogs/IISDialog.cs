namespace BluePrism.Api.Setup.Dialogs
{
    using System;
    using System.Linq;
    using Common;
    using Common.IIS;
    using global::WixSharp.UI.Forms;
    using static Common.IIS.SslCertificateReader;

    public partial class IISDialog : ManagedForm
    {
        public IISDialog()
        {
            InitializeComponent();
        }

        private void dialog_Load(object sender, EventArgs e)
        {
            banner.Image = Runtime.Session.GetResourceBitmap("banner");

            cmbSSLCertificate.DataSource = GetInstallerCertificates();
            cmbSSLCertificate.ValueMember = "CertificateDisplayName";
            cmbSSLCertificate.SelectedIndex = GetCertificateIndexFromCertificateId(Runtime.Session[MsiProperties.ApiIisSslCertificateId]);

            txtSiteName.Text = WebAppProperties.WebAppName;
            txtHostName.Text = Runtime.Session[MsiProperties.ApiIisHostname];
            numPort.Value = int.TryParse(Runtime.Session[MsiProperties.ApiIisPort], out var port) ? port : 1;
        }

        private void next_Click(object sender, EventArgs e)
        {
            Runtime.Session[MsiProperties.ApiIisHostname] = txtHostName.Text;
            Runtime.Session[MsiProperties.ApiIisPort] = numPort.Text;
            Runtime.Session[MsiProperties.ApiIisSslCertificateId] = GetCertificateIdFromCertificateSelectedIndex(cmbSSLCertificate.SelectedIndex);

            Shell.GoNext();
        }

        private int GetCertificateIndexFromCertificateId(string certificateId)
        {
            var certificate = cmbSSLCertificate
                .Items
                .Cast<InstallerCertificate>()
                .SingleOrDefault(x => x.Id.Equals(certificateId, StringComparison.OrdinalIgnoreCase));

            const int selectedFirstItemIndex = 0;

            return certificate != null
                ? cmbSSLCertificate.Items.IndexOf(certificate)
                : selectedFirstItemIndex;
        }

        private static string GetCertificateIdFromCertificateSelectedIndex(int selectedIndex) =>
           GetInstallerCertificates()[selectedIndex].Id;

        private void cancel_Click(object sender, EventArgs e)
        {
            Shell.Cancel();
        }

        private void back_Click(object sender, EventArgs e)
        {
            Shell.GoPrev();
        }

        private void OnFormFieldsChanged(object sender, EventArgs e)
        {
            next.Enabled = IsValidForm();
        }

        private bool IsValidForm() =>
            !string.IsNullOrEmpty(txtSiteName.Text)
            && !string.IsNullOrEmpty(txtHostName.Text)
            && !string.IsNullOrEmpty(numPort.Value.ToString())
            && numPort.Value > 0;
    }
}
