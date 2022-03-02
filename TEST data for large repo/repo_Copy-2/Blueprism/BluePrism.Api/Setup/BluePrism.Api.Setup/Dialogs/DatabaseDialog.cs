namespace BluePrism.Api.Setup.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using global::WixSharp.UI.Forms;
    using Microsoft.Win32;
    using RegistryHive = Microsoft.Win32.RegistryHive;

    public partial class DatabaseDialog : ManagedForm
    {
        public DatabaseDialog()
        {
            //NOTE: If this assembly is compiled for v4.0.30319 runtime, it may not be compatible with the MSI hosted CLR.
            //The incompatibility is particularly possible for the Embedded UI scenarios.
            //The safest way to avoid the problem is to compile the assembly for v3.5 Target Framework.WixSharp Setup
            InitializeComponent();
        }

        private void dialog_Load(object sender, EventArgs e)
        {
            _serverCombo.Text = Runtime.Session[MsiProperties.ApiSqlServer];
            if (Runtime.Session[MsiProperties.ApiSqlAuthenticationMode] == SqlAuthenticationMode.Trusted)
            {
                _trustedAuthenticationRadio.Checked = true;
            }
            else
            {
                _sqlAuthenticationRadio.Checked = true;
            }

            _username.Text = Runtime.Session[MsiProperties.ApiSqlUsername];
            _password.Text = Runtime.Session[MsiProperties.ApiSqlPassword];
            _databaseName.Text = Runtime.Session[MsiProperties.ApiSqlDatabaseName];

            PopulateServerList();
            if (string.IsNullOrWhiteSpace(_serverCombo.Text))
                _serverCombo.SelectedIndex = _serverCombo.Items.Count > 0 ? 0 : -1;

            UpdateNextButtonEnabled();
            UpdateCredentialsEnabled();

            banner.Image = Runtime.Session.GetResourceBitmap("banner");

            ResetLayout();
        }

        private void ResetLayout()
        {
            float ratio = (float)banner.Image.Width / (float)banner.Image.Height;
            panel2.Height = (int)(banner.Width / ratio);

            var upShift = (int)(next.Height * 2.3) - panel1.Height;
            panel1.Top -= upShift;
            panel1.Height += upShift;
        }

        private void next_Click(object sender, EventArgs e)
        {
            Runtime.Session[MsiProperties.ApiSqlServer] = _serverCombo.Text;
            Runtime.Session[MsiProperties.ApiSqlAuthenticationMode] =
                _trustedAuthenticationRadio.Checked ? SqlAuthenticationMode.Trusted : SqlAuthenticationMode.Sql;
            Runtime.Session[MsiProperties.ApiSqlUsername] = _username.Text;
            Runtime.Session[MsiProperties.ApiSqlPassword] = _password.Text;
            Runtime.Session[MsiProperties.ApiSqlDatabaseName] = _databaseName.Text;

            Shell.GoNext();
        }

        private void PopulateServerList()
        {
            var selectedServer = _serverCombo.Text;
            _serverCombo.Items.Clear();
            _serverCombo.Items.AddRange(GetLocalDatabaseServers().Cast<object>().ToArray());
            _serverCombo.Text =
                string.IsNullOrEmpty(selectedServer) && _serverCombo.Items.Count > 0
                ? (string)_serverCombo.Items[0]
                : selectedServer;
        }

        private static IEnumerable<string> GetLocalDatabaseServers()
        {
            foreach (var registryView in new[] { RegistryView.Registry32, RegistryView.Registry64 })
            {
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
                using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server"))
                {
                    var instances = (string[])key?.GetValue("InstalledInstances");
                    if (instances != null)
                    {
                        foreach (var element in instances)
                        {
                            yield return element == "MSSQLSERVER"
                                ? Environment.MachineName
                                : Environment.MachineName + @"\" + element;
                        }
                    }
                }
            }
        }

        private void UpdateNextButtonEnabled()
        {
            bool IsSqlAuthenticationConfigurationValid() =>
                _trustedAuthenticationRadio.Checked
                || _sqlAuthenticationRadio.Checked
                    && !string.IsNullOrWhiteSpace(_username.Text)
                    && !string.IsNullOrEmpty(_password.Text);

            next.Enabled =
                !string.IsNullOrWhiteSpace(_serverCombo.Text)
                && IsSqlAuthenticationConfigurationValid()
                && !string.IsNullOrEmpty(_databaseName.Text);
        }

        private void UpdateCredentialsEnabled()
        {
            _username.Enabled = _sqlAuthenticationRadio.Checked;
            _password.Enabled = _sqlAuthenticationRadio.Checked;
        }

        private void back_Click(object sender, EventArgs e)
        {
            Shell.GoPrev();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Shell.Cancel();
        }

        private void _refreshButton_Click(object sender, EventArgs e)
        {
            PopulateServerList();
        }

        private void _trustedAuthenticationRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateNextButtonEnabled();
            UpdateCredentialsEnabled();

            if (_trustedAuthenticationRadio.Checked)
            {
                _username.Text = string.Empty;
                _password.Text = string.Empty;
            }
        }

        private void _username_TextChanged(object sender, EventArgs e)
        {
            UpdateNextButtonEnabled();
        }

        private void _password_TextChanged(object sender, EventArgs e)
        {
            UpdateNextButtonEnabled();
        }

        private void _databaseName_TextChanged(object sender, EventArgs e)
        {
            UpdateNextButtonEnabled();
        }

        private void _serverCombo_TextChanged(object sender, EventArgs e)
        {
            UpdateNextButtonEnabled();
        }
    }
}
