using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AutomateControls.Forms;
using BluePrism.AutomateAppCore;
using BluePrism.BPServer.Enums;
using BluePrism.BPServer.Logging;
using BluePrism.BPServer.Properties;
using BluePrism.BPServer.Utility;
using BluePrism.Common.Security.Exceptions;
using BluePrism.Core.Utility;
using BPServer;
using NLog;

namespace BluePrism.BPServer
{
    public partial class frmMain : AutomateForm
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private Image _imagePlay, _imageStop;

        // The BP Server instance maintained by this form
        private clsBPServer mBPServer;

        // Whether the server should autostart or not
        private bool mAutoStart;

        // Whether the server should start with user context or not
        private bool mUserSpecifc;

        // List of key files loaded
        private List<string> keyFilesLoaded;

        private int? _wcfPerformanceMinutes = null;

        /// <summary>
        /// Gets the currently selected server option if one is selected.
        /// </summary>
        private MachineConfig.ServerConfig SelectedOption
        {
            get { return cmbConfigName.SelectedItem as MachineConfig.ServerConfig; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="start">True to automatically start the server, false
        /// otherwise.</param>
        public frmMain(bool start, bool userSpecfic, int? wcfPerformanceMinutes)
        {
            InitializeComponent();
            mAutoStart = start;
            mUserSpecifc = userSpecfic;
            MinimumSize = Size;
            _wcfPerformanceMinutes = wcfPerformanceMinutes;

            _imageStop = Image.FromFile("Resources/icon-stop.png");
            _imagePlay = Image.FromFile("Resources/icon-play.png");

            SetTitleWithVersion();
        }

        private void SetTitleWithVersion()
        {
            var title = Resources.BluePrismServer;
            var version = this.GetBluePrismVersion(fieldCount: 3);
            this.Text = $"{title} - v{version}";
        }

        /// <summary>
        /// Adds a message to the status output log on the form.
        /// </summary>
        /// <param name="msg">The message to be output.</param>
        /// <param name="level">The level at which the log message has been raised for.</param>
        private void AddStatusLine(string msg, LoggingLevel level)
        {
            // Check if we can safely call GUI-manipulation methods from this
            // thread - if not, invoke on the thread which owns the text box
            if (txtStatus.InvokeRequired)
            {
                // BeginInvoke() is asynchronous - it was hitting a deadlock if
                // a direct Invoke() was called while the GUI thread was trying
                // to shut down the scheduler.
                txtStatus.BeginInvoke(new Action<string, LoggingLevel>(AddStatusLine), msg, level);
            }
            else
            {
                if (level == LoggingLevel.Verbose && SelectedOption.Verbose
                    || level != LoggingLevel.Verbose)
                {
                    txtStatus.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] : {msg}\r\n");
                }

            }
        }

        /// <summary>
        /// Handles the initialisation of the server and the loading of any 
        /// database connections which are required in order to configure the server.
        /// Note that this does not perform any initialisation for the scheduler 
        /// other than the loading of the current configuration data. The setting
        /// up of the scheduler itself is done at the point of the scheduler starting
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The arguments providing a rich and essential tapestry of
        /// knowledge regarding the event... nah, just kidding, actually contains
        /// nothing at all.</param>
        private void frmMain_Load(object sender, EventArgs e)
        {
            AddStatusLine(Resources.LoadingConfiguration, LoggingLevel.Information);
            try
            {
                Options.Instance.Init(ConfigLocator.Instance(mUserSpecifc));
            }
            catch (CertificateException certificateException)
            {
                if (certificateException.CertificateErrorCode == CertificateErrorCode.NotFound)
                {
                    MessageBox.Show($"{Resources.ErrorCertificateCannotBeFound} Exception:{certificateException.Message}", nameof(CertificateException));
                    this.Close();
                    return;
                }

                AddStatusLine(string.Format(Resources.Failed0, certificateException.Message), LoggingLevel.Error);
                return;
            }
            catch (Exception ex)
            {
                AddStatusLine(string.Format(Resources.Failed0, ex.Message), LoggingLevel.Error);
                return;
            }
            AddStatusLine(Resources.SelectConfigurationAndClickStartToContinue, LoggingLevel.Information);

            mBPServer = new clsBPServer();
            mBPServer.Err += AddStatusLine;
            mBPServer.Warn += AddStatusLine;
            mBPServer.Info += AddStatusLine;
            mBPServer.Verbose += AddStatusLine;
            mBPServer.Analytics += AddStatusLine;

            Options.Instance.WcfPerformanceLogMinutes = _wcfPerformanceMinutes;

            UpdateConfigList(null);
            UpdateUI();

            //Record any key filenames loaded
            keyFilesLoaded = GetKeyFileNames();

            toolStripStatusLabel1.Text = Resources.ServerStopped;

            if (SelectedOption != null && mAutoStart)
                StartServer();
        }

        /// <summary>
        /// Deals with the form being closed without the server being stopped
        /// first - this ensures that the scheduler is stopped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ensure that the server is stopped before the window is closed...
            // otherwise there may be dangling resource connection managers which
            // will error when next woken up...
            if (mBPServer.Running)
                StopServer();

            _imagePlay.Dispose();
            _imageStop.Dispose();
        }
        
