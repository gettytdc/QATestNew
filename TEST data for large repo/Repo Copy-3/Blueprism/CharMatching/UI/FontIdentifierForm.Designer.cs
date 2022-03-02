namespace BluePrism.CharMatching.UI
{
    partial class FontIdentifierForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontIdentifierForm));
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabAutomatic = new System.Windows.Forms.TabPage();
            this.autoIdentifier = new BluePrism.CharMatching.UI.FontScannerPanel();
            this.tabManual = new System.Windows.Forms.TabPage();
            this.manualIdentifier = new BluePrism.CharMatching.UI.FontBrowserPanel();
            this.btnOk = new AutomateControls.Buttons.StandardStyledButton();
            this.btnCancel = new AutomateControls.Buttons.StandardStyledButton();
            this.label1 = new System.Windows.Forms.Label();
            this.lblHelp = new System.Windows.Forms.Label();
            this.cmbRegions = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabs.SuspendLayout();
            this.tabAutomatic.SuspendLayout();
            this.tabManual.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            resources.ApplyResources(this.tabs, "tabs");
            this.tabs.Controls.Add(this.tabAutomatic);
            this.tabs.Controls.Add(this.tabManual);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Selected += new System.Windows.Forms.TabControlEventHandler(this.HandleTabControlSelected);
            // 
            // tabAutomatic
            // 
            this.tabAutomatic.Controls.Add(this.autoIdentifier);
            resources.ApplyResources(this.tabAutomatic, "tabAutomatic");
            this.tabAutomatic.Name = "tabAutomatic";
            this.tabAutomatic.UseVisualStyleBackColor = true;
            // 
            // autoIdentifier
            // 
            resources.ApplyResources(this.autoIdentifier, "autoIdentifier");
            this.autoIdentifier.Name = "autoIdentifier";
            this.autoIdentifier.SpyRegion = null;
            this.autoIdentifier.WorkStarted += new System.EventHandler(this.HandleWorkStarted);
            this.autoIdentifier.WorkFinished += new System.EventHandler(this.HandleWorkFinished);
            // 
            // tabManual
            // 
            this.tabManual.Controls.Add(this.manualIdentifier);
            resources.ApplyResources(this.tabManual, "tabManual");
            this.tabManual.Name = "tabManual";
            this.tabManual.UseVisualStyleBackColor = true;
            // 
            // manualIdentifier
            // 
            resources.ApplyResources(this.manualIdentifier, "manualIdentifier");
            this.manualIdentifier.Name = "manualIdentifier";
            this.manualIdentifier.SpyRegion = null;
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // lblHelp
            // 
            resources.ApplyResources(this.lblHelp, "lblHelp");
            this.lblHelp.Name = "lblHelp";
            // 
            // cmbRegions
            // 
            resources.ApplyResources(this.cmbRegions, "cmbRegions");
            this.cmbRegions.DisplayMember = "Name";
            this.cmbRegions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRegions.FormattingEnabled = true;
            this.cmbRegions.Name = "cmbRegions";
            this.cmbRegions.SelectedIndexChanged += new System.EventHandler(this.cmbRegions_SelectedIndexChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // FontIdentifierForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.cmbRegions);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.tabs);
            this.Name = "FontIdentifierForm";
            this.tabs.ResumeLayout(false);
            this.tabAutomatic.ResumeLayout(false);
            this.tabManual.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabAutomatic;
        private FontScannerPanel autoIdentifier;
        private System.Windows.Forms.TabPage tabManual;
        private FontBrowserPanel manualIdentifier;
        private AutomateControls.Buttons.StandardStyledButton btnOk;
        private AutomateControls.Buttons.StandardStyledButton btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.ComboBox cmbRegions;
        private System.Windows.Forms.Label label2;
    }
}
