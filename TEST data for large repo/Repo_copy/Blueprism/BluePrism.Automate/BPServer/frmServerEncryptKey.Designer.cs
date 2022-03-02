namespace BluePrism.BPServer
{
    partial class frmServerEncryptKey
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmServerEncryptKey));
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new AutomateControls.Textboxes.StyledTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbAlgorithm = new AutomateControls.StyledComboBox();
            this.txtKey = new AutomateControls.Textboxes.StyledTextBox();
            this.btnOK = new AutomateControls.Buttons.StandardStyledButton();
            this.btnCancel = new AutomateControls.Buttons.StandardStyledButton();
            this.chkShowKey = new System.Windows.Forms.CheckBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.lblRetiredWarning = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtName
            // 
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.Name = "txtName";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cmbAlgorithm
            // 
            this.cmbAlgorithm.Checkable = false;
            this.cmbAlgorithm.FormattingEnabled = true;
            resources.ApplyResources(this.cmbAlgorithm, "cmbAlgorithm");
            this.cmbAlgorithm.Name = "cmbAlgorithm";
            this.cmbAlgorithm.SelectedIndexChanged += new System.EventHandler(this.cmbAlgorithm_SelectedIndexChanged);
            this.cmbAlgorithm.LostFocus += new System.EventHandler(this.cmbAlgorithm_LostFocus);
            // 
            // txtKey
            // 
            resources.ApplyResources(this.txtKey, "txtKey");
            this.txtKey.Name = "txtKey";
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.HandleOK);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.HandleCancel);
            // 
            // chkShowKey
            // 
            resources.ApplyResources(this.chkShowKey, "chkShowKey");
            this.chkShowKey.Name = "chkShowKey";
            this.chkShowKey.UseVisualStyleBackColor = true;
            this.chkShowKey.CheckedChanged += new System.EventHandler(this.HandleToggleKey);
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleGenerateKey);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label4.Name = "label4";
            // 
            // lblRetiredWarning
            // 
            resources.ApplyResources(this.lblRetiredWarning, "lblRetiredWarning");
            this.lblRetiredWarning.ForeColor = System.Drawing.Color.Red;
            this.lblRetiredWarning.Name = "lblRetiredWarning";
            // 
            // frmServerEncryptKey
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.lblRetiredWarning);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.chkShowKey);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.cmbAlgorithm);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmServerEncryptKey";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private AutomateControls.Textboxes.StyledTextBox txtName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private AutomateControls.StyledComboBox cmbAlgorithm;
        private AutomateControls.Textboxes.StyledTextBox txtKey;
        private AutomateControls.Buttons.StandardStyledButton btnOK;
        private AutomateControls.Buttons.StandardStyledButton btnCancel;
        private System.Windows.Forms.CheckBox chkShowKey;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblRetiredWarning;
    }
}
