using BluePrism.BPServer.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using AutomateControls.Forms;
using BluePrism.BPServer.SslCertificates;
using BluePrism.Core.Utility;
using NLog;
using SslCertBinding.Net;

namespace BluePrism.BPServer
{
    /// <summary>
    /// Form displayed as dialog to create a new SSL binding
    /// </summary>
    public partial class frmAddSslBinding : AutomateForm
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Address (host name or IP address) used as the server binding address
        /// </summary>
        private readonly string _address;
        /// <summary>
        /// The port used by the server
        /// </summary>
        private readonly int _port;
        /// <summary>
        /// Indicates whether SSL SNI bindings (using host name) are supported on this machine
        /// </summary>
        private readonly bool _sslSniSupported;
        /// <summary>
        /// Existing SSL bindings for the port
        /// </summary>
        private readonly List<SslCertificateBindingInfo> _currentBindings;

        /// <summary>
        /// Available SSL certificate store name options
        /// </summary>
        private static readonly StoreNameOption[] StoreOptions =
        {
            new StoreNameOption(StoreName.My, Resources.MyPersonal), 
            new StoreNameOption(StoreName.AuthRoot, Resources.AuthRoot), 
            new StoreNameOption(StoreName.CertificateAuthority, Resources.CertificateAuthority), 
        };

        private readonly SslCertificateBinder _sslCertificateBinder = new SslCertificateBinder();

        public frmAddSslBinding(string address, int port, bool sslSniSupported, List<SslCertificateBindingInfo> currentBindings)
        {
            InitializeComponent();
            pbSslSniNotEnabledWarningIcon.Image = BluePrism.Images.ToolImages.Warning_16x16;

            _address = address.Trim();
            _port = port;
            _sslSniSupported = sslSniSupported;
            _currentBindings = currentBindings;

            cbStoreName.DataSource = StoreOptions;
        }

        protected override void OnLoad(EventArgs e)
        {
            UpdateBindingAddressOptions();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            AddBinding();
        }

        /// <summary>
        /// Sets up controls within the binding address section according to the
        /// IP address or hostname supplied
        /// </summary>
        private void UpdateBindingAddressOptions()
        {
            string portNumber = _port.ToString(CultureInfo.InvariantCulture);
            rbBindToAnyIp.Text += portNumber;
            lblIpAddressPortInfo.Text += portNumber;
            lblHostNamePortInfo.Text += portNumber;

            BindingOptionState anyIpOptionState = BindingOptionState.Hidden;
            BindingOptionState specificIpOptionState = BindingOptionState.Hidden;
            BindingOptionState hostNameOptionState = BindingOptionState.Hidden;
            bool displaySslSniNotEnabledWarning = false;

            if (string.IsNullOrEmpty(_address))
            {
                // If no address specified, then only allow binding to any IP address
                anyIpOptionState = BindingOptionState.Enabled;
                rbBindToAnyIp.Checked = true;
            }
            else if (IPAddressHelper.IsValid(_address))
            {
                // If address is an IP address, then allow binding to specific or any IP address
                txtIpAddress.Text = _address;
                anyIpOptionState = BindingOptionState.Enabled;
                specificIpOptionState = BindingOptionState.Enabled;
                rbBindToSpecificIp.Checked = true;
            }
            else
            {
                // If address is a hostname, then enable all options
                txtIpAddress.ReadOnly = false;
                txtHostName.Text = _address;
                anyIpOptionState = BindingOptionState.Enabled;
                specificIpOptionState = BindingOptionState.Enabled;
                hostNameOptionState = _sslSniSupported ? BindingOptionState.Enabled : BindingOptionState.Disabled;
                if (_sslSniSupported)
                {
                    rbBindToHostName.Checked = true;
                }
                else
                {
                    rbBindToSpecificIp.Checked = true;
                    displaySslSniNotEnabledWarning = true;
                }
            }

            UpdateBindingAddressControls(anyIpOptionState, rbBindToAnyIp);
            UpdateBindingAddressControls(specificIpOptionState, rbBindToSpecificIp, txtIpAddress, lblIpAddressPortInfo);
            UpdateBindingAddressControls(hostNameOptionState, rbBindToHostName, txtHostName, lblHostNamePortInfo);

            pbSslSniNotEnabledWarningIcon.Visible = displaySslSniNotEnabledWarning;
            lblSslSniNotEnabledWarning.Visible = displaySslSniNotEnabledWarning;
        }