        /// <summary>
        /// Event handling the clicking of the 'Start' button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The utterly worthless event arguments which provide a sum
        /// total of nothing of value.</param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (mBPServer.Running)
                StopServer();
            else
                StartServer();

            UpdateUI();
        }

        /// <summary>
        /// Starts both the BP Server and the scheduler, if the scheduler is
        /// configured to activate.
        /// </summary>
        private void StartServer()
        {
            MachineConfig.ServerConfig cfg = SelectedOption;
            if (cfg == null)
                return;
            try
            {
                Cursor = Cursors.WaitCursor;
                ServerNLogConfig.SetAppProperties(false, cfg.Name);
                mBPServer.Start(cfg);
            }
            catch (Exception e)
            {
                AddStatusLine(string.Format(Resources.FailedToStartServer0, e.Message), LoggingLevel.Error);
                ServerNLogConfig.SetAppProperties(false);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Stops the server, including the scheduler if it is executing.
        /// </summary>
        private void StopServer()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                AddStatusLine(Resources.StoppingServer, LoggingLevel.Information);
                mBPServer.Stop();
                ServerNLogConfig.SetAppProperties(false);
            }
            catch (Exception e)
            {
                AddStatusLine(string.Format(Resources.FailedToStartServer0, e.Message), LoggingLevel.Error);
                ServerNLogConfig.SetAppProperties(false);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Update the list of configurations.
        /// </summary>
        /// <param name="sel">The configuration to select, or null to select the
        /// first available.</param>
        private void UpdateConfigList(MachineConfig.ServerConfig sel)
        {
            // Add all defined configurations to the dropdown...
            cmbConfigName.BeginUpdate();
            try
            {
                cmbConfigName.Items.Clear();
                foreach (MachineConfig.ServerConfig sopt in Options.Instance.Servers)
                {
                    cmbConfigName.Items.Add(sopt);
                    if (sopt == sel)
                        cmbConfigName.SelectedItem = sopt;
                }
                if (cmbConfigName.SelectedIndex == -1 && cmbConfigName.Items.Count > 0)
                    cmbConfigName.SelectedIndex = 0;
            }
            finally
            {
                cmbConfigName.EndUpdate();
            }
        }

        /// <summary>
        /// Update the user interface according to the current status.
        /// </summary>
        private void UpdateUI()
        {
            bool toggleAsRunning = mBPServer.Running;

            btnStart.Text = toggleAsRunning ? Resources.Stop : Resources.Start;
            btnStart.Image = toggleAsRunning ? _imageStop : _imagePlay;
            btnStart.BackColor = toggleAsRunning ? Color.FromArgb(11, 117, 183) : Color.White;
            btnStart.ForeColor = toggleAsRunning ? Color.White : Color.FromArgb(11, 117, 183);

            toolStripStatusLabel1.Text = toggleAsRunning
                ? Resources.ServerRunning
                : Resources.ServerStopped;

            btnStart.Enabled = SelectedOption != null;

            if (toggleAsRunning)
            {
                btnEdit.Enabled = false;
                btnNew.Enabled = false;
                btnDelete.Enabled = false;
                btnConfig.Enabled = false;
                cmbConfigName.Enabled = false;
            }
            else
            {
                btnEdit.Enabled = SelectedOption != null;
                btnDelete.Enabled = SelectedOption != null;
                btnNew.Enabled = true;
                btnConfig.Enabled = true;
                cmbConfigName.Enabled = cmbConfigName.Items.Count > 0;
            }
        }

        /// <summary>
        /// Saves any changes to in-memory configuration - called after configuration is 
        /// added, edited or deleted
        /// </summary>
        private void SaveConfig()
        {
            try
            {
                Options.Instance.Save();
                CheckRedundantKeyFiles();
                keyFilesLoaded = GetKeyFileNames();

                MessageBox.Show(this, Resources.ConfigurationChangesSaved, Resources.Success);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format(Resources.TheConfigurationCouldNotBeSaved0, ex.Message), Resources.Error);
            }
        }

        private void CheckRedundantKeyFiles()
        {
            //Remove any key files still referenced from list
            foreach (string file in GetKeyFileNames())
            {
                if (keyFilesLoaded.Contains(file)) keyFilesLoaded.Remove(file);
            }
            if (keyFilesLoaded.Count == 0) return;

            //Any remaining can be deleted
            List<string> notDeleted = new List<string>();
            foreach (string file in keyFilesLoaded)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    notDeleted.Add(string.Format(Resources._01, file, ex.Message));
                }
            }
            if (notDeleted.Count > 0) MessageBox.Show(this, Resources.UnableToDeleteTheFollowingFiles
                + String.Join("\n", notDeleted.ToArray()), Resources.DeleteError, MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// Returns a list of external encryption key filenames used in the config.
        /// </summary>
        /// <returns>List of filenames</returns>
        private List<string> GetKeyFileNames()
        {
            List<string> files = new List<string>();
            foreach (MachineConfig.ServerConfig sv in Options.Instance.Servers)
            {
                if (sv.KeyStore == MachineConfig.ServerConfig.KeyStoreType.ExternalFile)
                {
                    foreach (clsEncryptionScheme scheme in sv.EncryptionKeys.Values)
                    {
                        string file = Path.Combine(sv.ExternalKeyStoreFolder, scheme.ExternalFileName);
                        files.Add(file);
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// Handler for the timer tick - this is configured to fire every second,
        /// and just updates the status bar with the current number of connections.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">args. For the event.</param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mBPServer is null)
            {
                return;
            }

            int count = 0;
            if (mBPServer.Running)
                count = mBPServer.GetConnectedClients();
            toolStripStatusLabel3.Text = string.Format(Resources.Connections0, count);
        }

        /// <summary>
        /// Handle button pressed to delete currently selected configuration.
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            MachineConfig.ServerConfig opts = SelectedOption;
            if (opts == null)
                return;
            if (opts.IsDefault)
            {
                MessageBox.Show(this, string.Format(Resources.CannotDeleteConfiguration0, opts.Name), Resources.UnableToDelete, MessageBoxButtons.OK,
                    MessageBoxIcon.Hand);
                return;
            }

            if (BPMessageBox.ShowDialog(string.Format(Resources.ConfirmCustomBPServerDeletionMessage, opts.Name), Resources.ConfirmCustomBPServerDeletionTitle,
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Options.Instance.Servers.Remove(opts);
                UpdateConfigList(null);
                UpdateUI();
                SaveConfig();
            }
        }

        /// <summary>
        /// Handle button pressed to create a new configuration.
        /// </summary>
        private void btnNew_Click(object sender, EventArgs e)
        {
            MachineConfig.ServerConfig config = Options.Instance.NewServerConfig();
            using (frmServerConfig f = new frmServerConfig(config))
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    Options.Instance.Servers.Add(config);
                    UpdateConfigList(config);
                    UpdateUI();
                    SaveConfig();
                }
            }
        }

        /// <summary>
        /// Handle button pressed to edit a configuration.
        /// </summary>
        private void btnEdit_Click(object sender, EventArgs e)
        {
            MachineConfig.ServerConfig config = SelectedOption;
            if (config == null)
                return;

            using (frmServerConfig f = new frmServerConfig(config))
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    UpdateConfigList(config);
                    UpdateUI();
                    SaveConfig();
                }
            }
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            using (var form = new frmManageServerConfigEncryption(Options.Instance.SelectedConfigEncryptionMethod, Options.Instance.Thumbprint))
            {
                form.StatusUpdate += ManageEncryption_StatusUpdate;
                var dialogResult = form.ShowDialog();

                if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Ignore)
                {
                    Options.Instance.Thumbprint = form.Thumbprint;
                    Options.Instance.SelectedConfigEncryptionMethod = form.EncryptionMethod;
                    SaveConfig();
                }
            }
        }

        private void ManageEncryption_StatusUpdate(string message, LoggingLevel level)
        {
            if (level.ToNLogLevel() is LogLevel nLogLevel)
                Log.Log(nLogLevel, message);

            AddStatusLine(message, level);
        }

        private void cmbConfigName_DrawItem(object sender, DrawItemEventArgs e)
        {
            int index = e.Index >= 0 ? e.Index : 0;
            var brush = Brushes.White;
            e.DrawBackground();
            e.Graphics.DrawString(cmbConfigName.Items[index].ToString(), e.Font, brush, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Handle a change of selection in the configuration name combo box.
        /// </summary>
        private void cmbConfigName_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }
    }
}

