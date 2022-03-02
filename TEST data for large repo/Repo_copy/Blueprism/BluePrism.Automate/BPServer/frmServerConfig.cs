using AutomateControls.Forms;
using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib;
using BluePrism.BPServer.Properties;
using BluePrism.BPServer.SslCertificates;
using BluePrism.BPServer.UrlReservations;
using BluePrism.BPServer.WindowsServices;
using BluePrism.Core.HttpConfiguration;
using BluePrism.Core.Utility;
using BluePrism.Images;
using Microsoft.Win32;
using SslCertBinding.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using NLog;
using BluePrism.Utilities.Functional;
using BluePrism.Common.Security;

namespace BluePrism.BPServer
{
    public partial class frmServerConfig : AutomateForm
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Contains SSL certificate bindings for the currently selected port
        /// </summary>
        private List<SslCertificateBindingInfo> _currentSslBindings = new List<SslCertificateBindingInfo>();

        /// <summary>
        /// Contains list of BP Server windows services associated with the current connection name
        /// </summary>
        private List<WindowsServiceInfo> _currentWindowsServices = new List<WindowsServiceInfo>();

        /// <summary>
        /// Indicates whether the selected connection mode requires an SSL certificate
        /// </summary>
        private bool _sslCertificateRequired;

        /// <summary>
        /// Records whether we have displayed a warning in a MessageBox (this should only be shown
        /// once, not every time the SSL certificate binding list is refreshed)
        /// </summary>
        private bool _sslErrorMessageBoxDisplayed = false;

        private readonly SslCertificateBinder _certificateBinder = new SslCertificateBinder();

        /// <summary>
        /// Records whether we have displayed an windows service querying error in a MessageBox. 
        /// This error should only be shown once during the lifetime of the form, rather than 
        /// every time the service is queried.
        /// </summary>
        private bool _windowsServiceErrorMessageBoxDisplayed = false;

        /// <summary>
        /// Records the current status of server services associated with the 
        /// current configuration
        /// </summary>
        private WindowsServiceStatus _currentServiceStatus = WindowsServiceStatus.SingleService;

        /// <summary>
        /// Helper instance to help querying BP Server Windows Services
        /// </summary>
        private readonly WindowsServiceHelper _servicesHelper = new WindowsServiceHelper();

        /// <summary>
        /// Limits the number of windows service queries that are fired
        /// </summary>
        private readonly ActionLimiter _windowsServiceQueryLimiter;

        /// <summary>
        /// Limits the DisplaySSLBindings action
        /// </summary>
        private readonly ActionLimiter _displaySslBindingsLimiter;

        /// <summary>
        /// The connection modes that are based on WCF
        /// </summary>
        private static readonly ServerConnection.Mode[] WcfModes =  {
            ServerConnection.Mode.WCFSOAPTransport,
            ServerConnection.Mode.WCFInsecure,
            ServerConnection.Mode.WCFSOAPMessageWindows,
            ServerConnection.Mode.WCFSOAPTransportWindows
        };

        /// <summary>
        /// The connection modes that are based on Remoting
        /// </summary>
        private static readonly ServerConnection.Mode[] RemotingModes =  {
            ServerConnection.Mode.DotNetRemotingInsecure,
            ServerConnection.Mode.DotNetRemotingSecure
        };

        /// <summary>
        /// The connection modes that use SSL
        /// </summary>
        private static readonly ServerConnection.Mode[] SslModes = {
            ServerConnection.Mode.WCFSOAPTransport,
            ServerConnection.Mode.WCFSOAPTransportWindows
        };


        /// <summary>
        /// The server configuration options being edited (initialised once via constructor)
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MachineConfig.ServerConfig ServerConfig { get; private set; }

        public frmServerConfig(MachineConfig.ServerConfig serverConfig)
        {
            InitializeComponent();

            this.tcConfig.TabPages["tpASCR"].Text = Resources.ctlASCRSettingsTabTitle;

            this.ServerConfig = serverConfig;

            ctlASCRSettings.Config = serverConfig.CallbackConnectionConfig;

            //Check if the Reg Entry is here - SOFTWARE\Blue Prism Limited\Logstash
            if (!IsLogstashInstalled())
            {
                chkEnableDataGatewayProcess.ForeColor = System.Drawing.Color.DimGray;
                chkEnableDataGatewayProcess.AutoCheck = false;
                toolTip1.SetToolTip(chkEnableDataGatewayProcess, Resources.LogstashNotInstalled);

            }
            else
            {
                chkEnableDataGatewayProcess.AutoCheck = true;
                chkEnableDataGatewayProcess.ForeColor = new System.Drawing.Color();
                toolTip1.SetToolTip(chkEnableDataGatewayProcess, "");
            }
            txtDGPort.Enabled = chkEnableDataGatewayProcess.Enabled && chkEnableDataGatewayProcess.Checked;
            lblDGPort.Enabled = txtDGPort.Enabled;

            dgvSslBindings.AutoGenerateColumns = false;
            dgvWindowsServices.AutoGenerateColumns = false;
            SetUpWindowsServicesGridContextMenu();
            pbSslBindingWarningIcon.Image = ToolImages.Warning_16x16;
            pbGenericWindowsServiceWarning.Image = ToolImages.Warning_16x16;
            pbWindowsServiceWarning.Image = ToolImages.Warning_16x16;

            cmbConnectionMode.DataSource = Enum.GetValues(typeof(ServerConnection.Mode));

            _windowsServiceQueryLimiter = new ActionLimiter(TimeSpan.FromMilliseconds(250));
            _windowsServiceQueryLimiter.Execute += (s, e) => this.InvokeOnUiThread(DisplayWindowsServices);

            _displaySslBindingsLimiter = new ActionLimiter(TimeSpan.FromMilliseconds(250));
            _displaySslBindingsLimiter.Execute += (s, e) => this.InvokeOnUiThread(DisplaySslBindings);

            SetDataGatewayTabInitialFieldState();
        }

