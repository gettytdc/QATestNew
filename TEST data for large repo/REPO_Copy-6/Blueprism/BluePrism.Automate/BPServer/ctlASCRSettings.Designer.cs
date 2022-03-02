namespace BluePrism.BPServer
{
    partial class ctlASCRSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctlASCRSettings));
            this.lblHostname = new System.Windows.Forms.Label();
            this.txtHostName = new AutomateControls.Textboxes.StyledTextBox();
            this.lblPortNo = new System.Windows.Forms.Label();
            this.numPort = new AutomateControls.StyledNumericUpDown();
            this.gbBinding = new System.Windows.Forms.GroupBox();
            this.gbSecurity = new System.Windows.Forms.GroupBox();
            this.clientStoreName = new System.Windows.Forms.Label();
            this.serverStoreName = new System.Windows.Forms.Label();
            this.clientCertStoreName = new System.Windows.Forms.ComboBox();
            this.serverCertStoreName = new System.Windows.Forms.ComboBox();
            this.lblCertName = new System.Windows.Forms.Label();
            this.txtCertName = new AutomateControls.Textboxes.StyledTextBox();
            this.lblClientCertName = new System.Windows.Forms.Label();
            this.txtClientCertName = new AutomateControls.Textboxes.StyledTextBox();
            this.lblMode = new System.Windows.Forms.Label();
            this.radCertificate = new AutomateControls.StyledRadioButton();
            this.radWindows = new AutomateControls.StyledRadioButton();
            this.radInsecure = new AutomateControls.StyledRadioButton();
            this.gbProtocol = new System.Windows.Forms.GroupBox();
            this.pbGRPCWarningIcon = new System.Windows.Forms.PictureBox();
            this.lblGRPCWarning = new System.Windows.Forms.Label();
            this.lblCallbackProtocol = new System.Windows.Forms.Label();
            this.radGRPC = new AutomateControls.StyledRadioButton();
            this.radWCF = new AutomateControls.StyledRadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.gbBinding.SuspendLayout();
            this.gbSecurity.SuspendLayout();
            this.gbProtocol.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGRPCWarningIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHostname
            // 
            resources.ApplyResources(this.lblHostname, "lblHostname");
            this.lblHostname.Name = "lblHostname";
            // 
            // txtHostName
            // 
            resources.ApplyResources(this.txtHostName, "txtHostName");
            this.txtHostName.BorderColor = System.Drawing.Color.Empty;
            this.txtHostName.Name = "txtHostName";
            // 
            // lblPortNo
            // 
            resources.ApplyResources(this.lblPortNo, "lblPortNo");
            this.lblPortNo.Name = "lblPortNo";
            // 
            // numPort
            // 
            resources.ApplyResources(this.numPort, "numPort");
            this.numPort.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            // 
            // gbBinding
            // 
            resources.ApplyResources(this.gbBinding, "gbBinding");
            this.gbBinding.Controls.Add(this.lblHostname);
            this.gbBinding.Controls.Add(this.txtHostName);
            this.gbBinding.Controls.Add(this.lblPortNo);
            this.gbBinding.Controls.Add(this.numPort);
            this.gbBinding.Name = "gbBinding";
            this.gbBinding.TabStop = false;
            // 
            // gbSecurity
            // 
            resources.ApplyResources(this.gbSecurity, "gbSecurity");
            this.gbSecurity.Controls.Add(this.clientStoreName);
            this.gbSecurity.Controls.Add(this.serverStoreName);
            this.gbSecurity.Controls.Add(this.clientCertStoreName);
            this.gbSecurity.Controls.Add(this.serverCertStoreName);
            this.gbSecurity.Controls.Add(this.lblCertName);
            this.gbSecurity.Controls.Add(this.txtCertName);
            this.gbSecurity.Controls.Add(this.lblClientCertName);
            this.gbSecurity.Controls.Add(this.txtClientCertName);
            this.gbSecurity.Controls.Add(this.lblMode);
            this.gbSecurity.Controls.Add(this.radCertificate);
            this.gbSecurity.Controls.Add(this.radWindows);
            this.gbSecurity.Controls.Add(this.radInsecure);
            this.gbSecurity.Name = "gbSecurity";
            this.gbSecurity.TabStop = false;
            // 
            // clientStoreName
            // 
            resources.ApplyResources(this.clientStoreName, "clientStoreName");
            this.clientStoreName.Name = "clientStoreName";
            // 
            // serverStoreName
            // 
            resources.ApplyResources(this.serverStoreName, "serverStoreName");
            this.serverStoreName.Name = "serverStoreName";
            // 
            // clientCertStoreName
            // 
            this.clientCertStoreName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.clientCertStoreName.FormattingEnabled = true;
            resources.ApplyResources(this.clientCertStoreName, "clientCertStoreName");
            this.clientCertStoreName.Name = "clientCertStoreName";
            // 
            // serverCertStoreName
            // 
            this.serverCertStoreName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.serverCertStoreName.FormattingEnabled = true;
            resources.ApplyResources(this.serverCertStoreName, "serverCertStoreName");
            this.serverCertStoreName.Name = "serverCertStoreName";
            // 
            // lblCertName
            // 
            resources.ApplyResources(this.lblCertName, "lblCertName");
            this.lblCertName.Name = "lblCertName";
            // 
            // txtCertName
            // 
            this.txtCertName.BorderColor = System.Drawing.Color.Empty;
            resources.ApplyResources(this.txtCertName, "txtCertName");
            this.txtCertName.Name = "txtCertName";
            // 
            // lblClientCertName
            // 
            resources.ApplyResources(this.lblClientCertName, "lblClientCertName");
            this.lblClientCertName.Name = "lblClientCertName";
            // 
            // txtClientCertName
            // 
            this.txtClientCertName.BorderColor = System.Drawing.Color.Empty;
            resources.ApplyResources(this.txtClientCertName, "txtClientCertName");
            this.txtClientCertName.Name = "txtClientCertName";
            // 
            // lblMode
            // 
            resources.ApplyResources(this.lblMode, "lblMode");
            this.lblMode.Name = "lblMode";
            // 
            // radCertificate
            // 
            this.radCertificate.ButtonHeight = 25;
            this.radCertificate.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.radCertificate.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.radCertificate.FocusDiameter = 16;
            this.radCertificate.FocusThickness = 3;
            this.radCertificate.FocusYLocation = 9;
            this.radCertificate.ForceFocus = true;
            this.radCertificate.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.radCertificate.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
            resources.ApplyResources(this.radCertificate, "radCertificate");
            this.radCertificate.MouseLeaveColor = System.Drawing.Color.White;
            this.radCertificate.Name = "radCertificate";
            this.radCertificate.RadioButtonDiameter = 12;
            this.radCertificate.RadioButtonThickness = 2;
            this.radCertificate.RadioYLocation = 7;
            this.radCertificate.StringYLocation = 3;
            this.radCertificate.TabStop = true;
            this.radCertificate.TextColor = System.Drawing.Color.Black;
            this.radCertificate.UseVisualStyleBackColor = true;
            // 
            // radWindows
            // 
            this.radWindows.ButtonHeight = 25;
            this.radWindows.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.radWindows.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.radWindows.FocusDiameter = 16;
            this.radWindows.FocusThickness = 3;
            this.radWindows.FocusYLocation = 9;
            this.radWindows.ForceFocus = true;
            this.radWindows.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.radWindows.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
            resources.ApplyResources(this.radWindows, "radWindows");
            this.radWindows.MouseLeaveColor = System.Drawing.Color.White;
            this.radWindows.Name = "radWindows";
            this.radWindows.RadioButtonDiameter = 12;
            this.radWindows.RadioButtonThickness = 2;
            this.radWindows.RadioYLocation = 7;
            this.radWindows.StringYLocation = 3;
            this.radWindows.TabStop = true;
            this.radWindows.TextColor = System.Drawing.Color.Black;
            this.radWindows.UseVisualStyleBackColor = true;
            // 
            // radInsecure
            // 
            this.radInsecure.ButtonHeight = 25;
            this.radInsecure.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.radInsecure.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.radInsecure.FocusDiameter = 16;
            this.radInsecure.FocusThickness = 3;
            this.radInsecure.FocusYLocation = 9;
            this.radInsecure.ForceFocus = true;
            this.radInsecure.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.radInsecure.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
            resources.ApplyResources(this.radInsecure, "radInsecure");
            this.radInsecure.MouseLeaveColor = System.Drawing.Color.White;
            this.radInsecure.Name = "radInsecure";
            this.radInsecure.RadioButtonDiameter = 12;
            this.radInsecure.RadioButtonThickness = 2;
            this.radInsecure.RadioYLocation = 7;
            this.radInsecure.StringYLocation = 3;
            this.radInsecure.TabStop = true;
            this.radInsecure.TextColor = System.Drawing.Color.Black;
            this.radInsecure.UseVisualStyleBackColor = true;
            // 
            // gbProtocol
            // 
            resources.ApplyResources(this.gbProtocol, "gbProtocol");
            this.gbProtocol.Controls.Add(this.pbGRPCWarningIcon);
            this.gbProtocol.Controls.Add(this.lblGRPCWarning);
            this.gbProtocol.Controls.Add(this.lblCallbackProtocol);
            this.gbProtocol.Controls.Add(this.radGRPC);
            this.gbProtocol.Controls.Add(this.radWCF);
            this.gbProtocol.Name = "gbProtocol";
            this.gbProtocol.TabStop = false;
            // 
            // pbGRPCWarningIcon
            // 
            resources.ApplyResources(this.pbGRPCWarningIcon, "pbGRPCWarningIcon");
            this.pbGRPCWarningIcon.Name = "pbGRPCWarningIcon";
            this.pbGRPCWarningIcon.TabStop = false;
            // 
            // lblGRPCWarning
            // 
            resources.ApplyResources(this.lblGRPCWarning, "lblGRPCWarning");
            this.lblGRPCWarning.Name = "lblGRPCWarning";
            // 
            // lblCallbackProtocol
            // 
            resources.ApplyResources(this.lblCallbackProtocol, "lblCallbackProtocol");
            this.lblCallbackProtocol.Name = "lblCallbackProtocol";
            // 
            // radGRPC
            // 
            this.radGRPC.ButtonHeight = 25;
            this.radGRPC.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.radGRPC.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.radGRPC.FocusDiameter = 16;
            this.radGRPC.FocusThickness = 3;
            this.radGRPC.FocusYLocation = 9;
            this.radGRPC.ForceFocus = true;
            this.radGRPC.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.radGRPC.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
            resources.ApplyResources(this.radGRPC, "radGRPC");
            this.radGRPC.MouseLeaveColor = System.Drawing.Color.White;
            this.radGRPC.Name = "radGRPC";
            this.radGRPC.RadioButtonDiameter = 12;
            this.radGRPC.RadioButtonThickness = 2;
            this.radGRPC.RadioYLocation = 7;
            this.radGRPC.StringYLocation = 3;
            this.radGRPC.TabStop = true;
            this.radGRPC.TextColor = System.Drawing.Color.Black;
            this.radGRPC.UseVisualStyleBackColor = true;
            // 
            // radWCF
            // 
            this.radWCF.ButtonHeight = 25;
            this.radWCF.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.radWCF.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(195)))), ((int)(((byte)(0)))));
            this.radWCF.FocusDiameter = 16;
            this.radWCF.FocusThickness = 3;
            this.radWCF.FocusYLocation = 9;
            this.radWCF.ForceFocus = true;
            this.radWCF.ForeGroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.radWCF.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(201)))), ((int)(((byte)(216)))));
            resources.ApplyResources(this.radWCF, "radWCF");
            this.radWCF.MouseLeaveColor = System.Drawing.Color.White;
            this.radWCF.Name = "radWCF";
            this.radWCF.RadioButtonDiameter = 12;
            this.radWCF.RadioButtonThickness = 2;
            this.radWCF.RadioYLocation = 7;
            this.radWCF.StringYLocation = 3;
            this.radWCF.TabStop = true;
            this.radWCF.TextColor = System.Drawing.Color.Black;
            this.radWCF.UseVisualStyleBackColor = true;
            // 
            // ctlASCRSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbProtocol);
            this.Controls.Add(this.gbBinding);
            this.Controls.Add(this.gbSecurity);
            this.Name = "ctlASCRSettings";
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.gbBinding.ResumeLayout(false);
            this.gbBinding.PerformLayout();
            this.gbSecurity.ResumeLayout(false);
            this.gbSecurity.PerformLayout();
            this.gbProtocol.ResumeLayout(false);
            this.gbProtocol.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGRPCWarningIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblHostname;
        private AutomateControls.Textboxes.StyledTextBox txtHostName;
        private System.Windows.Forms.Label lblPortNo;
        private AutomateControls.StyledNumericUpDown numPort;
        private System.Windows.Forms.GroupBox gbBinding;
        private System.Windows.Forms.GroupBox gbSecurity;
        private System.Windows.Forms.Label lblMode;
        private AutomateControls.StyledRadioButton radCertificate;
        private AutomateControls.StyledRadioButton radWindows;
        private AutomateControls.StyledRadioButton radInsecure;
        private System.Windows.Forms.Label lblClientCertName;
        private System.Windows.Forms.Label lblCertName;
        private AutomateControls.Textboxes.StyledTextBox txtCertName;
        private AutomateControls.Textboxes.StyledTextBox txtClientCertName;
        private System.Windows.Forms.GroupBox gbProtocol;
        private System.Windows.Forms.Label lblCallbackProtocol;
        private AutomateControls.StyledRadioButton radGRPC;
        private AutomateControls.StyledRadioButton radWCF;
        private System.Windows.Forms.PictureBox pbGRPCWarningIcon;
        private System.Windows.Forms.Label lblGRPCWarning;
        private System.Windows.Forms.ComboBox clientCertStoreName;
        private System.Windows.Forms.ComboBox serverCertStoreName;
        private System.Windows.Forms.Label clientStoreName;
        private System.Windows.Forms.Label serverStoreName;
    }
}
