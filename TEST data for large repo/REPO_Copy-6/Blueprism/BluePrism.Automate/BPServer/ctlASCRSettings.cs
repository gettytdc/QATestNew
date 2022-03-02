using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using BluePrism.AutomateAppCore;
using BluePrism.BPServer.FormValidationRules;
using BluePrism.BPServer.Properties;
using BluePrism.ClientServerResources.Core.Config;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.Common.Security;
using BluePrism.Common.Security.Exceptions;
using BluePrism.Images;

namespace BluePrism.BPServer
{
    public partial class ctlASCRSettings : UserControl
    {
        private ConnectionConfig _cfg;

        public ConnectionConfig Config
        {
            get => _cfg;
            set
            {
                _cfg = value ?? throw new ArgumentNullException(nameof(value));
                PopulateForm();
            }
        }

        public ctlASCRSettings()
        {
            InitializeComponent();

            pbGRPCWarningIcon.Image = ToolImages.Warning_16x16;
            serverCertStoreName.DataSource = Enum.GetValues(typeof(StoreName));
            clientCertStoreName.DataSource = Enum.GetValues(typeof(StoreName));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private IEnumerable<string> ValidateCertificate(string name, CertificateStoreCheckFlags checkFlags, string errorMessage, StoreName storeName)
        {
            if (!radCertificate.Checked)
                yield break;
            if (string.IsNullOrWhiteSpace(name))
            {
                yield return errorMessage;
                yield break;
            }
            string exmsg = null;
            var certStore = new CertificateStoreService();
            try
            {
                var cert = certStore.GetCertificateByName(
                    name,
                    storeName,
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine);
                certStore.ValidateCert(cert, checkFlags);
            }
            catch (CertificateException ex)
            {
                #region set text based on error code
                switch (ex.CertificateErrorCode)
                {
                    case CertificateErrorCode.NotFound:
                        exmsg = Resources.NoCertificateForName;
                        break;
                    case CertificateErrorCode.NotActiveYet:
                        exmsg = Resources.TheCertificateHasNotBeenActivated;
                        break;
                    case CertificateErrorCode.Expired:
                        exmsg = Resources.TheCertificateHasExpired;
                        break;
                    case CertificateErrorCode.NotUnique:
                        exmsg = Resources.MultipleCertificatesForName;
                        break;
                    case CertificateErrorCode.Unauthorised:
                        exmsg = Resources.ThisUserDoesNotHaveAccessToThisCertificate;
                        break;
                    case CertificateErrorCode.PrivateKey:
                        exmsg = Resources.ThisCertificateDoesNotHaveAPrivateKey;
                        break;
                    case CertificateErrorCode.PrivateKeyNotAccessible:
                        exmsg = Resources.YouDoNotHaveAccessToThePrivateKeyAssociatedWithThisCertificate;
                        break;
                    case CertificateErrorCode.PartialCertificateChain:
                        exmsg = Resources.CertificateVerificationChainFailed;
                        break;
                    case CertificateErrorCode.EncryptDecryptFail:
                        exmsg = Resources.ThisCertificatePrivateKeyCannotDecrypt;
                        break;
                }
                #endregion
            }
            if (exmsg != null)
                yield return $"'{name}' - {exmsg}";
        }

        public IEnumerable<string> Validate(IEnumerable<MachineConfig.ServerConfig> otherConfigs, int bindingPort)
        {
            if (otherConfigs is null)
                throw new ArgumentNullException(nameof(otherConfigs));
            
            return AscrSettingsFormValidator.Validate(txtHostName.Text, numPort.Value, bindingPort, radGRPC.Checked)
                .Concat(ValidateCertificate(txtCertName.Text, CertificateStoreCheckFlags.PrivateKey, Resources.InvalidCertText, (StoreName)serverCertStoreName.SelectedItem))
                .Concat(ValidateCertificate(txtClientCertName.Text, CertificateStoreCheckFlags.PrivateKey, Resources.InvalidClientCertText, (StoreName)clientCertStoreName.SelectedItem)) // gRPC may not need to check client cert private key
                .Select(x => $"{Resources.ctlASCRSettingsTabTitle}: {x}"); // pad all messages with the name of this tab
            // ?? validate if <= windows 7 then its not gRPC (too soon)
        }

        public void PopulateForm()
        {
            AddEventHandlers();
            switch (Config.CallbackProtocol)
            {
                case CallbackConnectionProtocol.Grpc:
                    radGRPC.Checked = true;
                    break;
                case CallbackConnectionProtocol.Wcf:
                    radWCF.Checked = true;
                    break;
                default:
                    radInsecure.Checked = true;
                    break;
            }

            txtHostName.Text = Config.HostName;
            numPort.Value = Config.Port;
            
            switch (Config.Mode)
            {
                case InstructionalConnectionModes.Certificate:
                    radCertificate.Checked = true;
                    break;
                case InstructionalConnectionModes.Windows:
                    radWindows.Checked = true;
                    break;
                case InstructionalConnectionModes.Insecure:
                    radInsecure.Checked = true;
                    break;
                default:
                    radInsecure.Checked = true;
                    break;
            }
            txtCertName.Text = Config.CertificateName;
            txtClientCertName.Text = Config.ClientCertificateName;
            serverCertStoreName.SelectedItem = Config.ServerStore;
            clientCertStoreName.SelectedItem = Config.ClientStore;
        }

        private void SecurityModeUpdated(object sender, EventArgs e)
        {
            if (radCertificate.Checked)
            {
                txtCertName.Enabled = true;
                txtClientCertName.Enabled = true;
                serverCertStoreName.Enabled = true;
                clientCertStoreName.Enabled = true;
            }
            else
            {
                txtCertName.Enabled = false;
                txtClientCertName.Enabled = false;
                serverCertStoreName.Enabled = false;
                clientCertStoreName.Enabled = false;
            }
        }

        private void ProtocolTypeUpdated(object sender, EventArgs e)
        {
            if (radGRPC.Checked)
            {
                radWindows.Enabled = false;
                if (radWindows.Checked)
                {
                    radInsecure.Checked = true;
                }
                pbGRPCWarningIcon.Visible = lblGRPCWarning.Visible = true;
            }
            else
            {
                radWindows.Enabled = true;
                pbGRPCWarningIcon.Visible = lblGRPCWarning.Visible = false;
            }
        }

        public ConnectionConfig GetCurrentValues()
        {
            var mode = InstructionalConnectionModes.None;
            if (radCertificate.Checked)
            {
                mode = InstructionalConnectionModes.Certificate;
            }
            else if (radWindows.Checked)
            {
                mode = InstructionalConnectionModes.Windows;
            }
            else if (radInsecure.Checked)
            {
                mode = InstructionalConnectionModes.Insecure;
            }

            return new ConnectionConfig
            {
                CallbackProtocol = radGRPC.Checked ? CallbackConnectionProtocol.Grpc : CallbackConnectionProtocol.Wcf,
                HostName = txtHostName.Text.Trim(),
                Port = (int)numPort.Value,
                Mode = mode,
                CertificateName = txtCertName.Text.Trim(),
                ClientCertificateName = txtClientCertName.Text.Trim(),
                ServerStore = (StoreName) serverCertStoreName.SelectedItem,
                ClientStore = (StoreName) clientCertStoreName.SelectedItem
            };
        }

        private void AddEventHandlers()
        {
            radGRPC.CheckedChanged += ProtocolTypeUpdated;
            radWCF.CheckedChanged += ProtocolTypeUpdated;
            radCertificate.CheckedChanged += SecurityModeUpdated;
            radWindows.CheckedChanged += SecurityModeUpdated;
            radInsecure.CheckedChanged += SecurityModeUpdated;
        }

    }
}