        private static bool IsLogstashInstalled()
        {
            using (var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                var key = view32.OpenSubKey(@"SOFTWARE\Blue Prism Limited\Logstash", false);
                return key != null;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
                if (_displaySslBindingsLimiter != null) _displaySslBindingsLimiter.Dispose();
                if (_windowsServiceQueryLimiter != null) _windowsServiceQueryLimiter.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateOptionsUI();
        }

        /// <summary>
        /// Updates the UI for the server options from the loaded options and
        /// the currently set server config in this form
        /// </summary>
        private void UpdateOptionsUI()
        {
            if (ServerConfig == null)
            {
                Debug.Fail("The server config should never be set to null (right?)");
                return;
            }

            int sel = -1;
            int tsel = 0;
            cmbConnection.Items.Clear();
            foreach (clsDBConnectionSetting con in AutomateAppCore.Options.Instance.Connections)
            {
                cmbConnection.Items.Add(con);
                if (con.ConnectionName == ServerConfig.Connection)
                    sel = tsel;
                tsel++;
            }
            if (sel != -1) cmbConnection.SelectedIndex = sel;
            txtName.Text = ServerConfig.Name;
            txtName.ReadOnly = ServerConfig.IsDefault;
            numPort.Value = ServerConfig.Port;

            cmbConnectionMode.SelectedItem = ServerConfig.ConnectionMode;
            chkDisableScheduler.Checked = ServerConfig.SchedulerDisabled;
            chkEnableDataGatewayProcess.Checked = ServerConfig.DataPipelineProcessEnabled && IsLogstashInstalled();

            if (chkEnableDataGatewayProcess.Checked)
            {
                radRunAsServerUser.Checked = !ServerConfig.DataGatewaySpecificUser;
                radRunAsSpecificUser.Checked = ServerConfig.DataGatewaySpecificUser;

                if (ServerConfig.DataGatewaySpecificUser)
                {
                    txtDGDomain.Text = ServerConfig.DataGatewayDomain;
                    txtDGPassword.SecurePassword = ServerConfig.DataGatewayPassword;
                    txtDGUsername.Text = ServerConfig.DataGatewayUser;
                }
                else
                {
                    txtDGDomain.Text = "";
                    txtDGPassword.Text = "";
                    txtDGUsername.Text = "";
                }
            }

            if (ServerConfig.DataPipelineProcessCommandListenerPort != 0)
            {
                txtDGPort.Text = ServerConfig.DataPipelineProcessCommandListenerPort.ToString();
            }
            chkServiceStatusEventLogging.Checked = ServerConfig.StatusEventLogging;
            chkVerbose.Checked = ServerConfig.Verbose;
            chkLogTraffic.Checked = ServerConfig.LogTraffic;
            chkLogDataGatewaysConsole.Checked = ServerConfig.DataGatewayLogToConsole;
            chkEnabledDataGatewaysTrace.Checked = ServerConfig.DataGatewayTraceLogging;

            if (ServerConfig.AuthenticationServerBrokerConfig != null)
            {
                txtBrokerAddress.Text = ServerConfig.AuthenticationServerBrokerConfig.BrokerAddress;
                txtBrokerUsername.Text = ServerConfig.AuthenticationServerBrokerConfig.BrokerUsername;
                txtBrokerPassword.SecurePassword = ServerConfig.AuthenticationServerBrokerConfig.BrokerPassword;
                txtEnvironmentIdentifier.Text = ServerConfig.AuthenticationServerBrokerConfig.EnvironmentIdentifier;
            }

            if (ServerConfig.BindTo == null)
                txtBindTo.Text = "";
            else
                txtBindTo.Text = ServerConfig.BindTo;

            UpdateUiForConnectionMode(ServerConfig.ConnectionMode);
            /// Only handle connection mode changing once the UI values have been
            /// intially set
            cmbConnectionMode.SelectedIndexChanged
                += new System.EventHandler(HandleConnectionModeChanged);

            dgvKeys.Rows.Clear();
            Boolean missingKey = false;
            foreach (KeyValuePair<string, clsEncryptionScheme> key in ServerConfig.EncryptionKeys)
            {
                clsEncryptionScheme scheme = (clsEncryptionScheme)key.Value;
                if (!scheme.HasValidKey) missingKey = true;

                int i = dgvKeys.Rows.Add(scheme.Name, scheme.AlgorithmName);
                dgvKeys.Rows[i].Tag = scheme;
            }
            if (ServerConfig.KeyStore == MachineConfig.ServerConfig.KeyStoreType.ExternalFile)
            {
                chkExternalFiles.Checked = true;
                txtKeyFolder.Text = ServerConfig.ExternalKeyStoreFolder;
            }
            //If any keys are not available disallow change of keystore location
            if (missingKey)
            {
                chkExternalFiles.Enabled = false;
                lblExternalFolder.Enabled = false;
                txtKeyFolder.Enabled = false;
                btnBrowse.Enabled = false;
            }

            chkOrdered.Checked = ServerConfig.Ordered ?? true;
        }

        /// <summary>
        /// Handles the cancel button being clicked on this server config form
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Handles the OK button being clicked on this server config form
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            if (cmbConnection.SelectedItem == null)
                ServerConfig.Connection = null;
            else
                ServerConfig.Connection = ((clsDBConnectionSetting)cmbConnection.SelectedItem).ConnectionName;
            ServerConfig.StatusEventLogging = chkServiceStatusEventLogging.Checked;
            ServerConfig.Name = txtName.Text;
            ServerConfig.ConnectionMode = (ServerConnection.Mode)cmbConnectionMode.SelectedItem;
            ServerConfig.SchedulerDisabled = chkDisableScheduler.Checked;
            ServerConfig.DataPipelineProcessEnabled = chkEnableDataGatewayProcess.Checked;
            ServerConfig.DataPipelineProcessCommandListenerPort = int.Parse(txtDGPort.Text);

            ServerConfig.DataGatewayDomain = "";
            ServerConfig.DataGatewayUser = "";
            ServerConfig.DataGatewayPassword = new SafeString();
            ServerConfig.DataGatewaySpecificUser = false;
            
            //If Enable Data Gateways is checked and Run as Blue Prism Server is not checked then store the credentials to use 
            if (chkEnableDataGatewayProcess.Checked && !radRunAsServerUser.Checked)
            {
                ServerConfig.DataGatewaySpecificUser = true;
                ServerConfig.DataGatewayDomain = txtDGDomain.Text;
                ServerConfig.DataGatewayUser = txtDGUsername.Text;
                ServerConfig.DataGatewayPassword = txtDGPassword.SecurePassword;
            }

            ServerConfig.Port = (int)numPort.Value;
            ServerConfig.BindTo = (txtBindTo.Text == "" ? null : txtBindTo.Text);
            ServerConfig.Verbose = chkVerbose.Checked;
            ServerConfig.LogTraffic = chkLogTraffic.Checked;
            ServerConfig.Ordered = chkOrdered.Checked;
            ServerConfig.DataGatewayTraceLogging = chkEnabledDataGatewaysTrace.Checked;
            ServerConfig.DataGatewayLogToConsole = chkLogDataGatewaysConsole.Checked;

            try
            {
                ServerConfig.AuthenticationServerBrokerConfig = new AuthenticationServerBrokerConfig(txtBrokerAddress.Text, txtBrokerUsername.Text, txtBrokerPassword.SecurePassword, txtEnvironmentIdentifier.Text);
            }
            catch
            {
                ServerConfig.AuthenticationServerBrokerConfig = null;
                Log.Warn("No Authentication Server Broker was set.");
            }

            ServerConfig.CallbackConnectionConfig = ctlASCRSettings.GetCurrentValues();

            ServerConfig.EncryptionKeys.Clear();
            foreach (DataGridViewRow row in dgvKeys.Rows)
            {
                clsEncryptionScheme scheme = (clsEncryptionScheme)row.Tag;
                ServerConfig.EncryptionKeys.Add(scheme.Name, scheme);
            }
            ServerConfig.KeyStore = MachineConfig.ServerConfig.KeyStoreType.Embedded;
            if (chkExternalFiles.Checked)
            {
                ServerConfig.KeyStore = MachineConfig.ServerConfig.KeyStoreType.ExternalFile;
                ServerConfig.ExternalKeyStoreFolder = txtKeyFolder.Text.Trim();
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Validates data in the form and displays a warning if it is invalid
        /// </summary>
        /// <returns>A boolean value indicating whether the form is valid</returns>
        private bool ValidateForm()
        {
            var otherConfigs = Options.Instance.Servers.Except(new[] { ServerConfig }).ToList();
            var errorMessages = new List<string>();
            // Ensure unique name
            string name = txtName.Text.Trim();
            if (otherConfigs.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                errorMessages.Add(string.Format(Resources.TheConfigurationName0IsAlreadyInUse, txtName.Text));
            }

            // Ensure valid key store folder
            if (chkExternalFiles.Checked)
            {
                string keyStoreFolder = txtKeyFolder.Text.Trim();
                if (keyStoreFolder == string.Empty)
                {
                    errorMessages.Add(Resources.PleaseSpecifyAnEncryptionKeyFolder);
                }
                else if (!System.IO.Directory.Exists(keyStoreFolder))
                {
                    errorMessages.Add(Resources.TheSpecifiedEncryptionKeyFolderDoesNotExist);
                }
                else
                {
                    var configUsingKeyStoreFolder = otherConfigs
                        .FirstOrDefault(x => x.KeyStore == MachineConfig.ServerConfig.KeyStoreType.ExternalFile
                            && x.ExternalKeyStoreFolder.Equals(keyStoreFolder, StringComparison.InvariantCultureIgnoreCase));
                    if (configUsingKeyStoreFolder != null)
                    {
                        string message = string.Format(
                            Resources.AnotherServerConfiguration0UsesTheFolder1ForStoringEncryptionKeysPleaseCorrectT,
                            configUsingKeyStoreFolder.Name, keyStoreFolder);
                        errorMessages.Add(message);
                    }
                }
            }

            // Connection mode
            var connectionMode = (ServerConnection.Mode)cmbConnectionMode.SelectedItem;
            if (RemotingModes.Contains(connectionMode))
            {
                if (!string.IsNullOrWhiteSpace(txtBindTo.Text) && !IPAddressHelper.IsValid(txtBindTo.Text.Trim()))
                {
                    errorMessages.Add(Resources.PleaseSpecifyAValidIPAddressInTheBindingSection);
                }
            }

            if (txtDGPort.Enabled && !int.TryParse(txtDGPort.Text, out int result))
            {
                errorMessages.Add(Resources.PleaseSpecifyAValidPort);
            }

            if (IsAuthenticationServerIntegrationTabPartiallyCompleted())
            {
                errorMessages.Add(Resources.BrokerSettingsValidationError);
            }


            errorMessages.AddRange(ctlASCRSettings.Validate(otherConfigs, (int)numPort.Value));

            if (errorMessages.Any())
            {
                string doubleLineBreak = Environment.NewLine + Environment.NewLine;
                string message = Resources.PleaseCorrectTheFollowingErrorsAndTryAgain
                                 + doubleLineBreak
                                 + string.Join(doubleLineBreak, errorMessages);
                MessageBox.Show(this, message, Resources.ValidationError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private bool IsAuthenticationServerIntegrationTabPartiallyCompleted()
        {
            var noFieldsPopulated = txtBrokerAddress.Text.Length == 0 && txtBrokerUsername.Text.Length == 0
                && txtBrokerPassword.SecurePassword.Length == 0 && txtEnvironmentIdentifier.Text.Length == 0;

            var allFieldsPopulated = txtBrokerAddress.Text.Length > 0 && txtBrokerUsername.Text.Length > 0
                && txtBrokerPassword.SecurePassword.Length > 0 && txtEnvironmentIdentifier.Text.Length > 0;

            return !noFieldsPopulated && !allFieldsPopulated;
        }

        private void chkExternalFiles_CheckedChanged(object sender, EventArgs e)
        {
            lblExternalFolder.Enabled = chkExternalFiles.Checked;
            txtKeyFolder.Enabled = chkExternalFiles.Checked;
            btnBrowse.Enabled = chkExternalFiles.Checked;
            if (!chkExternalFiles.Checked) txtKeyFolder.Text = string.Empty;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            if (System.IO.Directory.Exists(txtKeyFolder.Text))
                fbd.SelectedPath = txtKeyFolder.Text;

            if (fbd.ShowDialog() == DialogResult.OK)
                txtKeyFolder.Text = fbd.SelectedPath;
        }

        private void numPort_ValueChanged(object sender, EventArgs e)
        {
            if (_sslCertificateRequired)
            {
                _displaySslBindingsLimiter.Trigger();
            }
            _windowsServiceQueryLimiter.Trigger();

        }

        private void txtBindTo_TextChanged(object sender, EventArgs e)
        {
            if (_sslCertificateRequired)
            {
                _displaySslBindingsLimiter.Trigger();
            }
            _windowsServiceQueryLimiter.Trigger();
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            _windowsServiceQueryLimiter.Trigger();
        }

        /// <summary>
        /// Handles the 'enable event logging' checkbox being checked/unchecked.
        /// This ensures that the 'verbose event logging' checkbox is enabled or
        /// disabled appropriately
        /// </summary>
        private void chkServiceStatusEventLogging_CheckedChanged(object sender, EventArgs e)
        {
            chkVerbose.Enabled = chkServiceStatusEventLogging.Checked;
        }

        /// <summary>
        /// Handles the "Manage Permissions" link click to open the form
        /// </summary>
        private void llManagePermissions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var mode = (ServerConnection.Mode)cmbConnectionMode.SelectedItem;
            var properties = new BindingProperties(txtBindTo.Text.Trim(), (int)numPort.Value, SslModes.Contains(mode));
            using (var form = new frmManageUrlReservations(properties, _currentWindowsServices))
            {
                form.ShowDialog(this);
            }

            _windowsServiceQueryLimiter.Trigger();
        }

        /// <summary>
        /// Handles the Connection Mode changing updating help text box and
        /// available configuration options accordingly
        /// </summary>
        private void HandleConnectionModeChanged(object sender, EventArgs e)
        {
            if (ServerConfig != null) UpdateUiForConnectionMode((ServerConnection.Mode)cmbConnectionMode.SelectedItem);
        }

        private void UpdateUiForConnectionMode(ServerConnection.Mode mode)
        {
            txtConnectionModeHelp.Text = ServerConnection.GetHelpText(mode);
            UpdateBindingOptionsForConnectionMode(mode);
            UpdateWindowsServicesTabForConnectionMode(mode);
            // Requery windows services, because you need to re-check permissions 
            // as url reservation work differently for transport/non-transport 
            // modes
            DisplayWindowsServices();
        }

        #region Data Gateways Functions

        /// <summary>
        /// Updates controls within the binding section and SSL Certificate tab based 
        /// on connection type and whether SSL certificate setup is required
        /// </summary>
        /// <param name="mode"></param>
        private void UpdateBindingOptionsForConnectionMode(ServerConnection.Mode mode)
        {
            string wcfBindToLabelText = Resources.HostNameOrIPAddress;
            string remotingBindToLabelText = Resources.IPAddress;

            bool usingWcf = WcfModes.Contains(mode);
            lblBindToLabel.Text = usingWcf ? wcfBindToLabelText : remotingBindToLabelText;
            chkOrdered.Visible = false;
            lblOrdered.Visible = false;

            _sslCertificateRequired = SslModes.Contains(mode);
            if (_sslCertificateRequired)
            {
                ToggleSslCertificateControls(true);
                DisplaySslBindings();
            }
            else
            {
                ToggleSslCertificateControls(false);
                _currentSslBindings.Clear();
            }
        }

        private void chkEnableDataGatewayProcess_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDataGatewaysView();
        }

        private void UpdateDataGatewaysView()
        { 
            bool specificUserEnabledState = !radRunAsServerUser.Checked && chkEnableDataGatewayProcess.Checked;

            lblDGPassword.Enabled = specificUserEnabledState;
            txtDGPassword.Enabled = specificUserEnabledState;

            lblDGUsername.Enabled = specificUserEnabledState;
            txtDGUsername.Enabled = specificUserEnabledState;

            lblDGDomain.Enabled = specificUserEnabledState;
            txtDGDomain.Enabled = specificUserEnabledState;

            chkLogDataGatewaysConsole.Enabled = chkEnableDataGatewayProcess.Checked;
            chkEnabledDataGatewaysTrace.Enabled = chkEnableDataGatewayProcess.Checked && 
                                                  chkLogDataGatewaysConsole.Enabled && 
                                                  chkLogDataGatewaysConsole.Checked;


            grpDataGatewaysUser.Enabled = specificUserEnabledState;

            lblDGPort.Enabled = chkEnableDataGatewayProcess.Checked; 
            txtDGPort.Enabled = chkEnableDataGatewayProcess.Checked; 

            fraDGUser.Enabled = chkEnableDataGatewayProcess.Checked;

            radRunAsServerUser.Enabled = chkEnableDataGatewayProcess.Checked;
            radRunAsSpecificUser.Enabled = chkEnableDataGatewayProcess.Checked;
        }


        private void radRunAsServerUser_CheckedChanged(object sender, EventArgs e) => UpdateDataGatewaysView();

        private void radRunAsSpecificUser_CheckedChanged(object sender, EventArgs e) => UpdateDataGatewaysView();

        private void chkLogDataGatewaysConsole_CheckedChanged_1(object sender, EventArgs e) => UpdateDataGatewaysView();

        private void SetDataGatewayTabInitialFieldState()
        {
            chkEnableDataGatewayProcess.Checked = false;

            radRunAsServerUser.Checked = true;
            radRunAsSpecificUser.Checked = false;

            UpdateDataGatewaysView();
        }
        
#endregion

        #region SSL Certificate binding

        /// <summary>
        /// Shows or hides the controls used to manage SSL certificates
        /// </summary>
        private void ToggleSslCertificateControls(bool visible)
        {
            var sslControls = new Control[]
            {
                lblSslBindingsIntro,
                dgvSslBindings,
                llAddSslBinding,
                llViewSslBinding,
                llDeleteSslBinding
            };
            sslControls.ForEach(x => x.Visible = visible).Evaluate();
            if (visible)
            {
                if (!tcConfig.TabPages.Contains(tpSslCertificate))
                {
                    // https://stackoverflow.com/a/1532485 - handle must be created for
                    // Insert to work
                    var handle = tcConfig.Handle;
                    tpSslCertificate.Visible = true;
                    tcConfig.TabPages.Insert(1, tpSslCertificate);
                }
            }
            else
            {
                if (tcConfig.TabPages.Contains(tpSslCertificate))
                {
                    tcConfig.TabPages.Remove(tpSslCertificate);
                }
                DisplaySslWarning(null);
            }
        }

        /// <summary>
        /// Displays a list of existing SSL certificate bindings for the current 
        /// IP / host name and port
        /// </summary>
        private void DisplaySslBindings()
        {
            string address = txtBindTo.Text.Trim();
            int port = (int)numPort.Value;
            var status = RefreshSslBindings(address, port);
            string warningMessage = null;
            switch (status)
            {
                case SslBindingStatus.Error:
                    warningMessage = string.Format(Resources.ErrorLoadingCertificateBindingsOnThisMachineForPort0YouWillNeedToCheckThisManua, port);
                    break;
                case SslBindingStatus.None:
                    warningMessage = string.Format(Resources.NoCertificateHasBeenSetUpForPort0, port);
                    break;
                case SslBindingStatus.PossibleMatches:
                    warningMessage = string.Format(Resources.PleaseCheckThatACertificateSetUpForPort0AppliesToTheAddressUsedByTheServer, port);
                    break;
                case SslBindingStatus.Matches:
                    break;
            }
            DisplaySslWarning(warningMessage);

            lblSslBindingsIntro.Text = string.Format(Resources.CertificatesSetUpOnThisMachineForPort0, port);
            llDeleteSslBinding.Enabled = _currentSslBindings.Any();
            llViewSslBinding.Enabled = _currentSslBindings.Any();
            dgvSslBindings.DataSource = _currentSslBindings;
            dgvSslBindings.Refresh();
            dgvSslBindings.ClearSelection();
        }

        /// <summary>
        /// Updates the _currentSslBindings field with any SSL certificate bindings that apply to the port
        /// used by the server and checks whether any of them apply to the current server binding address.
        /// Returns a status to indicate the outcome of the check.
        /// </summary>
        /// <param name="address">The server binding address for this configuration</param>
        /// <param name="port">The port for this configuration</param>
        /// <returns>The result of attempting to load SSL certificate bindings for
        /// the specified binding address and port</returns>
        private SslBindingStatus RefreshSslBindings(string address, int port)
        {
            try
            {
                _currentSslBindings = _certificateBinder.GetSslBindingsForPort(port).ToList();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error listing SSL certificate bindings");
                DisplaySslErrorMessageBox();
                _currentSslBindings = new List<SslCertificateBindingInfo>();
                return SslBindingStatus.Error;
            }
            if (!_currentSslBindings.Any())
                return SslBindingStatus.None;

            if (string.IsNullOrEmpty(address))
            {
                address = "0.0.0.0";
            }
            string addressAndPort = string.Concat(address, ":", port);
            BindingEndPoint serverEndPoint;
            if (!BindingEndPoint.TryParse(addressAndPort, out serverEndPoint))
            {
                // Invalid IP address or host name supplied - they'll need to check manually
                return SslBindingStatus.PossibleMatches;
            }
            // Check whether existing bindings will match requests to the server endpoint
            // (we don't do anything clever like resolving IP addresses for host name as 
            // DNS setup might vary between server and client)
            bool exactMatch = _currentSslBindings.Any
                (binding => AppliesToServerAddress(binding, serverEndPoint));
            return exactMatch ? SslBindingStatus.Matches : SslBindingStatus.PossibleMatches;
        }

        /// <summary>
        /// Indicates whether an existing SSL binding applies to requests to the server address
        /// </summary>
        /// <remarks>Note that the port is not compared here - existing bindings will already
        /// have been filtered by the current configuration's port</remarks>
        /// <param name="binding">The SSL certificate binding to test</param>
        /// <param name="serverEndPoint">Binding end point representing the server address</param>
        /// <returns>True if the SSL binding applies to requests to the server address</returns>
        private bool AppliesToServerAddress(SslCertificateBindingInfo binding, BindingEndPoint serverEndPoint)
        {
            if (binding.EndPoint.EndPointType == BindingEndPointType.IpAddress
                && binding.EndPoint.IpAddress.Equals(IPAddress.Any))
            {
                return true;
            }
            return binding.EndPoint.AddressAndPort.Equals
                (serverEndPoint.AddressAndPort, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Displays warning about error loading SSL bindings in MessageBox if it 
        /// has not been displayed already during this form's lifetime
        /// </summary>
        private void DisplaySslErrorMessageBox()
        {
            if (!_sslErrorMessageBoxDisplayed)
            {
                string message = Resources.AnUnexpectedErrorOccurredWhileLoadingSSLCertificateInformationFullDetailsOfTheE;
                MessageBox.Show(this,
                    message,
                    Resources.ErrorLoadingSSLInformation, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _sslErrorMessageBoxDisplayed = true;
            }
        }

        /// <summary>
        /// Update or hide warning message about SSL certificate bindings shown next
        /// to the binding address and port
        /// </summary>
        /// <param name="message">The message to display. The controls will be 
        /// hidden if null.</param>
        private void DisplaySslWarning(string message)
        {
            lblSslCertificateWarning.Text = message ?? "";
            bool visible = message != null;
            lblSslCertificateWarning.Visible = visible;
            pbSslBindingWarningIcon.Visible = visible;
        }

        /// <summary>
        /// Adds SSL binding via separate dialog
        /// </summary>
        private void HandleAddSslBinding(object sender, LinkLabelLinkClickedEventArgs e)
        {
            bool sslSniSupported = _certificateBinder.SslSniBindingsSupported;
            var form = new frmAddSslBinding(txtBindTo.Text, (int)numPort.Value, sslSniSupported, _currentSslBindings);
            var result = form.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                DisplaySslBindings();
            }
        }

        /// <summary>
        /// Displays certificate used by SSL binding
        /// </summary>
        private void HandleViewSslBinding(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var binding = EnsureSelectedSslBinding(true);
            if (binding == null) return;

            if (binding.Certificate != null)
            {
                // Display using built-in Windows dialogue
                X509Certificate2UI.DisplayCertificate(binding.Certificate, Handle);
            }
            else
            {
                MessageBox.Show(this, Resources.NoCertificateFound, Resources.CertificateNotAvailableItMayHaveBeenUninstalledAfterTheSSLBindingWasCreated, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Deletes an SSL binding
        /// </summary>
        private void HandleDeleteSslBinding(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var binding = EnsureSelectedSslBinding(true);
            if (binding == null) return;

            var result = MessageBox.Show(this, string.Format(Resources.AreYouSureYouWantToRemoveTheSSLBindingOn0ClickYesToProceedOrNoToCancel, binding.EndPoint), Resources.DeleteBinding, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _certificateBinder.Delete(binding.EndPoint);
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error deleting SSL certificate binding");
                    DialogResult = DialogResult.Cancel;
                    string message = Resources.AnUnexpectedErrorOccurredWhileDeletingTheSSLBindingFullDetailsOfTheErrorHaveBee;
                    MessageBox.Show(this, message,
                        Resources.ErrorDeletingSSLBinding, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                DisplaySslBindings();
            }
        }

        /// <summary>
        /// Returns the SslCertificateBindingInfo for the selected row in the ssl 
        /// bindings listing.
        /// </summary>
        /// <param name="displayWarning">Indicates whether a warning should be 
        /// displayed if no item is selected</param>
        /// <returns>Returns a SslCertificateBindingInfo for the selected row in the ssl 
        /// bindings listing. Returns null if <paramref name="displayWarning"/> 
        /// is set to false, and no item is selected </returns>
        private SslCertificateBindingInfo EnsureSelectedSslBinding(bool displayWarning)
        {
            if (dgvSslBindings.SelectedRows.Count == 0)
            {
                if (displayWarning)
                {
                    MessageBox.Show(this, Resources.PleaseSelectABinding, Resources.NoBindingSelected, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return null;
            }
            var row = dgvSslBindings.SelectedRows[0];
            return row.DataBoundItem as SslCertificateBindingInfo;
        }

        #endregion

        #region Encryption keys

        /// <summary>
        /// Add new encryption key
        /// </summary>
        private void HandleAddKey(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmServerEncryptKey f = new frmServerEncryptKey();
            foreach (DataGridViewRow r in dgvKeys.Rows)
            {
                f.UsedNames.Add((string)r.Cells["colName"].Value);
            }
            if (f.ShowDialog() == DialogResult.OK)
            {
                int i = dgvKeys.Rows.Add(f.Encrypter.Name, f.Encrypter.AlgorithmName);
                dgvKeys.Rows[i].Tag = f.Encrypter;
                dgvKeys.Rows[i].Selected = true;
            }
            f.Dispose();
        }

        /// <summary>
        /// Update existing encryption key
        /// </summary>
        private void HandleEditKey(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (dgvKeys.SelectedRows.Count == 0) return;
            EditKey(dgvKeys.SelectedRows[0]);
        }

        private void EditKey(DataGridViewRow row)
        {
            clsEncryptionScheme scheme = (clsEncryptionScheme)row.Tag;
            if (!scheme.HasValidKey)
            {
                MessageBox.Show(this, Resources.TheCurrentUserAccountDoesNotHaveAccessToThisEncryptionKey,
                    Resources.AccessDenied, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            frmServerEncryptKey f = new frmServerEncryptKey();
            f.Encrypter = scheme;
            foreach (DataGridViewRow r in dgvKeys.Rows)
            {
                if (!r.Equals(row)) f.UsedNames.Add((string)r.Cells["colName"].Value);
            }
            if (f.ShowDialog() == DialogResult.OK)
            {
                row.Tag = f.Encrypter;
                row.Cells["colName"].Value = f.Encrypter.Name;
                row.Cells["colMethod"].Value = f.Encrypter.AlgorithmName;
            }
            f.Dispose();
        }

        /// <summary>
        /// Remove encryption key
        /// </summary>
        private void HandleDeleteKey(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (dgvKeys.SelectedRows.Count == 0) return;

            clsEncryptionScheme scheme = (clsEncryptionScheme)dgvKeys.SelectedRows[0].Tag;
            if (!scheme.HasValidKey)
            {
                MessageBox.Show(this, Resources.TheCurrentUserAccountDoesNotHaveAccessToThisEncryptionKey,
                    Resources.AccessDenied, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (MessageBox.Show(this, Resources.DeletingThisKeyWillMeanAnyDataEncryptedWithItWillNoLongerBeReadableByBluePrism,
                Resources.ConfirmDeletion, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                return;

            dgvKeys.Rows.Remove(dgvKeys.SelectedRows[0]);
        }

        private void dgvKeys_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            EditKey(dgvKeys.Rows[e.RowIndex]);
        }

        #endregion

        # region Windows Services

        /// <summary>
        /// Query windows services, to find windows services associated with the 
        /// current configuration, and return the status i.e. No Services,
        /// Single Service or Multiple services.
        /// </summary>
        /// <returns> 
        /// The service status of the current configuration
        /// </returns>
        private WindowsServiceStatus RefreshWindowsServices()
        {

            WindowsServiceStatus status = WindowsServiceStatus.None;

            try
            {
                var mode = (ServerConnection.Mode)cmbConnectionMode.SelectedItem;
                var properties = new BindingProperties(txtBindTo.Text.Trim(), (int)numPort.Value, SslModes.Contains(mode));

                _currentWindowsServices =
                    _servicesHelper.GetWindowsServicesForConfigurationName(txtName.Text.Trim(), properties).ToList();

                if (_currentWindowsServices.Any(s => !s.HasUrlPermission))
                    status |= WindowsServiceStatus.RequiresPermissions;

                // Check for conflicting URL Reservations.
                HttpConfigurationService configService = new HttpConfigurationService();
                var all = configService.GetUrlReservations();

                string url = properties.BindingReservationUrl;
                var conflicts = all
                    .Select(x => new
                    {
                        UrlReservation = x,
                        MatchType = UrlReservationMatcher.Compare(url, x.Url)
                    })
                    .Count(x => x.MatchType == UrlReservationMatchType.Conflict);

                if (conflicts > 0)
                {
                    status |= WindowsServiceStatus.ConflictingUrlReservations;
                }


                switch (_currentWindowsServices.Count)
                {
                    case 0: return status | WindowsServiceStatus.NoServices;
                    case 1: return status | WindowsServiceStatus.SingleService;
                    default: return status | WindowsServiceStatus.MultipleServices;
                }

            }
            catch (Exception exception)
            {
                DisplayWindowsServiceErrorMessageBox();
                Log.Error(exception, "Error listing Server Services");
                _currentWindowsServices = new List<WindowsServiceInfo>();
                return status | WindowsServiceStatus.Error;
            }

        }

        /// <summary>
        /// Displays a list of BP Server windows services associated with the current configuration name
        /// </summary>
        private void DisplayWindowsServices()
        {
            _currentServiceStatus = RefreshWindowsServices();

            string lblTxt = String.Format(Resources.WindowsServicesAssociatedWith0ServerConfiguration, txtName.Text);

            lblWindowsServices.Text = lblTxt;
            toolTip1.SetToolTip(lblWindowsServices, lblTxt);

            dgvWindowsServices.DataSource = _currentWindowsServices;
            dgvWindowsServices.Refresh();
            dgvWindowsServices.ClearSelection();

            llManagePermissions.Enabled = (_currentWindowsServices.Count > 0);
            btnCreateService.Visible = _currentServiceStatus.HasFlag(WindowsServiceStatus.NoServices);
            DisplayWindowsServiceWarning((ServerConnection.Mode)cmbConnectionMode.SelectedItem);

        }

        /// <summary>
        /// Updates the server services tab based on a connection mode
        /// </summary>
        /// <param name="mode">The connection mode of the server configuration</param>
        private void UpdateWindowsServicesTabForConnectionMode(ServerConnection.Mode mode)
        {
            var isWcfMode = WcfModes.Contains(mode);
            colPermission.Visible = isWcfMode;
            llManagePermissions.Visible = isWcfMode;
        }


        /// <summary>
        /// Displays any errors from querying Windows Services in a MessageBox. The error box
        /// is only displayed once during this form's lifetime so that the message doesn't keep
        /// appearing for every querying error.
        /// </summary>
        private void DisplayWindowsServiceErrorMessageBox()
        {
            if (!_windowsServiceErrorMessageBoxDisplayed)
            {
                string message = Resources.AnUnexpectedErrorOccurredWhilstQueryingWindowsServicesFullDetailsOfTheErrorHave;
                MessageBox.Show(this,
                    message,
                    Resources.ErrorCheckingServices, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _windowsServiceErrorMessageBoxDisplayed = true;
            }
        }


        /// <summary>
        /// Update or hide the warning messages about Server Services 
        /// </summary>
        /// <param name="mode">The connection mode of the server configuration</param>
        private void DisplayWindowsServiceWarning(ServerConnection.Mode mode)
        {
            var isWcfMode = WcfModes.Contains(mode);

            var genericWarning = "";

            var requiresPermissions = (isWcfMode && _currentServiceStatus.HasFlag(WindowsServiceStatus.RequiresPermissions));

            var warnings = new List<string>();


            if (_currentServiceStatus.HasFlag(WindowsServiceStatus.SingleService))
            {
                if (requiresPermissions)
                {
                    genericWarning = Resources.WindowsServicesRequireAttentionSeeTheServerServicesTabForMoreInformation;

                    warnings.Add(Resources.TheWindowsServiceIsRunningUnderAUserAccountThatDoesNotAppearToHavePermissionToL);
                }
            }

            if (_currentServiceStatus.HasFlag(WindowsServiceStatus.Error))
            {
                warnings.Add(Resources.ErrorWhenQueryingWindowsServices);
            }


            if (_currentServiceStatus.HasFlag(WindowsServiceStatus.MultipleServices))
            {
                genericWarning = Resources.WindowsServicesRequireAttentionSeeTheServerServicesTabForMoreInformation;

                if (requiresPermissions)
                {
                    warnings.Add(Resources.SomeWindowsServicesAreRunningUnderUserAccountsThatDoNotAppearToHavePermissionTo);
                }

                warnings.Add(Resources.MultipleWindowsServicesAreAssociatedWithThisServerConfigurationCheckThereAreNoD);

            }

            if (_currentServiceStatus.HasFlag(WindowsServiceStatus.NoServices))
            {
                genericWarning = Resources.ThereAreNoWindowsServicesForThisConfigurationSeeTheServerServicesTabForMoreInfo;

                warnings.Add(string.Format(Resources.ThereAreNoWindowsServicesAssociatedWithTheCurrentServerConfigurationToCreateAWi, _servicesHelper.GetCreateWindowsServiceCommandText(txtName.Text)));


            }

            if (_currentServiceStatus.HasFlag(WindowsServiceStatus.ConflictingUrlReservations))
            {
                genericWarning = Resources.WindowsServicesRequireAttentionSeeTheServerServicesTabForMoreInformation;
                warnings.Add(
                    Resources.ConflictingURLReservationsConfiguredThisMayCauseConnectionsToTheServerToFailRes);
            }

            var showGenericWarning = (genericWarning != "");
            lblGenericWindowsServiceWarning.Visible = showGenericWarning;
            pbGenericWindowsServiceWarning.Visible = showGenericWarning;
            lblGenericWindowsServiceWarning.Text = genericWarning;

            var detailedWarning = BuildDetailedWindowsServiceWarningMessage(warnings);
            var showDetailedWarning = (detailedWarning != "");
            txtWindowsServiceWarning.Visible = showDetailedWarning;
            pbWindowsServiceWarning.Visible = showDetailedWarning;
            txtWindowsServiceWarning.Text = detailedWarning;


        }

        /// <summary>
        /// Build a windows service warning message from a list of warnings.
        /// </summary>
        /// <param name="warnings">A list of warnings, each describing a reason why
        /// windows services may not be correctly set up for this server configuration</param>
        /// <returns>A single readable message describing an windows service warnings</returns>
        private string BuildDetailedWindowsServiceWarningMessage(List<string> warnings)
        {
            if (warnings.Count == 0) return "";
            if (warnings.Count == 1) return warnings[0];

            var stb = new StringBuilder();
            stb.AppendLine(Resources.TheFollowingIssuesRequireAttention);

            for (int i = 0; i < warnings.Count; i++)
            {
                stb.AppendLine();
                stb.AppendLine(String.Format("{0}) {1}", i + 1, warnings[i]));
            }

            return stb.ToString();

        }


        /// <summary>
        /// Handles the cell formatting event of the Windows Services grid, and sets the 
        /// has permissions cell value to be a tick or a warning depending on the 
        /// underlying value.
        /// </summary>
        private void HandleWindowsServicesCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvWindowsServices.Columns[e.ColumnIndex].Name.Equals(colPermission.Name))
            {
                var row = dgvWindowsServices.Rows[e.RowIndex];
                var service = (WindowsServiceInfo)row.DataBoundItem;

                DataGridViewCell cell = row.Cells[e.ColumnIndex];

                if ((bool)e.Value)
                {
                    e.Value = ToolImages.Tick_16x16;
                    cell.ToolTipText = "";
                }
                else
                {
                    e.Value = ToolImages.Warning_16x16;
                    cell.ToolTipText =
                        String.Format(Resources._0DoesNotAppearToHavePermissionToListenOnThisConfigurationSURL,
                        service.StartName);
                }

            }

        }

        /// <summary>
        /// Handles the windows services grid right click menu refresh click
        /// </summary>
        private void HandleRefreshWindowsServices(object sender, EventArgs e)
        {
            DisplayWindowsServices();
        }

        /// <summary>
        /// Handle the selection changed event of the windows services grid, and 
        /// remove the selection. This is to prevent rows being selected and 
        /// looking as if some action can be performed on them.
        /// </summary>
        private void HandleWindowsServicesSelectionChanged(object sender, EventArgs e)
        {
            dgvWindowsServices.ClearSelection();
        }

        /// <summary>
        /// Set up the right click context menu on the windows services grid, 
        /// which can be used to refresh the grid.
        /// </summary>
        private void SetUpWindowsServicesGridContextMenu()
        {
            ContextMenuStrip servicesMenu = new ContextMenuStrip();
            ToolStripMenuItem refreshServices = new ToolStripMenuItem(Resources.Refresh);
            refreshServices.Image = BluePrism.Images.ToolImages.Refresh_16x16;
            //Assign event handlers
            refreshServices.Click += new EventHandler(HandleRefreshWindowsServices);

            //Add to main context menu
            servicesMenu.Items.AddRange(new ToolStripItem[] { refreshServices });
            //Assign to datagridview
            dgvWindowsServices.ContextMenuStrip = servicesMenu;
        }

        private void HandlesCreateServiceClick(object sender, EventArgs e)
        {
            try
            {
                modWin32.CreateService(_servicesHelper.GetServicePathForCurrentDirectory(txtName.Text), _servicesHelper.GetServiceName(txtName.Text));
                DisplayWindowsServices();
            }
            catch (Win32Exception ex)
            {
                Log.Error(ex, "Error Creating Service");
                MessageBox.Show(this,
                    ex.Message,
                    Resources.ErrorCreatingService, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        #endregion

        #region Inner types

        /// <summary>
        /// The result of an attempt to load SSL certificate bindings for the currently selected 
        /// server binding address (ip address or hostname) and port and determine whether any of
        /// them apply
        /// </summary>
        private enum SslBindingStatus
        {
            /// <summary>
            /// An error occurred while retrieving the bindings, which could indicate a problem with
            /// the OS or network setup
            /// </summary>
            Error,
            /// <summary>
            /// No SSL certificate bindings were found that could match the server binding address
            /// and port
            /// </summary>
            None,
            /// <summary>
            /// SSL certificate bindings exist for the port, but it cannot be determined
            /// whether they apply to the server's binding address. For example IP address bindings 
            /// may exist when a host name is used for the server configuration.
            /// </summary>
            PossibleMatches,
            /// <summary>
            /// SSL certificate bindings exist for the port that either use the &quot;Any&quot; IP address (0.0.0.0) or
            /// exactly match the server binding address.
            /// </summary>
            Matches
        }


        /// <summary>
        /// The result of querying for windows services associated with the
        /// current configuration name.
        /// </summary>
        [Flags]
        private enum WindowsServiceStatus
        {
            /// <summary>
            /// No status found
            /// </summary>
            None = 0,
            /// <summary>
            /// A single service was found
            /// </summary>
            SingleService = 1,
            /// <summary>
            /// No services were found
            /// </summary>
            NoServices = 2,
            /// <summary>
            //  Multiple services were found
            /// </summary>
            MultipleServices = 4,
            /// <summary>
            /// The services require permissions to be configured
            /// </summary>
            RequiresPermissions = 8,
            /// <summary>
            /// An error occurred trying to query Windows Services
            /// </summary>
            Error = 16,
            /// <summary>
            /// There are conflicting URL permissions specified
            /// </summary>
            ConflictingUrlReservations = 32


        }





        #endregion
        

    }
}
