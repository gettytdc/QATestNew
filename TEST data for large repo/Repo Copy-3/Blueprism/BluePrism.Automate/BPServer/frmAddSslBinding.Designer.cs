namespace BluePrism.BPServer
{
    partial class frmAddSslBinding
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddSslBinding));
            this.btnCancel = new AutomateControls.Buttons.StandardStyledButton();
            this.btnOK = new AutomateControls.Buttons.StandardStyledButton();
            this.rbBindToAnyIp = new AutomateControls.StyledRadioButton();
            this.rbBindToSpecificIp = new AutomateControls.StyledRadioButton();
            this.rbBindToHostName = new AutomateControls.StyledRadioButton();
            this.gbAddress = new System.Windows.Forms.GroupBox();
            this.lblHostNamePortInfo = new System.Windows.Forms.Label();
            this.txtHostName = new AutomateControls.Textboxes.StyledTextBox();
            this.lblIpAddressPortInfo = new System.Windows.Forms.Label();
            this.txtIpAddress = new AutomateControls.Textboxes.StyledTextBox();
            this.pbSslSniNotEnabledWarningIcon = new System.Windows.Forms.PictureBox();
            this.lblSslSniNotEnabledWarning = new System.Windows.Forms.Label();
            this.gbStore = new System.Windows.Forms.GroupBox();
            this.cbStoreName = new System.Windows.Forms.ComboBox();
            this.gbAddress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSslSniNotEnabledWarningIcon)).BeginInit();
            this.gbStore.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // rbBindToAnyIp
            // 
            resources.ApplyResources(this.rbBindToAnyIp, "rbBindToAnyIp");
            this.rbBindToAnyIp.Name = "rbBindToAnyIp";
            this.rbBindToAnyIp.TabStop = true;
            this.rbBindToAnyIp.UseVisualStyleBackColor = true;
            // 
            // rbBindToSpecificIp
            // 
            resources.ApplyResources(this.rbBindToSpecificIp, "rbBindToSpecificIp");
            this.rbBindToSpecificIp.Name = "rbBindToSpecificIp";
            this.rbBindToSpecificIp.TabStop = true;
            this.rbBindToSpecificIp.UseVisualStyleBackColor = true;
            // 
            // rbBindToHostName
            // 
            resources.ApplyResources(this.rbBindToHostName, "rbBindToHostName");
            this.rbBindToHostName.Name = "rbBindToHostName";
            this.rbBindToHostName.TabStop = true;
            this.rbBindToHostName.UseVisualStyleBackColor = true;
            // 
            // gbAddress
            // 
            this.gbAddress.Controls.Add(this.lblHostNamePortInfo);
            this.gbAddress.Controls.Add(this.txtHostName);
            this.gbAddress.Controls.Add(this.lblIpAddressPortInfo);
            this.gbAddress.Controls.Add(this.txtIpAddress);
            this.gbAddress.Controls.Add(this.rbBindToAnyIp);
            this.gbAddress.Controls.Add(this.rbBindToHostName);
            this.gbAddress.Controls.Add(this.rbBindToSpecificIp);
            resources.ApplyResources(this.gbAddress, "gbAddress");
            this.gbAddress.Name = "gbAddress";
            this.gbAddress.TabStop = false;
            // 
            // lblHostNamePortInfo
            // 
            resources.ApplyResources(this.lblHostNamePortInfo, "lblHostNamePortInfo");
            this.lblHostNamePortInfo.Name = "lblHostNamePortInfo";
            // 
            // txtHostName
            // 
            resources.ApplyResources(this.txtHostName, "txtHostName");
            this.txtHostName.Name = "txtHostName";
            this.txtHostName.ReadOnly = true;
            // 
            // lblIpAddressPortInfo
            // 
            resources.ApplyResources(this.lblIpAddressPortInfo, "lblIpAddressPortInfo");
            this.lblIpAddressPortInfo.Name = "lblIpAddressPortInfo";
            // 
            // txtIpAddress
            // 
            resources.ApplyResources(this.txtIpAddress, "txtIpAddress");
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.ReadOnly = true;
            // 
            // pbSslSniNotEnabledWarningIcon
            // 
            resources.ApplyResources(this.pbSslSniNotEnabledWarningIcon, "pbSslSniNotEnabledWarningIcon");
            this.pbSslSniNotEnabledWarningIcon.Name = "pbSslSniNotEnabledWarningIcon";
            this.pbSslSniNotEnabledWarningIcon.TabStop = false;
            // 
            // lblSslSniNotEnabledWarning
            // 
            resources.ApplyResources(this.lblSslSniNotEnabledWarning, "lblSslSniNotEnabledWarning");
            this.lblSslSniNotEnabledWarning.Name = "lblSslSniNotEnabledWarning";
            // 
            // gbStore
            // 
            this.gbStore.Controls.Add(this.cbStoreName);
            resources.ApplyResources(this.gbStore, "gbStore");
            this.gbStore.Name = "gbStore";
            this.gbStore.TabStop = false;
            // 
            // cbStoreName
            // 
            this.cbStoreName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbStoreName.FormattingEnabled = true;
            resources.ApplyResources(this.cbStoreName, "cbStoreName");
            this.cbStoreName.Name = "cbStoreName";
            // 
            // frmAddSslBinding
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.gbStore);
            this.Controls.Add(this.pbSslSniNotEnabledWarningIcon);
            this.Controls.Add(this.lblSslSniNotEnabledWarning);
            this.Controls.Add(this.gbAddress);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAddSslBinding";
            this.gbAddress.ResumeLayout(false);
            this.gbAddress.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSslSniNotEnabledWarningIcon)).EndInit();
            this.gbStore.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AutomateControls.Buttons.StandardStyledButton btnCancel;
        private AutomateControls.Buttons.StandardStyledButton btnOK;
        private AutomateControls.StyledRadioButton rbBindToAnyIp;
        private AutomateControls.StyledRadioButton rbBindToSpecificIp;
        private AutomateControls.StyledRadioButton rbBindToHostName;
        private System.Windows.Forms.GroupBox gbAddress;
        private System.Windows.Forms.Label lblHostNamePortInfo;
        private AutomateControls.Textboxes.StyledTextBox txtHostName;
        private System.Windows.Forms.Label lblIpAddressPortInfo;
        private AutomateControls.Textboxes.StyledTextBox txtIpAddress;
        private System.Windows.Forms.PictureBox pbSslSniNotEnabledWarningIcon;
        private System.Windows.Forms.Label lblSslSniNotEnabledWarning;
        private System.Windows.Forms.GroupBox gbStore;
        private System.Windows.Forms.ComboBox cbStoreName;
    }
}