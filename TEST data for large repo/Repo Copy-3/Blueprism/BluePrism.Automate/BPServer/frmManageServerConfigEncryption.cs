using AutomateControls.Forms;
using BluePrism.AutomateAppCore;
using BluePrism.BPServer.Properties;
using BluePrism.Common.Security;
using BluePrism.Common.Security.Exceptions;
using System;
using System.Linq;
using System.Windows.Forms;

namespace BluePrism.BPServer
{
    public partial class frmManageServerConfigEncryption : AutomateForm
    {

        public frmManageServerConfigEncryption()
        {
            InitializeComponent();
            lblCertificateWarning.Visible = rdoOwnCertificate.Checked;
        }

        public frmManageServerConfigEncryption(MachineConfig.ConfigEncryptionMethod configEncryptionMethod,string thumbPrint) : this()
        {
            EncryptionMethod = configEncryptionMethod;
            Thumbprint = thumbPrint;
        }

        private bool _forceLostFocusOnLoad = true;

        private DialogResult _dialogResult = DialogResult.Cancel;

        private MachineConfig.ConfigEncryptionMethod _encryptionMethod;

        public event StatusHandler StatusUpdate;

        public delegate void StatusHandler(string message, Enums.LoggingLevel level);  
        
        public CertificateErrorCode CertificateErrorCode { get; private set; }

        public MachineConfig.ConfigEncryptionMethod EncryptionMethod
        {
            get { return _encryptionMethod; }
            set
            {
                _encryptionMethod = value;
                RefreshDisplay();
            }
        }

        public string Thumbprint
        {
            get => new string(txtThumbprint.Text.Trim().Where(c => !char.IsControl(c)).ToArray());
            set => txtThumbprint.Text = value;
        }

        /// <summary>
        /// Refresh the display based on the current settings
        /// </summary>
        private void RefreshDisplay()
        {
            if (_encryptionMethod == MachineConfig.ConfigEncryptionMethod.BuiltIn)
            {
                rdoBPEncryption.Checked = true;
                rdoOwnCertificate.Checked = false;
            }
            else if (_encryptionMethod == MachineConfig.ConfigEncryptionMethod.OwnCertificate)
            {
                rdoBPEncryption.Checked = false;
                rdoOwnCertificate.Checked = true;
            }
        }

        private void CheckedChanged()
        {
            if(rdoOwnCertificate.Checked)
            {
                txtThumbprint.Enabled = true;
                labelOwnCertificate.Enabled = true;
                lblCertificateWarning.Visible = true;
            }
            else
            {
                labelOwnCertificate.Enabled = false;
                txtThumbprint.Enabled = false;
                txtThumbprint.Clear();
                lblCertificateWarning.Visible = false;
            }
        }

        private void ChkboxBPEncryption_CheckedChanged(object sender, EventArgs e) => CheckedChanged();

        private void ChkboxOwnCertificate_CheckedChanged(object sender, EventArgs e) => CheckedChanged();

        private void TxtThumbprint_Click(object sender, EventArgs e) => txtThumbprint.Text = Clipboard.GetText();

        private void RdoOwnCertificate_MouseClick(object sender, MouseEventArgs e) => ForceControlLoseFocus();

        private void RdoBPEncryption_MouseClick(object sender, MouseEventArgs e) => ForceControlLoseFocus();

        private void RdoBPEncryption_Leave(object sender, EventArgs e) => ForceControlLoseFocus();

        private void RdoOwnCertificate_Leave(object sender, EventArgs e) => ForceControlLoseFocus();

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (rdoOwnCertificate.Checked)
            {
                if (string.IsNullOrEmpty(txtThumbprint.Text))
                {
                    BPMessageBox.ShowDialog(Resources.EnterAThumbprint, Resources.Error, MessageBoxButtons.OK);
                    return;
                }
                EncryptionMethod = MachineConfig.ConfigEncryptionMethod.OwnCertificate;
                ValidateThumbprint();
            }
            else if (rdoBPEncryption.Checked)
            {
                EncryptionMethod = MachineConfig.ConfigEncryptionMethod.BuiltIn;
                _dialogResult = DialogResult.OK;
            }

