namespace BluePrism.BPServer
{
    partial class frmServerConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components=null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmServerConfig));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
			this.btnOK = new AutomateControls.Buttons.StandardStyledButton(this.components);
			this.btnCancel = new AutomateControls.Buttons.StandardStyledButton(this.components);
			this.chkDisableScheduler = new System.Windows.Forms.CheckBox();
			this.tcConfig = new System.Windows.Forms.TabControl();
			this.tpDetails = new System.Windows.Forms.TabPage();
			this.pbGenericWindowsServiceWarning = new System.Windows.Forms.PictureBox();
			this.lblGenericWindowsServiceWarning = new System.Windows.Forms.Label();
			this.gbBinding = new System.Windows.Forms.GroupBox();
			this.chkOrdered = new System.Windows.Forms.CheckBox();
			this.lblOrdered = new System.Windows.Forms.Label();
			this.pbSslBindingWarningIcon = new System.Windows.Forms.PictureBox();
			this.lblSslCertificateWarning = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.txtBindTo = new AutomateControls.Textboxes.StyledTextBox();
			this.lblBindToLabel = new System.Windows.Forms.Label();
			this.numPort = new AutomateControls.StyledNumericUpDown();
			this.txtConnectionModeHelp = new AutomateControls.Textboxes.StyledTextBox();
			this.lblConnectionMode = new System.Windows.Forms.Label();
			this.cmbConnectionMode = new System.Windows.Forms.ComboBox();
			this.cmbConnection = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtName = new AutomateControls.Textboxes.StyledTextBox();
			this.tpSslCertificate = new System.Windows.Forms.TabPage();
			this.llDeleteSslBinding = new AutomateControls.BulletedLinkLabel();
			this.llViewSslBinding = new AutomateControls.BulletedLinkLabel();
			this.dgvSslBindings = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CertificateDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.llAddSslBinding = new AutomateControls.BulletedLinkLabel();
			this.lblSslBindingsIntro = new System.Windows.Forms.Label();
			this.tpKeys = new System.Windows.Forms.TabPage();
			this.lblExternalFolder = new System.Windows.Forms.Label();
			this.btnBrowse = new AutomateControls.Buttons.StandardStyledButton(this.components);
			this.txtKeyFolder = new AutomateControls.Textboxes.StyledTextBox();
			this.chkExternalFiles = new System.Windows.Forms.CheckBox();
			this.llDeleteKey = new AutomateControls.BulletedLinkLabel();
			this.llEditKey = new AutomateControls.BulletedLinkLabel();
			this.llAddKey = new AutomateControls.BulletedLinkLabel();
			this.dgvKeys = new System.Windows.Forms.DataGridView();
			this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colMethod = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tpServerServices = new System.Windows.Forms.TabPage();
			this.btnCreateService = new AutomateControls.Buttons.StandardStyledButton(this.components);
			this.llManagePermissions = new AutomateControls.BulletedLinkLabel();
			this.txtWindowsServiceWarning = new AutomateControls.Textboxes.StyledTextBox();
			this.pbWindowsServiceWarning = new System.Windows.Forms.PictureBox();
			this.dgvWindowsServices = new System.Windows.Forms.DataGridView();
			this.colServiceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colFullPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colStartName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colStartMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colState = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colPermission = new System.Windows.Forms.DataGridViewImageColumn();
			this.lblWindowsServices = new System.Windows.Forms.Label();
			this.tpLogging = new System.Windows.Forms.TabPage();
			this.chkLogTraffic = new System.Windows.Forms.CheckBox();
			this.chkVerbose = new System.Windows.Forms.CheckBox();
			this.chkServiceStatusEventLogging = new System.Windows.Forms.CheckBox();
			this.tpDataGateways = new System.Windows.Forms.TabPage();
			this.chkEnabledDataGatewaysTrace = new System.Windows.Forms.CheckBox();
			this.chkLogDataGatewaysConsole = new System.Windows.Forms.CheckBox();
			this.lblDGPort = new System.Windows.Forms.Label();
			this.txtDGPort = new AutomateControls.Textboxes.StyledTextBox();
			this.fraDGUser = new System.Windows.Forms.GroupBox();
			this.radRunAsSpecificUser = new AutomateControls.StyledRadioButton();
			this.radRunAsServerUser = new AutomateControls.StyledRadioButton();
			this.grpDataGatewaysUser = new System.Windows.Forms.GroupBox();
			this.txtDGPassword = new AutomateControls.SecurePasswordTextBox();
			this.lblDGDomain = new System.Windows.Forms.Label();
			this.txtDGDomain = new AutomateControls.Textboxes.StyledTextBox();
			this.lblDGPassword = new System.Windows.Forms.Label();
			this.lblDGUsername = new System.Windows.Forms.Label();
			this.txtDGUsername = new AutomateControls.Textboxes.StyledTextBox();
			this.chkEnableDataGatewayProcess = new System.Windows.Forms.CheckBox();
			this.tpAuthenticationServerIntegration = new System.Windows.Forms.TabPage();
			this.gbBrokerSettings = new System.Windows.Forms.GroupBox();
			this.txtBrokerPassword = new AutomateControls.SecurePasswordTextBox();
			this.txtEnvironmentIdentifier = new AutomateControls.Textboxes.StyledTextBox();
			this.lblEnvironmentIdentifier = new System.Windows.Forms.Label();
			this.txtBrokerAddress = new AutomateControls.Textboxes.StyledTextBox();
			this.lblBrokerAddress = new System.Windows.Forms.Label();
			this.lblPassword = new System.Windows.Forms.Label();
			this.txtBrokerUsername = new AutomateControls.Textboxes.StyledTextBox();
			this.lblUsername = new System.Windows.Forms.Label();
            this.tpASCR = new System.Windows.Forms.TabPage();
            this.ctlASCRSettings = new BluePrism.BPServer.ctlASCRSettings();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tcConfig.SuspendLayout();
			this.tpDetails.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbGenericWindowsServiceWarning)).BeginInit();
			this.gbBinding.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbSslBindingWarningIcon)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
			this.tpSslCertificate.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvSslBindings)).BeginInit();
			this.tpKeys.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).BeginInit();
			this.tpServerServices.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbWindowsServiceWarning)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvWindowsServices)).BeginInit();
			this.tpLogging.SuspendLayout();
			this.tpDataGateways.SuspendLayout();
			this.fraDGUser.SuspendLayout();
			this.grpDataGatewaysUser.SuspendLayout();
			this.tpAuthenticationServerIntegration.SuspendLayout();
			this.gbBrokerSettings.SuspendLayout();
            this.tpASCR.SuspendLayout();
            this.SuspendLayout();
			// 
			// btnOK
			// 
			resources.ApplyResources(this.btnOK, "btnOK");
			this.btnOK.BackColor = System.Drawing.Color.White;
			this.btnOK.Name = "btnOK";
			this.btnOK.UseVisualStyleBackColor = false;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.BackColor = System.Drawing.Color.White;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = false;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// chkDisableScheduler
			// 
			resources.ApplyResources(this.chkDisableScheduler, "chkDisableScheduler");
			this.chkDisableScheduler.Name = "chkDisableScheduler";
			this.chkDisableScheduler.UseVisualStyleBackColor = true;
			// 
			// tcConfig
			// 
			resources.ApplyResources(this.tcConfig, "tcConfig");
			this.tcConfig.Controls.Add(this.tpDetails);
			this.tcConfig.Controls.Add(this.tpSslCertificate);
			this.tcConfig.Controls.Add(this.tpKeys);
			this.tcConfig.Controls.Add(this.tpServerServices);
			this.tcConfig.Controls.Add(this.tpLogging);
			this.tcConfig.Controls.Add(this.tpDataGateways);
            this.tcConfig.Controls.Add(this.tpASCR);
            this.tcConfig.Controls.Add(this.tpAuthenticationServerIntegration);
			this.tcConfig.Cursor = System.Windows.Forms.Cursors.Default;
			this.tcConfig.Name = "tcConfig";
			this.tcConfig.SelectedIndex = 0;
			// 
			// tpDetails
			// 
			this.tpDetails.Controls.Add(this.pbGenericWindowsServiceWarning);
			this.tpDetails.Controls.Add(this.lblGenericWindowsServiceWarning);
			this.tpDetails.Controls.Add(this.gbBinding);
			this.tpDetails.Controls.Add(this.txtConnectionModeHelp);
			this.tpDetails.Controls.Add(this.lblConnectionMode);
			this.tpDetails.Controls.Add(this.cmbConnectionMode);
			this.tpDetails.Controls.Add(this.cmbConnection);
			this.tpDetails.Controls.Add(this.label1);
			this.tpDetails.Controls.Add(this.chkDisableScheduler);
			this.tpDetails.Controls.Add(this.label2);
			this.tpDetails.Controls.Add(this.txtName);
			resources.ApplyResources(this.tpDetails, "tpDetails");
			this.tpDetails.Name = "tpDetails";
			this.tpDetails.UseVisualStyleBackColor = true;
			// 
			// pbGenericWindowsServiceWarning
			// 
			resources.ApplyResources(this.pbGenericWindowsServiceWarning, "pbGenericWindowsServiceWarning");
			this.pbGenericWindowsServiceWarning.Name = "pbGenericWindowsServiceWarning";
			this.pbGenericWindowsServiceWarning.TabStop = false;
			// 
			// lblGenericWindowsServiceWarning
			// 
			resources.ApplyResources(this.lblGenericWindowsServiceWarning, "lblGenericWindowsServiceWarning");
			this.lblGenericWindowsServiceWarning.Name = "lblGenericWindowsServiceWarning";
			// 
			// gbBinding
			// 
			this.gbBinding.Controls.Add(this.chkOrdered);
			this.gbBinding.Controls.Add(this.lblOrdered);
			this.gbBinding.Controls.Add(this.pbSslBindingWarningIcon);
			this.gbBinding.Controls.Add(this.lblSslCertificateWarning);
			this.gbBinding.Controls.Add(this.label3);
			this.gbBinding.Controls.Add(this.txtBindTo);
			this.gbBinding.Controls.Add(this.lblBindToLabel);
			this.gbBinding.Controls.Add(this.numPort);
			resources.ApplyResources(this.gbBinding, "gbBinding");
			this.gbBinding.Name = "gbBinding";
			this.gbBinding.TabStop = false;
			// 
			// chkOrdered
			// 
			resources.ApplyResources(this.chkOrdered, "chkOrdered");
			this.chkOrdered.Name = "chkOrdered";
			this.chkOrdered.UseVisualStyleBackColor = true;
			// 
			// lblOrdered
			// 
			resources.ApplyResources(this.lblOrdered, "lblOrdered");
			this.lblOrdered.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblOrdered.Name = "lblOrdered";
			// 
			// pbSslBindingWarningIcon
			// 
			resources.ApplyResources(this.pbSslBindingWarningIcon, "pbSslBindingWarningIcon");
			this.pbSslBindingWarningIcon.Name = "pbSslBindingWarningIcon";
			this.pbSslBindingWarningIcon.TabStop = false;
			// 
			// lblSslCertificateWarning
			// 
			resources.ApplyResources(this.lblSslCertificateWarning, "lblSslCertificateWarning");
			this.lblSslCertificateWarning.Name = "lblSslCertificateWarning";
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// txtBindTo
			// 
			resources.ApplyResources(this.txtBindTo, "txtBindTo");
			this.txtBindTo.BorderColor = System.Drawing.Color.Empty;
			this.txtBindTo.Name = "txtBindTo";
			this.txtBindTo.TextChanged += new System.EventHandler(this.txtBindTo_TextChanged);
			// 
			// lblBindToLabel
			// 
			resources.ApplyResources(this.lblBindToLabel, "lblBindToLabel");
			this.lblBindToLabel.Name = "lblBindToLabel";
			// 
			// numPort
			// 
			resources.ApplyResources(this.numPort, "numPort");
			this.numPort.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.numPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numPort.Name = "numPort";
			this.numPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numPort.ValueChanged += new System.EventHandler(this.numPort_ValueChanged);
			// 
			// txtConnectionModeHelp
			// 
			resources.ApplyResources(this.txtConnectionModeHelp, "txtConnectionModeHelp");
			this.txtConnectionModeHelp.BorderColor = System.Drawing.Color.Empty;
			this.txtConnectionModeHelp.Name = "txtConnectionModeHelp";
			this.txtConnectionModeHelp.ReadOnly = true;
			// 
			// lblConnectionMode
			// 
			resources.ApplyResources(this.lblConnectionMode, "lblConnectionMode");
			this.lblConnectionMode.Name = "lblConnectionMode";
			// 
			// cmbConnectionMode
			// 
			resources.ApplyResources(this.cmbConnectionMode, "cmbConnectionMode");
			this.cmbConnectionMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbConnectionMode.FormattingEnabled = true;
			this.cmbConnectionMode.Name = "cmbConnectionMode";
			// 
			// cmbConnection
			// 
			resources.ApplyResources(this.cmbConnection, "cmbConnection");
			this.cmbConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbConnection.FormattingEnabled = true;
			this.cmbConnection.Name = "cmbConnection";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// txtName
			// 
			resources.ApplyResources(this.txtName, "txtName");
			this.txtName.BackColor = System.Drawing.SystemColors.Window;
            this.txtName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.txtName.Name = "txtName";
			this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			// 
			// tpSslCertificate
			// 
			this.tpSslCertificate.Controls.Add(this.llDeleteSslBinding);
			this.tpSslCertificate.Controls.Add(this.llViewSslBinding);
			this.tpSslCertificate.Controls.Add(this.dgvSslBindings);
			this.tpSslCertificate.Controls.Add(this.llAddSslBinding);
			this.tpSslCertificate.Controls.Add(this.lblSslBindingsIntro);
			resources.ApplyResources(this.tpSslCertificate, "tpSslCertificate");
			this.tpSslCertificate.Name = "tpSslCertificate";
			this.tpSslCertificate.UseVisualStyleBackColor = true;
			// 
			// llDeleteSslBinding
			// 
			resources.ApplyResources(this.llDeleteSslBinding, "llDeleteSslBinding");
			this.llDeleteSslBinding.LinkColor = System.Drawing.SystemColors.ControlText;
			this.llDeleteSslBinding.Name = "llDeleteSslBinding";
			this.llDeleteSslBinding.TabStop = true;
			this.llDeleteSslBinding.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleDeleteSslBinding);
			// 
			// llViewSslBinding
			// 
			resources.ApplyResources(this.llViewSslBinding, "llViewSslBinding");
			this.llViewSslBinding.LinkColor = System.Drawing.SystemColors.ControlText;
			this.llViewSslBinding.Name = "llViewSslBinding";
			this.llViewSslBinding.TabStop = true;
			this.llViewSslBinding.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleViewSslBinding);
			// 
			// dgvSslBindings
			// 
			this.dgvSslBindings.AllowUserToAddRows = false;
			this.dgvSslBindings.AllowUserToDeleteRows = false;
			resources.ApplyResources(this.dgvSslBindings, "dgvSslBindings");
			this.dgvSslBindings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvSslBindings.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dgvSslBindings.BackgroundColor = System.Drawing.SystemColors.Control;
			this.dgvSslBindings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvSslBindings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.CertificateDescription});
			this.dgvSslBindings.MultiSelect = false;
			this.dgvSslBindings.Name = "dgvSslBindings";
			this.dgvSslBindings.RowHeadersVisible = false;
			this.dgvSslBindings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.DataPropertyName = "EndPoint";
			this.dataGridViewTextBoxColumn1.FillWeight = 30F;
			resources.ApplyResources(this.dataGridViewTextBoxColumn1, "dataGridViewTextBoxColumn1");
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// CertificateDescription
			// 
			this.CertificateDescription.DataPropertyName = "CertificateDescription";
			dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.CertificateDescription.DefaultCellStyle = dataGridViewCellStyle6;
			resources.ApplyResources(this.CertificateDescription, "CertificateDescription");
			this.CertificateDescription.Name = "CertificateDescription";
			this.CertificateDescription.ReadOnly = true;
			// 
			// llAddSslBinding
			// 
			resources.ApplyResources(this.llAddSslBinding, "llAddSslBinding");
			this.llAddSslBinding.LinkColor = System.Drawing.SystemColors.ControlText;
			this.llAddSslBinding.Name = "llAddSslBinding";
			this.llAddSslBinding.TabStop = true;
			this.llAddSslBinding.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleAddSslBinding);
			// 
			// lblSslBindingsIntro
			// 
			resources.ApplyResources(this.lblSslBindingsIntro, "lblSslBindingsIntro");
			this.lblSslBindingsIntro.Name = "lblSslBindingsIntro";
			// 
			// tpKeys
			// 
			this.tpKeys.Controls.Add(this.lblExternalFolder);
			this.tpKeys.Controls.Add(this.btnBrowse);
			this.tpKeys.Controls.Add(this.txtKeyFolder);
			this.tpKeys.Controls.Add(this.chkExternalFiles);
			this.tpKeys.Controls.Add(this.llDeleteKey);
			this.tpKeys.Controls.Add(this.llEditKey);
			this.tpKeys.Controls.Add(this.llAddKey);
			this.tpKeys.Controls.Add(this.dgvKeys);
			resources.ApplyResources(this.tpKeys, "tpKeys");
			this.tpKeys.Name = "tpKeys";
			this.tpKeys.UseVisualStyleBackColor = true;
			// 
			// lblExternalFolder
			// 
			resources.ApplyResources(this.lblExternalFolder, "lblExternalFolder");
			this.lblExternalFolder.Name = "lblExternalFolder";
			// 
			// btnBrowse
			// 
			resources.ApplyResources(this.btnBrowse, "btnBrowse");
			this.btnBrowse.BackColor = System.Drawing.Color.White;
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.UseVisualStyleBackColor = false;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// txtKeyFolder
			// 
			resources.ApplyResources(this.txtKeyFolder, "txtKeyFolder");
			this.txtKeyFolder.BorderColor = System.Drawing.Color.Empty;
			this.txtKeyFolder.Name = "txtKeyFolder";
			this.txtKeyFolder.ReadOnly = true;
			// 
			// chkExternalFiles
			// 
			resources.ApplyResources(this.chkExternalFiles, "chkExternalFiles");
			this.chkExternalFiles.Name = "chkExternalFiles";
			this.chkExternalFiles.UseVisualStyleBackColor = true;
			this.chkExternalFiles.CheckedChanged += new System.EventHandler(this.chkExternalFiles_CheckedChanged);
			// 
			// llDeleteKey
			// 
			resources.ApplyResources(this.llDeleteKey, "llDeleteKey");
			this.llDeleteKey.LinkColor = System.Drawing.SystemColors.ControlText;
			this.llDeleteKey.Name = "llDeleteKey";
			this.llDeleteKey.TabStop = true;
			this.llDeleteKey.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleDeleteKey);
			// 
			// llEditKey
			// 
			resources.ApplyResources(this.llEditKey, "llEditKey");
			this.llEditKey.LinkColor = System.Drawing.SystemColors.ControlText;
			this.llEditKey.Name = "llEditKey";
			this.llEditKey.TabStop = true;
			this.llEditKey.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleEditKey);
			// 
			// llAddKey
			// 
			resources.ApplyResources(this.llAddKey, "llAddKey");
			this.llAddKey.LinkColor = System.Drawing.SystemColors.ControlText;
			this.llAddKey.Name = "llAddKey";
			this.llAddKey.TabStop = true;
			this.llAddKey.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleAddKey);
			// 
			// dgvKeys
			// 
			this.dgvKeys.AllowUserToAddRows = false;
			this.dgvKeys.AllowUserToDeleteRows = false;
			this.dgvKeys.AllowUserToResizeRows = false;
			resources.ApplyResources(this.dgvKeys, "dgvKeys");
			this.dgvKeys.BackgroundColor = System.Drawing.SystemColors.Control;
			this.dgvKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvKeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colMethod});
			this.dgvKeys.MultiSelect = false;
			this.dgvKeys.Name = "dgvKeys";
			this.dgvKeys.RowHeadersVisible = false;
			this.dgvKeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvKeys.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvKeys_CellDoubleClick);
			// 
			// colName
			// 
			resources.ApplyResources(this.colName, "colName");
			this.colName.Name = "colName";
			this.colName.ReadOnly = true;
			// 
			// colMethod
			// 
			this.colMethod.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.colMethod, "colMethod");
			this.colMethod.Name = "colMethod";
			this.colMethod.ReadOnly = true;
			// 
			// tpServerServices
			// 
			this.tpServerServices.Controls.Add(this.btnCreateService);
			this.tpServerServices.Controls.Add(this.llManagePermissions);
			this.tpServerServices.Controls.Add(this.txtWindowsServiceWarning);
			this.tpServerServices.Controls.Add(this.pbWindowsServiceWarning);
			this.tpServerServices.Controls.Add(this.dgvWindowsServices);
			this.tpServerServices.Controls.Add(this.lblWindowsServices);
			resources.ApplyResources(this.tpServerServices, "tpServerServices");
			this.tpServerServices.Name = "tpServerServices";
			this.tpServerServices.UseVisualStyleBackColor = true;
			// 
			// btnCreateService
			// 
			resources.ApplyResources(this.btnCreateService, "btnCreateService");
			this.btnCreateService.BackColor = System.Drawing.Color.White;
			this.btnCreateService.Name = "btnCreateService";
			this.btnCreateService.UseVisualStyleBackColor = false;
			this.btnCreateService.Click += new System.EventHandler(this.HandlesCreateServiceClick);
			// 
			// llManagePermissions
			// 
			resources.ApplyResources(this.llManagePermissions, "llManagePermissions");
			this.llManagePermissions.LinkColor = System.Drawing.SystemColors.ControlText;
			this.llManagePermissions.Name = "llManagePermissions";
			this.llManagePermissions.TabStop = true;
			this.llManagePermissions.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llManagePermissions_LinkClicked);
			// 
			// txtWindowsServiceWarning
			// 
			this.txtWindowsServiceWarning.AcceptsReturn = true;
			resources.ApplyResources(this.txtWindowsServiceWarning, "txtWindowsServiceWarning");
			this.txtWindowsServiceWarning.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtWindowsServiceWarning.BorderColor = System.Drawing.Color.Empty;
			this.txtWindowsServiceWarning.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtWindowsServiceWarning.Name = "txtWindowsServiceWarning";
			this.txtWindowsServiceWarning.ReadOnly = true;
			// 
			// pbWindowsServiceWarning
			// 
			resources.ApplyResources(this.pbWindowsServiceWarning, "pbWindowsServiceWarning");
			this.pbWindowsServiceWarning.Name = "pbWindowsServiceWarning";
			this.pbWindowsServiceWarning.TabStop = false;
			// 
			// dgvWindowsServices
			// 
			this.dgvWindowsServices.AllowUserToAddRows = false;
			this.dgvWindowsServices.AllowUserToDeleteRows = false;
			resources.ApplyResources(this.dgvWindowsServices, "dgvWindowsServices");
			this.dgvWindowsServices.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvWindowsServices.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dgvWindowsServices.BackgroundColor = System.Drawing.SystemColors.Control;
			this.dgvWindowsServices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvWindowsServices.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colServiceName,
            this.colFullPath,
            this.colStartName,
            this.colStartMode,
            this.colState,
            this.colPermission});
			dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvWindowsServices.DefaultCellStyle = dataGridViewCellStyle8;
			this.dgvWindowsServices.MultiSelect = false;
			this.dgvWindowsServices.Name = "dgvWindowsServices";
			this.dgvWindowsServices.RowHeadersVisible = false;
			dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvWindowsServices.RowsDefaultCellStyle = dataGridViewCellStyle9;
			this.dgvWindowsServices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvWindowsServices.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.HandleWindowsServicesCellFormatting);
			this.dgvWindowsServices.SelectionChanged += new System.EventHandler(this.HandleWindowsServicesSelectionChanged);
			// 
			// colServiceName
			// 
			this.colServiceName.DataPropertyName = "Name";
			this.colServiceName.FillWeight = 25F;
			resources.ApplyResources(this.colServiceName, "colServiceName");
			this.colServiceName.Name = "colServiceName";
			// 
			// colFullPath
			// 
			this.colFullPath.DataPropertyName = "PathName";
			this.colFullPath.FillWeight = 35F;
			resources.ApplyResources(this.colFullPath, "colFullPath");
			this.colFullPath.Name = "colFullPath";
			this.colFullPath.ReadOnly = true;
			// 
			// colStartName
			// 
			this.colStartName.DataPropertyName = "StartName";
			this.colStartName.FillWeight = 22F;
			resources.ApplyResources(this.colStartName, "colStartName");
			this.colStartName.Name = "colStartName";
			this.colStartName.ReadOnly = true;
			// 
			// colStartMode
			// 
			this.colStartMode.DataPropertyName = "StartMode";
			this.colStartMode.FillWeight = 18F;
			resources.ApplyResources(this.colStartMode, "colStartMode");
			this.colStartMode.Name = "colStartMode";
			// 
			// colState
			// 
			this.colState.DataPropertyName = "State";
			this.colState.FillWeight = 13F;
			resources.ApplyResources(this.colState, "colState");
			this.colState.Name = "colState";
			// 
			// colPermission
			// 
			this.colPermission.DataPropertyName = "HasUrlPermission";
			dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle7.NullValue = null;
			dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.colPermission.DefaultCellStyle = dataGridViewCellStyle7;
			this.colPermission.FillWeight = 5F;
			resources.ApplyResources(this.colPermission, "colPermission");
			this.colPermission.Name = "colPermission";
			// 
			// lblWindowsServices
			// 
			resources.ApplyResources(this.lblWindowsServices, "lblWindowsServices");
			this.lblWindowsServices.AutoEllipsis = true;
			this.lblWindowsServices.Name = "lblWindowsServices";
			this.toolTip1.SetToolTip(this.lblWindowsServices, resources.GetString("lblWindowsServices.ToolTip"));
			// 
			// tpLogging
			// 
			this.tpLogging.Controls.Add(this.chkLogTraffic);
			this.tpLogging.Controls.Add(this.chkVerbose);
			this.tpLogging.Controls.Add(this.chkServiceStatusEventLogging);
			resources.ApplyResources(this.tpLogging, "tpLogging");
			this.tpLogging.Name = "tpLogging";
			this.tpLogging.UseVisualStyleBackColor = true;
			// 
			// chkLogTraffic
			// 
			resources.ApplyResources(this.chkLogTraffic, "chkLogTraffic");
			this.chkLogTraffic.Name = "chkLogTraffic";
			this.chkLogTraffic.UseVisualStyleBackColor = true;
			// 
			// chkVerbose
			// 
			resources.ApplyResources(this.chkVerbose, "chkVerbose");
			this.chkVerbose.Name = "chkVerbose";
			this.chkVerbose.UseVisualStyleBackColor = true;
			// 
			// chkServiceStatusEventLogging
			// 
			resources.ApplyResources(this.chkServiceStatusEventLogging, "chkServiceStatusEventLogging");
			this.chkServiceStatusEventLogging.Checked = true;
			this.chkServiceStatusEventLogging.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkServiceStatusEventLogging.Name = "chkServiceStatusEventLogging";
			this.chkServiceStatusEventLogging.UseVisualStyleBackColor = true;
			this.chkServiceStatusEventLogging.CheckedChanged += new System.EventHandler(this.chkServiceStatusEventLogging_CheckedChanged);
			// 
			// tpDataGateways
			// 
			this.tpDataGateways.Controls.Add(this.chkEnabledDataGatewaysTrace);
			this.tpDataGateways.Controls.Add(this.chkLogDataGatewaysConsole);
			this.tpDataGateways.Controls.Add(this.lblDGPort);
			this.tpDataGateways.Controls.Add(this.txtDGPort);
			this.tpDataGateways.Controls.Add(this.fraDGUser);
			this.tpDataGateways.Controls.Add(this.chkEnableDataGatewayProcess);
			resources.ApplyResources(this.tpDataGateways, "tpDataGateways");
			this.tpDataGateways.Name = "tpDataGateways";
			this.tpDataGateways.UseVisualStyleBackColor = true;
			// 
			// chkEnabledDataGatewaysTrace
			// 
			resources.ApplyResources(this.chkEnabledDataGatewaysTrace, "chkEnabledDataGatewaysTrace");
			this.chkEnabledDataGatewaysTrace.Name = "chkEnabledDataGatewaysTrace";
			this.chkEnabledDataGatewaysTrace.UseVisualStyleBackColor = true;
			// 
			// chkLogDataGatewaysConsole
			// 
			resources.ApplyResources(this.chkLogDataGatewaysConsole, "chkLogDataGatewaysConsole");
			this.chkLogDataGatewaysConsole.Name = "chkLogDataGatewaysConsole";
			this.chkLogDataGatewaysConsole.UseVisualStyleBackColor = true;
			this.chkLogDataGatewaysConsole.CheckedChanged += new System.EventHandler(this.chkLogDataGatewaysConsole_CheckedChanged_1);
			// 
			// lblDGPort
			// 
			resources.ApplyResources(this.lblDGPort, "lblDGPort");
			this.lblDGPort.Name = "lblDGPort";
			// 
			// txtDGPort
			// 
			this.txtDGPort.BorderColor = System.Drawing.Color.Empty;
			resources.ApplyResources(this.txtDGPort, "txtDGPort");
			this.txtDGPort.Name = "txtDGPort";
			// 
			// fraDGUser
			// 
			this.fraDGUser.Controls.Add(this.radRunAsSpecificUser);
			this.fraDGUser.Controls.Add(this.radRunAsServerUser);
			this.fraDGUser.Controls.Add(this.grpDataGatewaysUser);
			resources.ApplyResources(this.fraDGUser, "fraDGUser");
			this.fraDGUser.Name = "fraDGUser";
			this.fraDGUser.TabStop = false;
			// 
			// radRunAsSpecificUser
			// 
			this.radRunAsSpecificUser.ButtonHeight = 21;
			this.radRunAsSpecificUser.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.radRunAsSpecificUser.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
			this.radRunAsSpecificUser.FocusDiameter = 16;
			this.radRunAsSpecificUser.FocusThickness = 3;
			this.radRunAsSpecificUser.FocusYLocation = 9;
			this.radRunAsSpecificUser.ForceFocus = true;
			this.radRunAsSpecificUser.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
			this.radRunAsSpecificUser.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
			resources.ApplyResources(this.radRunAsSpecificUser, "radRunAsSpecificUser");
			this.radRunAsSpecificUser.MouseLeaveColor = System.Drawing.Color.White;
			this.radRunAsSpecificUser.Name = "radRunAsSpecificUser";
			this.radRunAsSpecificUser.RadioButtonDiameter = 12;
			this.radRunAsSpecificUser.RadioButtonThickness = 2;
			this.radRunAsSpecificUser.RadioYLocation = 7;
			this.radRunAsSpecificUser.StringYLocation = 1;
			this.radRunAsSpecificUser.TabStop = true;
			this.radRunAsSpecificUser.TextColor = System.Drawing.Color.Black;
			this.radRunAsSpecificUser.UseVisualStyleBackColor = true;
			this.radRunAsSpecificUser.CheckedChanged += new System.EventHandler(this.radRunAsSpecificUser_CheckedChanged);
			// 
			// radRunAsServerUser
			// 
			this.radRunAsServerUser.ButtonHeight = 21;
			this.radRunAsServerUser.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.radRunAsServerUser.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
			this.radRunAsServerUser.FocusDiameter = 16;
			this.radRunAsServerUser.FocusThickness = 3;
			this.radRunAsServerUser.FocusYLocation = 9;
			this.radRunAsServerUser.ForceFocus = true;
			this.radRunAsServerUser.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
			this.radRunAsServerUser.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
			resources.ApplyResources(this.radRunAsServerUser, "radRunAsServerUser");
			this.radRunAsServerUser.MouseLeaveColor = System.Drawing.Color.White;
			this.radRunAsServerUser.Name = "radRunAsServerUser";
			this.radRunAsServerUser.RadioButtonDiameter = 12;
			this.radRunAsServerUser.RadioButtonThickness = 2;
			this.radRunAsServerUser.RadioYLocation = 7;
			this.radRunAsServerUser.StringYLocation = 1;
			this.radRunAsServerUser.TabStop = true;
			this.radRunAsServerUser.TextColor = System.Drawing.Color.Black;
			this.radRunAsServerUser.UseVisualStyleBackColor = true;
			this.radRunAsServerUser.CheckedChanged += new System.EventHandler(this.radRunAsServerUser_CheckedChanged);
			// 
			// grpDataGatewaysUser
			// 
			this.grpDataGatewaysUser.Controls.Add(this.txtDGPassword);
			this.grpDataGatewaysUser.Controls.Add(this.lblDGDomain);
			this.grpDataGatewaysUser.Controls.Add(this.txtDGDomain);
			this.grpDataGatewaysUser.Controls.Add(this.lblDGPassword);
			this.grpDataGatewaysUser.Controls.Add(this.lblDGUsername);
			this.grpDataGatewaysUser.Controls.Add(this.txtDGUsername);
			resources.ApplyResources(this.grpDataGatewaysUser, "grpDataGatewaysUser");
			this.grpDataGatewaysUser.Name = "grpDataGatewaysUser";
			this.grpDataGatewaysUser.TabStop = false;
			// 
			// txtDGPassword
			// 
			this.txtDGPassword.BorderColor = System.Drawing.Color.Empty;
			resources.ApplyResources(this.txtDGPassword, "txtDGPassword");
			this.txtDGPassword.Name = "txtDGPassword";
			// 
			// lblDGDomain
			// 
			resources.ApplyResources(this.lblDGDomain, "lblDGDomain");
			this.lblDGDomain.Name = "lblDGDomain";
			// 
			// txtDGDomain
			// 
			this.txtDGDomain.BorderColor = System.Drawing.Color.Empty;
			resources.ApplyResources(this.txtDGDomain, "txtDGDomain");
			this.txtDGDomain.Name = "txtDGDomain";
			// 
			// lblDGPassword
			// 
			resources.ApplyResources(this.lblDGPassword, "lblDGPassword");
			this.lblDGPassword.Name = "lblDGPassword";
			// 
			// lblDGUsername
			// 
			resources.ApplyResources(this.lblDGUsername, "lblDGUsername");
			this.lblDGUsername.Name = "lblDGUsername";
			// 
			// txtDGUsername
			// 
			this.txtDGUsername.BorderColor = System.Drawing.Color.Empty;
			resources.ApplyResources(this.txtDGUsername, "txtDGUsername");
			this.txtDGUsername.Name = "txtDGUsername";
			// 
			// chkEnableDataGatewayProcess
			// 
			resources.ApplyResources(this.chkEnableDataGatewayProcess, "chkEnableDataGatewayProcess");
			this.chkEnableDataGatewayProcess.Name = "chkEnableDataGatewayProcess";
			this.chkEnableDataGatewayProcess.UseVisualStyleBackColor = true;
			this.chkEnableDataGatewayProcess.CheckedChanged += new System.EventHandler(this.chkEnableDataGatewayProcess_CheckedChanged);
			// 
			// tpAuthenticationServerIntegration
			// 
			this.tpAuthenticationServerIntegration.Controls.Add(this.gbBrokerSettings);
			resources.ApplyResources(this.tpAuthenticationServerIntegration, "tpAuthenticationServerIntegration");
			this.tpAuthenticationServerIntegration.Name = "tpAuthenticationServerIntegration";
			this.tpAuthenticationServerIntegration.UseVisualStyleBackColor = true;
			// 
			// gbBrokerSettings
			// 
			resources.ApplyResources(this.gbBrokerSettings, "gbBrokerSettings");
			this.gbBrokerSettings.Controls.Add(this.txtBrokerPassword);
			this.gbBrokerSettings.Controls.Add(this.txtEnvironmentIdentifier);
			this.gbBrokerSettings.Controls.Add(this.lblEnvironmentIdentifier);
			this.gbBrokerSettings.Controls.Add(this.txtBrokerAddress);
			this.gbBrokerSettings.Controls.Add(this.lblBrokerAddress);
			this.gbBrokerSettings.Controls.Add(this.lblPassword);
			this.gbBrokerSettings.Controls.Add(this.txtBrokerUsername);
			this.gbBrokerSettings.Controls.Add(this.lblUsername);
			this.gbBrokerSettings.Name = "gbBrokerSettings";
			this.gbBrokerSettings.TabStop = false;
			// 
			// txtBrokerPassword
			// 
			this.txtBrokerPassword.BackColor = System.Drawing.SystemColors.Window;
			this.txtBrokerPassword.BorderColor = System.Drawing.Color.Empty;
			resources.ApplyResources(this.txtBrokerPassword, "txtBrokerPassword");
			this.txtBrokerPassword.Name = "txtBrokerPassword";
			// 
			// txtEnvironmentIdentifier
			// 
			resources.ApplyResources(this.txtEnvironmentIdentifier, "txtEnvironmentIdentifier");
			this.txtEnvironmentIdentifier.BackColor = System.Drawing.SystemColors.Window;
			this.txtEnvironmentIdentifier.BorderColor = System.Drawing.Color.Empty;
			this.txtEnvironmentIdentifier.Name = "txtEnvironmentIdentifier";
			// 
			// lblEnvironmentIdentifier
			// 
			resources.ApplyResources(this.lblEnvironmentIdentifier, "lblEnvironmentIdentifier");
			this.lblEnvironmentIdentifier.Name = "lblEnvironmentIdentifier";
			// 
			// txtBrokerAddress
			// 
			resources.ApplyResources(this.txtBrokerAddress, "txtBrokerAddress");
			this.txtBrokerAddress.BorderColor = System.Drawing.Color.Empty;
			this.txtBrokerAddress.Name = "txtBrokerAddress";
			// 
			// lblBrokerAddress
			// 
			resources.ApplyResources(this.lblBrokerAddress, "lblBrokerAddress");
			this.lblBrokerAddress.Name = "lblBrokerAddress";
			// 
			// lblPassword
			// 
			resources.ApplyResources(this.lblPassword, "lblPassword");
			this.lblPassword.Name = "lblPassword";
			// 
			// txtBrokerUsername
			// 
			resources.ApplyResources(this.txtBrokerUsername, "txtBrokerUsername");
			this.txtBrokerUsername.BackColor = System.Drawing.SystemColors.Window;
			this.txtBrokerUsername.BorderColor = System.Drawing.Color.Empty;
			this.txtBrokerUsername.Name = "txtBrokerUsername";
			// 
			// lblUsername
			// 
			resources.ApplyResources(this.lblUsername, "lblUsername");
			this.lblUsername.Name = "lblUsername";
			// 
			// tpASCR
			// 
			this.tpASCR.Controls.Add(this.ctlASCRSettings);
			resources.ApplyResources(this.tpASCR, "tpASCR");
			this.tpASCR.Name = "tpASCR";
			this.tpASCR.UseVisualStyleBackColor = true;
			// 
			// ctlASCRSettings1
			// 
			resources.ApplyResources(this.ctlASCRSettings, "ctlASCRSettings1");
			this.ctlASCRSettings.Name = "ctlASCRSettings1";
			// 
			// toolTip1
			// 
			this.toolTip1.ShowAlways = true;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.DataPropertyName = "CertificateDescription";
			dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewTextBoxColumn2.DefaultCellStyle = dataGridViewCellStyle10;
			resources.ApplyResources(this.dataGridViewTextBoxColumn2, "dataGridViewTextBoxColumn2");
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn3
			// 
			resources.ApplyResources(this.dataGridViewTextBoxColumn3, "dataGridViewTextBoxColumn3");
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			resources.ApplyResources(this.dataGridViewTextBoxColumn4, "dataGridViewTextBoxColumn4");
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn5
			// 
			this.dataGridViewTextBoxColumn5.DataPropertyName = "Name";
			this.dataGridViewTextBoxColumn5.FillWeight = 25F;
			resources.ApplyResources(this.dataGridViewTextBoxColumn5, "dataGridViewTextBoxColumn5");
			this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			// 
			// dataGridViewTextBoxColumn6
			// 
			this.dataGridViewTextBoxColumn6.DataPropertyName = "PathName";
			this.dataGridViewTextBoxColumn6.FillWeight = 35F;
			resources.ApplyResources(this.dataGridViewTextBoxColumn6, "dataGridViewTextBoxColumn6");
			this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
			this.dataGridViewTextBoxColumn6.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn7
			// 
			this.dataGridViewTextBoxColumn7.DataPropertyName = "StartName";
			this.dataGridViewTextBoxColumn7.FillWeight = 22F;
			resources.ApplyResources(this.dataGridViewTextBoxColumn7, "dataGridViewTextBoxColumn7");
			this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
			this.dataGridViewTextBoxColumn7.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn8
			// 
			this.dataGridViewTextBoxColumn8.DataPropertyName = "StartMode";
			this.dataGridViewTextBoxColumn8.FillWeight = 18F;
			resources.ApplyResources(this.dataGridViewTextBoxColumn8, "dataGridViewTextBoxColumn8");
			this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
			// 
			// dataGridViewTextBoxColumn9
			// 
			this.dataGridViewTextBoxColumn9.DataPropertyName = "State";
			this.dataGridViewTextBoxColumn9.FillWeight = 13F;
			resources.ApplyResources(this.dataGridViewTextBoxColumn9, "dataGridViewTextBoxColumn9");
			this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
			// 
			// frmServerConfig
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.tcConfig);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmServerConfig";
			this.tcConfig.ResumeLayout(false);
			this.tpDetails.ResumeLayout(false);
			this.tpDetails.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbGenericWindowsServiceWarning)).EndInit();
			this.gbBinding.ResumeLayout(false);
			this.gbBinding.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbSslBindingWarningIcon)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
			this.tpSslCertificate.ResumeLayout(false);
			this.tpSslCertificate.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvSslBindings)).EndInit();
			this.tpKeys.ResumeLayout(false);
			this.tpKeys.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).EndInit();
			this.tpServerServices.ResumeLayout(false);
			this.tpServerServices.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbWindowsServiceWarning)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvWindowsServices)).EndInit();
			this.tpLogging.ResumeLayout(false);
			this.tpLogging.PerformLayout();
			this.tpDataGateways.ResumeLayout(false);
			this.tpDataGateways.PerformLayout();
			this.fraDGUser.ResumeLayout(false);
			this.grpDataGatewaysUser.ResumeLayout(false);
			this.grpDataGatewaysUser.PerformLayout();
			this.tpAuthenticationServerIntegration.ResumeLayout(false);
			this.gbBrokerSettings.ResumeLayout(false);
			this.gbBrokerSettings.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private AutomateControls.Buttons.StandardStyledButton btnOK;
        private AutomateControls.Buttons.StandardStyledButton btnCancel;
        private System.Windows.Forms.CheckBox chkDisableScheduler;
        private System.Windows.Forms.TabControl tcConfig;
        private System.Windows.Forms.TabPage tpDetails;
        private System.Windows.Forms.TabPage tpKeys;
        private System.Windows.Forms.Label lblExternalFolder;
        private AutomateControls.Buttons.StandardStyledButton btnBrowse;
        private AutomateControls.Textboxes.StyledTextBox txtKeyFolder;
        private System.Windows.Forms.CheckBox chkExternalFiles;
        private AutomateControls.BulletedLinkLabel llDeleteKey;
        private AutomateControls.BulletedLinkLabel llEditKey;
        private AutomateControls.BulletedLinkLabel llAddKey;
        private System.Windows.Forms.DataGridView dgvKeys;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMethod;
        private System.Windows.Forms.TabPage tpSslCertificate;
        private AutomateControls.BulletedLinkLabel llAddSslBinding;
        private System.Windows.Forms.Label lblSslBindingsIntro;
        private System.Windows.Forms.DataGridView dgvSslBindings;
        private AutomateControls.BulletedLinkLabel llDeleteSslBinding;
        private AutomateControls.BulletedLinkLabel llViewSslBinding;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn CertificateDescription;
        private System.Windows.Forms.GroupBox gbBinding;
        private System.Windows.Forms.PictureBox pbSslBindingWarningIcon;
        private System.Windows.Forms.Label lblSslCertificateWarning;
        private System.Windows.Forms.Label label3;
        private AutomateControls.Textboxes.StyledTextBox txtBindTo;
        private System.Windows.Forms.Label lblBindToLabel;
        private AutomateControls.StyledNumericUpDown numPort;
        private AutomateControls.Textboxes.StyledTextBox txtConnectionModeHelp;
        private System.Windows.Forms.Label lblConnectionMode;
        private System.Windows.Forms.ComboBox cmbConnectionMode;
        private System.Windows.Forms.ComboBox cmbConnection;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private AutomateControls.Textboxes.StyledTextBox txtName;
        private System.Windows.Forms.TabPage tpLogging;
        private System.Windows.Forms.CheckBox chkLogTraffic;
        private System.Windows.Forms.CheckBox chkVerbose;
        private System.Windows.Forms.CheckBox chkServiceStatusEventLogging;
        private System.Windows.Forms.TabPage tpServerServices;
        private System.Windows.Forms.Label lblWindowsServices;
        private System.Windows.Forms.DataGridView dgvWindowsServices;
        private System.Windows.Forms.PictureBox pbGenericWindowsServiceWarning;
        private System.Windows.Forms.Label lblGenericWindowsServiceWarning;
        private System.Windows.Forms.PictureBox pbWindowsServiceWarning;
        private AutomateControls.BulletedLinkLabel llManagePermissions;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServiceName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFullPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colState;
        private System.Windows.Forms.DataGridViewImageColumn colPermission;
        private AutomateControls.Textboxes.StyledTextBox txtWindowsServiceWarning;
        private AutomateControls.Buttons.StandardStyledButton btnCreateService;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TabPage tpDataGateways;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.GroupBox fraDGUser;
        private AutomateControls.StyledRadioButton radRunAsSpecificUser;
        private AutomateControls.StyledRadioButton radRunAsServerUser;
        private System.Windows.Forms.GroupBox grpDataGatewaysUser;
        private AutomateControls.SecurePasswordTextBox txtDGPassword;
        private System.Windows.Forms.Label lblDGDomain;
        private AutomateControls.Textboxes.StyledTextBox txtDGDomain;
        private System.Windows.Forms.Label lblDGPassword;
        private System.Windows.Forms.Label lblDGUsername;
        private AutomateControls.Textboxes.StyledTextBox txtDGUsername;
        private System.Windows.Forms.CheckBox chkEnableDataGatewayProcess;
        private System.Windows.Forms.Label lblDGPort;
        private AutomateControls.Textboxes.StyledTextBox txtDGPort;
        private System.Windows.Forms.CheckBox chkEnabledDataGatewaysTrace;
        private System.Windows.Forms.CheckBox chkLogDataGatewaysConsole;
        private System.Windows.Forms.CheckBox chkOrdered;
        private System.Windows.Forms.Label lblOrdered;
        private System.Windows.Forms.TabPage tpAuthenticationServerIntegration;
        private System.Windows.Forms.Label lblEnvironmentIdentifier;
        private AutomateControls.Textboxes.StyledTextBox txtEnvironmentIdentifier;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblUsername;
        private AutomateControls.Textboxes.StyledTextBox txtBrokerUsername;
        private System.Windows.Forms.Label lblBrokerAddress;
        private AutomateControls.Textboxes.StyledTextBox txtBrokerAddress;
        private System.Windows.Forms.GroupBox gbBrokerSettings;
        private AutomateControls.SecurePasswordTextBox txtBrokerPassword;
		private System.Windows.Forms.TabPage tpASCR;
		private ctlASCRSettings ctlASCRSettings;
	}
}
