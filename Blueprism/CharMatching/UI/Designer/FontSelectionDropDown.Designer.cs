namespace BluePrism.CharMatching.UI.Designer
{
    partial class FontSelectionDropDown
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontSelectionDropDown));
            this.lbFonts = new System.Windows.Forms.ListBox();
            this.btnNewFont = new AutomateControls.SplitButton();
            this.mnuNewFont = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuIdentifySysFont = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCreateEmptyFont = new System.Windows.Forms.ToolStripMenuItem();
            this._worker = new System.ComponentModel.BackgroundWorker();
            this.btnClear = new AutomateControls.Buttons.StandardStyledButton();
            this.mnuNewFont.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbFonts
            // 
            resources.ApplyResources(this.lbFonts, "lbFonts");
            this.lbFonts.Name = "lbFonts";
            this.lbFonts.Sorted = true;
            // 
            // btnNewFont
            // 
            resources.ApplyResources(this.btnNewFont, "btnNewFont");
            this.btnNewFont.ContextMenuStrip = this.mnuNewFont;
            this.btnNewFont.Name = "btnNewFont";
            this.btnNewFont.SplitMenuStrip = this.mnuNewFont;
            // 
            // mnuNewFont
            // 
            this.mnuNewFont.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuIdentifySysFont,
            this.mnuCreateEmptyFont});
            this.mnuNewFont.Name = "mnuNewFont";
            resources.ApplyResources(this.mnuNewFont, "mnuNewFont");
            // 
            // mnuIdentifySysFont
            // 
            this.mnuIdentifySysFont.Name = "mnuIdentifySysFont";
            resources.ApplyResources(this.mnuIdentifySysFont, "mnuIdentifySysFont");
            this.mnuIdentifySysFont.Click += new System.EventHandler(this.HandleIdentifySystemFont);
            // 
            // mnuCreateEmptyFont
            // 
            this.mnuCreateEmptyFont.Name = "mnuCreateEmptyFont";
            resources.ApplyResources(this.mnuCreateEmptyFont, "mnuCreateEmptyFont");
            this.mnuCreateEmptyFont.Click += new System.EventHandler(this.HandleCreateEmptyFont);
            // 
            // _worker
            // 
            this._worker.WorkerReportsProgress = true;
            this._worker.WorkerSupportsCancellation = true;
            // 
            // btnClear
            // 
            resources.ApplyResources(this.btnClear, "btnClear");
            this.btnClear.Name = "btnClear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.HandleClearClicked);
            // 
            // FontSelectionDropDown
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnNewFont);
            this.Controls.Add(this.lbFonts);
            resources.ApplyResources(this, "$this");
            this.Name = "FontSelectionDropDown";
            this.mnuNewFont.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbFonts;
        private AutomateControls.SplitButton btnNewFont;
        private System.Windows.Forms.ContextMenuStrip mnuNewFont;
        private System.Windows.Forms.ToolStripMenuItem mnuIdentifySysFont;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateEmptyFont;
        private System.ComponentModel.BackgroundWorker _worker;
        private AutomateControls.Buttons.StandardStyledButton btnClear;
    }
}