            // user has chosen to abort, ignore the error prompt or the thumbprint was valid
            if (_dialogResult != DialogResult.Retry)
                Close();
        }

        private void ValidateThumbprint()
        {
            try
            {
                var certStore = new CertificateStoreService();

                certStore.ValidateThumbprint(Thumbprint);

                using (var certificate = certStore.GetCertificate(Thumbprint))
                    certStore.ValidateCert(certificate, CertificateStoreCheckFlags.PrivateKey);

                 _dialogResult = DialogResult.OK;
            }
            catch (CertificateException ex)
            {
                CertificateErrorCode = ex.CertificateErrorCode;

                switch (ex.CertificateErrorCode)
                {
                    case CertificateErrorCode.NotFound:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.NoCertificateExistsForThisThumbprint, Resources.TheCertificateIsNotValid, MessageBoxButtons.RetryCancel);
                        break;
                    case CertificateErrorCode.NotActiveYet:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.TheCertificateHasNotBeenActivated, Resources.TheCertificateIsNotValid, MessageBoxButtons.RetryCancel);
                        break;
                    case CertificateErrorCode.Expired:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.TheCertificateHasExpired, Resources.TheCertificateIsNotValid, MessageBoxButtons.RetryCancel);
                        break;
                    case CertificateErrorCode.NotUnique:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.MultipleCertificatesExistForThisThumbprint, Resources.TheCertificateIsNotValid, MessageBoxButtons.RetryCancel);
                        break;
                    case CertificateErrorCode.Unauthorised:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.ThisUserDoesNotHaveAccessToThisCertificate, Resources.TheCertificateIsNotValid, MessageBoxButtons.RetryCancel, ex.InnerException);
                        break;
                    case CertificateErrorCode.PrivateKey:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.ThisCertificateDoesNotHaveAPrivateKey, Resources.TheCertificateIsNotValid, MessageBoxButtons.RetryCancel);
                        break;
                    case CertificateErrorCode.PrivateKeyNotAccessible:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.YouDoNotHaveAccessToThePrivateKeyAssociatedWithThisCertificate, Resources.TheCertificateIsNotValid, MessageBoxButtons.AbortRetryIgnore);
                        break;
                    case CertificateErrorCode.PartialCertificateChain:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.CertificateVerificationChainFailed, Resources.TheCertificateIsNotValid, MessageBoxButtons.RetryCancel);
                        break;
                    case CertificateErrorCode.InvalidThumbprintRegex:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.TheThumbprintIsNotInTheCorrectFormat, Resources.TheCertificateIsNotValid, MessageBoxButtons.RetryCancel);
                        break;
                    case CertificateErrorCode.EncryptDecryptFail:
                        _dialogResult = BPMessageBox.ShowDialog(Resources.ThisCertificatePrivateKeyCannotDecrypt, Resources.TheCertificateIsNotValid, MessageBoxButtons.RetryCancel);
                        break;
                }
            }
            catch (Exception ex)
            {
                _dialogResult = BPMessageBox.ShowDialog(ex.Message, ex.GetType().ToString(), MessageBoxButtons.RetryCancel, ex);
            }
        }

        private void FrmManageServerConfigEncryption_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dialogResult == DialogResult.Ignore && CertificateErrorCode.HasFlag(CertificateErrorCode.PrivateKeyNotAccessible))
            {
                StatusUpdate?.Invoke($"{Resources.YouDoNotHaveAccessToThePrivateKeyAssociatedWithThisCertificate} Thumbprint: {Thumbprint}.", Enums.LoggingLevel.Warning);
            }
            DialogResult = _dialogResult;
        }

        private void ForceControlLoseFocus()
        {
            if (_forceLostFocusOnLoad)
            {
                _forceLostFocusOnLoad = false;
                rdoBPEncryption.ForceFocus = true;
                rdoOwnCertificate.ForceFocus = true;
            }
        }
    }
}
