namespace BluePrism.CharMatching.UI
{
    partial class FontScannerPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontScannerPanel));
            this.label1 = new System.Windows.Forms.Label();
            this.txtExpected = new AutomateControls.Textboxes.StyledTextBox();
            this.tableFonts = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStyle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnGo = new AutomateControls.Buttons.StandardStyledButton();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnCancel = new AutomateControls.Buttons.StandardStyledButton();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lnkSearchDetails = new System.Windows.Forms.LinkLabel();
            this.picbox = new System.Windows.Forms.PictureBox();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tableFonts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picbox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtExpected
            // 
            resources.ApplyResources(this.txtExpected, "txtExpected");
            this.txtExpected.Name = "txtExpected";
            // 
            // tableFonts
            // 
            this.tableFonts.AllowUserToAddRows = false;
            this.tableFonts.AllowUserToDeleteRows = false;
            this.tableFonts.AllowUserToResizeRows = false;
            resources.ApplyResources(this.tableFonts, "tableFonts");
            this.tableFonts.BackgroundColor = System.Drawing.SystemColors.Control;
            this.tableFonts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableFonts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colStyle,
            this.colSize});
            this.tableFonts.MultiSelect = false;
            this.tableFonts.Name = "tableFonts";
            this.tableFonts.ReadOnly = true;
            this.tableFonts.RowHeadersVisible = false;
            this.tableFonts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tableFonts.SelectionChanged += new System.EventHandler(this.HandleFontSelectionChanged);
            // 
            // colName
            // 
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.colName, "colName");
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colStyle
            // 
            this.colStyle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            resources.ApplyResources(this.colStyle, "colStyle");
            this.colStyle.Name = "colStyle";
            this.colStyle.ReadOnly = true;
            // 
            // colSize
            // 
            this.colSize.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            resources.ApplyResources(this.colSize, "colSize");
            this.colSize.Name = "colSize";
            this.colSize.ReadOnly = true;
            // 
            // btnGo
            // 
            resources.ApplyResources(this.btnGo, "btnGo");
            this.btnGo.Name = "btnGo";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.HandleGoPressed);
            // 
            // progress
            // 
            resources.ApplyResources(this.progress, "progress");
            this.progress.Name = "progress";
            // 
            // lblStatus
            // 
            resources.ApplyResources(this.lblStatus, "lblStatus");
            this.lblStatus.Name = "lblStatus";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.HandleCancelClicked);
            // 
            // picPreview
            // 
            resources.ApplyResources(this.picPreview, "picPreview");
            this.picPreview.BackColor = System.Drawing.Color.AliceBlue;
            this.picPreview.Name = "picPreview";
            this.picPreview.TabStop = false;
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
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // lnkSearchDetails
            // 
            resources.ApplyResources(this.lnkSearchDetails, "lnkSearchDetails");
            this.lnkSearchDetails.Name = "lnkSearchDetails";
            this.lnkSearchDetails.TabStop = true;
            this.lnkSearchDetails.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleSearchDetailLinkClick);
            // 
            // picbox
            // 
            resources.ApplyResources(this.picbox, "picbox");
            this.picbox.BackColor = System.Drawing.Color.AliceBlue;
            this.picbox.Name = "picbox";
            this.picbox.TabStop = false;
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // FontScannerPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.label7);
            this.Controls.Add(this.picbox);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.lnkSearchDetails);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.tableFonts);
            this.Controls.Add(this.txtExpected);
            this.Controls.Add(this.label1);
            this.Name = "FontScannerPanel";
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.tableFonts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picbox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private AutomateControls.Textboxes.StyledTextBox txtExpected;
        private System.Windows.Forms.DataGridView tableFonts;
        private AutomateControls.Buttons.StandardStyledButton btnGo;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.Label lblStatus;
        private AutomateControls.Buttons.StandardStyledButton btnCancel;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStyle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSize;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel lnkSearchDetails;
        private System.Windows.Forms.PictureBox picbox;
        private System.Windows.Forms.Label label7;
    }
}
