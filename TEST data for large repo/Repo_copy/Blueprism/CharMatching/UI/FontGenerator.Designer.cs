namespace BluePrism.CharMatching.UI
{
    partial class FontGenerator
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontGenerator));
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            this.cmbFont = new System.Windows.Forms.ComboBox();
            this.spinnerSize = new AutomateControls.StyledNumericUpDown();
            this.cbBold = new System.Windows.Forms.CheckBox();
            this.cbItal = new System.Windows.Forms.CheckBox();
            this.cbUline = new System.Windows.Forms.CheckBox();
            this.cbStr = new System.Windows.Forms.CheckBox();
            this.panInput = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnGenerate = new AutomateControls.Buttons.StandardStyledButton(this.components);
            this.txtGeneratedName = new AutomateControls.Textboxes.StyledTextBox();
            this.ttFontGen = new System.Windows.Forms.ToolTip(this.components);
            this.progbar = new System.Windows.Forms.ProgressBar();
            this.btnAbort = new AutomateControls.Buttons.StandardStyledButton(this.components);
            this.lblStatus = new System.Windows.Forms.Label();
            this.panWork = new System.Windows.Forms.Panel();
            this.worker = new System.ComponentModel.BackgroundWorker();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spinnerSize)).BeginInit();
            this.panInput.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panWork.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            this.ttFontGen.SetToolTip(label2, resources.GetString("label2.ToolTip"));
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            this.ttFontGen.SetToolTip(label3, resources.GetString("label3.ToolTip"));
            // 
            // label4
            // 
            resources.ApplyResources(label4, "label4");
            label4.Name = "label4";
            this.ttFontGen.SetToolTip(label4, resources.GetString("label4.ToolTip"));
            // 
            // label5
            // 
            resources.ApplyResources(label5, "label5");
            label5.Name = "label5";
            this.ttFontGen.SetToolTip(label5, resources.GetString("label5.ToolTip"));
            // 
            // cmbFont
            // 
            resources.ApplyResources(this.cmbFont, "cmbFont");
            this.cmbFont.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.cmbFont.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbFont.DisplayMember = "Name";
            this.cmbFont.FormattingEnabled = true;
            this.cmbFont.Name = "cmbFont";
            this.ttFontGen.SetToolTip(this.cmbFont, resources.GetString("cmbFont.ToolTip"));
            this.cmbFont.ValueMember = "Name";
            this.cmbFont.SelectedIndexChanged += new System.EventHandler(this.HandleFontFamilySelected);
            this.cmbFont.Validating += new System.ComponentModel.CancelEventHandler(this.HandleFontFamilyValidating);
            // 
            // spinnerSize
            // 
            this.spinnerSize.DecimalPlaces = 1;
            this.spinnerSize.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            resources.ApplyResources(this.spinnerSize, "spinnerSize");
            this.spinnerSize.Maximum = new decimal(new int[] {
            144,
            0,
            0,
            0});
            this.spinnerSize.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.spinnerSize.Name = "spinnerSize";
            this.ttFontGen.SetToolTip(this.spinnerSize, resources.GetString("spinnerSize.ToolTip"));
            this.spinnerSize.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.spinnerSize.ValueChanged += new System.EventHandler(this.HandleFontSizeChanged);
            // 
            // cbBold
            // 
            resources.ApplyResources(this.cbBold, "cbBold");
            this.cbBold.Name = "cbBold";
            this.ttFontGen.SetToolTip(this.cbBold, resources.GetString("cbBold.ToolTip"));
            this.cbBold.UseVisualStyleBackColor = true;
            this.cbBold.CheckedChanged += new System.EventHandler(this.HandleStyleChanged);
            // 
            // cbItal
            // 
            resources.ApplyResources(this.cbItal, "cbItal");
            this.cbItal.Name = "cbItal";
            this.ttFontGen.SetToolTip(this.cbItal, resources.GetString("cbItal.ToolTip"));
            this.cbItal.UseVisualStyleBackColor = true;
            this.cbItal.CheckedChanged += new System.EventHandler(this.HandleStyleChanged);
            // 
            // cbUline
            // 
            resources.ApplyResources(this.cbUline, "cbUline");
            this.cbUline.Name = "cbUline";
            this.ttFontGen.SetToolTip(this.cbUline, resources.GetString("cbUline.ToolTip"));
            this.cbUline.UseVisualStyleBackColor = true;
            this.cbUline.CheckedChanged += new System.EventHandler(this.HandleStyleChanged);
            // 
            // cbStr
            // 
            resources.ApplyResources(this.cbStr, "cbStr");
            this.cbStr.Name = "cbStr";
            this.ttFontGen.SetToolTip(this.cbStr, resources.GetString("cbStr.ToolTip"));
            this.cbStr.UseVisualStyleBackColor = true;
            this.cbStr.CheckedChanged += new System.EventHandler(this.HandleStyleChanged);
            // 
            // panInput
            // 
            this.panInput.Controls.Add(this.flowLayoutPanel1);
            this.panInput.Controls.Add(this.btnGenerate);
            this.panInput.Controls.Add(this.txtGeneratedName);
            this.panInput.Controls.Add(label4);
            this.panInput.Controls.Add(label1);
            this.panInput.Controls.Add(this.cmbFont);
            this.panInput.Controls.Add(label2);
            this.panInput.Controls.Add(label3);
            this.panInput.Controls.Add(this.spinnerSize);
            resources.ApplyResources(this.panInput, "panInput");
            this.panInput.Name = "panInput";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cbBold);
            this.flowLayoutPanel1.Controls.Add(this.cbItal);
            this.flowLayoutPanel1.Controls.Add(this.cbUline);
            this.flowLayoutPanel1.Controls.Add(this.cbStr);
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // btnGenerate
            // 
            resources.ApplyResources(this.btnGenerate, "btnGenerate");
            this.btnGenerate.BackColor = System.Drawing.Color.White;
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.UseVisualStyleBackColor = false;
            this.btnGenerate.Click += new System.EventHandler(this.HandleGenerate);
            // 
            // txtGeneratedName
            // 
            resources.ApplyResources(this.txtGeneratedName, "txtGeneratedName");
            this.txtGeneratedName.BorderColor = System.Drawing.Color.Empty;
            this.txtGeneratedName.Name = "txtGeneratedName";
            this.ttFontGen.SetToolTip(this.txtGeneratedName, resources.GetString("txtGeneratedName.ToolTip"));
            // 
            // progbar
            // 
            resources.ApplyResources(this.progbar, "progbar");
            this.progbar.Name = "progbar";
            // 
            // btnAbort
            // 
            resources.ApplyResources(this.btnAbort, "btnAbort");
            this.btnAbort.BackColor = System.Drawing.Color.White;
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.UseVisualStyleBackColor = false;
            this.btnAbort.Click += new System.EventHandler(this.HandleAbort);
            // 
            // lblStatus
            // 
            resources.ApplyResources(this.lblStatus, "lblStatus");
            this.lblStatus.Name = "lblStatus";
            // 
            // panWork
            // 
            this.panWork.Controls.Add(label5);
            this.panWork.Controls.Add(this.btnAbort);
            this.panWork.Controls.Add(this.progbar);
            resources.ApplyResources(this.panWork, "panWork");
            this.panWork.Name = "panWork";
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.HandleWorkerDoWork);
            this.worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.HandleWorkerProgressChanged);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.HandleWorkerCompleted);
            // 
            // FontGenerator
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.panWork);
            this.Controls.Add(this.panInput);
            this.Controls.Add(this.lblStatus);
            resources.ApplyResources(this, "$this");
            this.Name = "FontGenerator";
            ((System.ComponentModel.ISupportInitialize)(this.spinnerSize)).EndInit();
            this.panInput.ResumeLayout(false);
            this.panInput.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panWork.ResumeLayout(false);
            this.panWork.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbFont;
        private AutomateControls.StyledNumericUpDown spinnerSize;
        private System.Windows.Forms.CheckBox cbBold;
        private System.Windows.Forms.CheckBox cbItal;
        private System.Windows.Forms.CheckBox cbUline;
        private System.Windows.Forms.CheckBox cbStr;
        private System.Windows.Forms.Panel panInput;
        private AutomateControls.Textboxes.StyledTextBox txtGeneratedName;
        private System.Windows.Forms.ToolTip ttFontGen;
        private System.Windows.Forms.ProgressBar progbar;
        private AutomateControls.Buttons.StandardStyledButton btnAbort;
        private System.Windows.Forms.Label lblStatus;
        private AutomateControls.Buttons.StandardStyledButton btnGenerate;
        private System.Windows.Forms.Panel panWork;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