        /// <summary>
        /// Sets the Visible and Enabled properties of one or more controls according to
        /// specified state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="controls"></param>
        private void UpdateBindingAddressControls(BindingOptionState state, params Control[] controls)
        {
            foreach (var control in controls)
            {
                control.Visible = state != BindingOptionState.Hidden;
                control.Enabled = state == BindingOptionState.Enabled;
            }
        }

        /// <summary>
        /// Adds a new SSL certificate binding based on the selected address and port
        /// </summary>
        private void AddBinding()
        {
            BindingEndPoint endPoint;
            if (rbBindToAnyIp.Checked)
            {
                endPoint = new BindingEndPoint(IPAddress.Any, _port);
            }
            else if (rbBindToSpecificIp.Checked)
            {
                IPAddress ipAddress;
                if (!IPAddress.TryParse(txtIpAddress.Text, out ipAddress))
                {
                    MessageBox.Show(this, Resources.PleaseEnterAValidIPAddress, Resources.InvalidAddress, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                endPoint = new BindingEndPoint(ipAddress, _port);
            }
            else
            {
                endPoint = new BindingEndPoint(_address, _port);
            }

            bool bindingExists = _currentBindings.Any(x => x.EndPoint.AddressAndPort.Equals(endPoint.AddressAndPort, StringComparison.InvariantCultureIgnoreCase));
            if (bindingExists)
            {
                string message = string.Format(Resources.AddingANewCertificateWillReplaceTheExistingBindingOn0AreYouSureYouWishToContinu, endPoint.AddressAndPort);
                var result = MessageBox.Show(this, message, Resources.ReplaceBinding, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // Select a certificate
            var selecteItem = (StoreNameOption)cbStoreName.SelectedItem;
            var storeName = selecteItem.Name;
            var store = new X509Store(storeName, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
            if (store.Certificates.Count == 0)
            {
                MessageBox.Show(this, string.Format(Resources.NoCertificatesFoundInStore0, selecteItem.Title), Resources.NoCertificates, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selected = X509Certificate2UI.SelectFromCollection(store.Certificates, Resources.SelectCertificate,
                Resources.SelectACertificateFromTheFollowingListToAdd, X509SelectionFlag.SingleSelection);
            if (selected.Count == 1)
            {
                var certificate = selected[0];

                var appId = Guid.NewGuid();
                try
                {
                    _sslCertificateBinder.Add(endPoint, appId, certificate, storeName);
                    DialogResult = DialogResult.OK;
                }
                catch(Exception exception)
                {
                    Log.Error(exception, "Error adding SSL certificate binding");

                    DialogResult = DialogResult.Cancel;
                    string message = Resources.AnUnexpectedErrorOccuredWhileAddingTheSSLCertificateBindingFullDetailsOfTheErro;
                    MessageBox.Show(this,
                        message,
                        Resources.ErrorAddingSSLBinding, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Close();
            }
        }


        #region Inner types

        /// <summary>
        /// Defines 3 states available for binding options
        /// </summary>
        private enum BindingOptionState
        {
            /// <summary>
            /// Not shown
            /// </summary>
            Hidden,
            /// <summary>
            /// Visible but not available to select
            /// </summary>
            Disabled,
            /// <summary>
            /// Available to select
            /// </summary>
            Enabled
        }

        /// <summary>
        /// Option displayed in certificate store combobox
        /// </summary>
        private class StoreNameOption
        {
            public StoreNameOption(StoreName name, string title)
            {
                Name = name;
                Title = title;
            }

            /// <summary>
            /// The StoreName value for the option
            /// </summary>
            public StoreName Name { get; private set; }
            
            /// <summary>
            /// A user-friendly title displayed in the list
            /// </summary>
            public string Title { get; private set; }

            public override string ToString()
            {
                return Title;
            }
        }

        #endregion

    }
}
