namespace BluePrism.BPServer
{
    partial class frmManageServerConfigEncryption
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmManageServerConfigEncryption));
            this.txtThumbprint = new AutomateControls.Textboxes.StyledTextBox();
            this.labelOwnCertificate = new System.Windows.Forms.Label();
            this.rdoOwnCertificate = new AutomateControls.StyledRadioButton();
            this.rdoBPEncryption = new AutomateControls.StyledRadioButton();
            this.headerLabel = new System.Windows.Forms.Label();
            this.btnOk = new AutomateControls.Buttons.StandardStyledButton(this.components);
            this.lblCertificateWarning = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtThumbprint
            // 
            this.txtThumbprint.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.txtThumbprint, "txtThumbprint");
            this.txtThumbprint.Name = "txtThumbprint";
            this.txtThumbprint.Click += new System.EventHandler(this.TxtThumbprint_Click);
            // 
            // labelOwnCertificate
            // 
            resources.ApplyResources(this.labelOwnCertificate, "labelOwnCertificate");
            this.labelOwnCertificate.Name = "labelOwnCertificate";
            // 
            // rdoOwnCertificate
            // 
            this.rdoOwnCertificate.ButtonHeight = 21;
            this.rdoOwnCertificate.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.rdoOwnCertificate.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.rdoOwnCertificate.FocusDiameter = 16;
            this.rdoOwnCertificate.FocusThickness = 3;
            this.rdoOwnCertificate.FocusYLocation = 9;
            resources.ApplyResources(this.rdoOwnCertificate, "rdoOwnCertificate");
            this.rdoOwnCertificate.ForceFocus = false;
            this.rdoOwnCertificate.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.rdoOwnCertificate.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
            this.rdoOwnCertificate.MouseLeaveColor = System.Drawing.Color.White;
            this.rdoOwnCertificate.Name = "rdoOwnCertificate";
            this.rdoOwnCertificate.RadioButtonDiameter = 12;
            this.rdoOwnCertificate.RadioButtonThickness = 2;
            this.rdoOwnCertificate.RadioYLocation = 7;
            this.rdoOwnCertificate.StringYLocation = 3;
            this.rdoOwnCertificate.TextColor = System.Drawing.Color.Black;
            this.rdoOwnCertificate.UseVisualStyleBackColor = true;
            this.rdoOwnCertificate.CheckedChanged += new System.EventHandler(this.ChkboxOwnCertificate_CheckedChanged);
            this.rdoOwnCertificate.Leave += new System.EventHandler(this.RdoOwnCertificate_Leave);
            this.rdoOwnCertificate.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RdoOwnCertificate_MouseClick);
            // 
            // rdoBPEncryption
            // 
            this.rdoBPEncryption.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.rdoBPEncryption.ButtonHeight = 21;
            this.rdoBPEncryption.Checked = true;
            this.rdoBPEncryption.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.rdoBPEncryption.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.rdoBPEncryption.FocusDiameter = 16;
            this.rdoBPEncryption.FocusThickness = 3;
            this.rdoBPEncryption.FocusYLocation = 9;
            resources.ApplyResources(this.rdoBPEncryption, "rdoBPEncryption");
            this.rdoBPEncryption.ForceFocus = false;
            this.rdoBPEncryption.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rdoBPEncryption.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.rdoBPEncryption.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
            this.rdoBPEncryption.MouseLeaveColor = System.Drawing.Color.White;
            this.rdoBPEncryption.Name = "rdoBPEncryption";
            this.rdoBPEncryption.RadioButtonDiameter = 12;
            this.rdoBPEncryption.RadioButtonThickness = 2;
            this.rdoBPEncryption.RadioYLocation = 7;
            this.rdoBPEncryption.StringYLocation = 3;
            this.rdoBPEncryption.TabStop = true;
            this.rdoBPEncryption.TextColor = System.Drawing.Color.Black;
            this.rdoBPEncryption.UseVisualStyleBackColor = false;
            this.rdoBPEncryption.CheckedChanged += new System.EventHandler(this.ChkboxBPEncryption_CheckedChanged);
            this.rdoBPEncryption.Leave += new System.EventHandler(this.RdoBPEncryption_Leave);
            this.rdoBPEncryption.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RdoBPEncryption_MouseClick);
            // 
            // headerLabel
            // 
            resources.ApplyResources(this.headerLabel, "headerLabel");
            this.headerLabel.Name = "headerLabel";
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.BackColor = System.Drawing.Color.White;
            this.btnOk.FlatAppearance.BorderColor = System.Drawing.Color.Blue;
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // lblCertificateWarning
            // 
            resources.ApplyResources(this.lblCertificateWarning, "lblCertificateWarning");
            this.lblCertificateWarning.Name = "lblCertificateWarning";
            // 
            // frmManageServerConfigEncryption
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.lblCertificateWarning);
            this.Controls.Add(this.labelOwnCertificate);
            this.Controls.Add(this.txtThumbprint);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.headerLabel);
            this.Controls.Add(this.rdoBPEncryption);
            this.Controls.Add(this.rdoOwnCertificate);
            this.Name = "frmManageServerConfigEncryption";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmManageServerConfigEncryption_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private AutomateControls.StyledRadioButton rdoBPEncryption;
        private AutomateControls.StyledRadioButton rdoOwnCertificate;
        private AutomateControls.Textboxes.StyledTextBox txtThumbprint;
        private System.Windows.Forms.Label labelOwnCertificate;
        private System.Windows.Forms.Label headerLabel;
        private AutomateControls.Buttons.StandardStyledButton btnOk;
        private System.Windows.Forms.Label lblCertificateWarning;
    }
}