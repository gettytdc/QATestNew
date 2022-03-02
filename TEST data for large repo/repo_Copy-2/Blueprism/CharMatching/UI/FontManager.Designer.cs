namespace BluePrism.CharMatching.UI
{
    partial class FontManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontManager));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.gridFonts = new System.Windows.Forms.DataGridView();
            this.stripFontMenu = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripSplitButton();
            this.mnuNewFromSysFont = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNewFromScreenshot = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNewFromImport = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExport = new System.Windows.Forms.ToolStripButton();
            this.btnEdit = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.btnReferences = new System.Windows.Forms.ToolStripButton();
            this.ctxMenuNewFont = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fromSystemFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromScreenshotRegionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridFonts)).BeginInit();
            this.stripFontMenu.SuspendLayout();
            this.ctxMenuNewFont.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.gridFonts);
            resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.stripFontMenu);
            resources.ApplyResources(this.toolStripContainer1.TopToolStripPanel, "toolStripContainer1.TopToolStripPanel");
            // 
            // gridFonts
            // 
            this.gridFonts.AllowUserToAddRows = false;
            this.gridFonts.AllowUserToDeleteRows = false;
            this.gridFonts.AllowUserToResizeRows = false;
            this.gridFonts.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.gridFonts.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridFonts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridFonts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName});
            resources.ApplyResources(this.gridFonts, "gridFonts");
            this.gridFonts.GridColor = System.Drawing.SystemColors.Control;
            this.gridFonts.Name = "gridFonts";
            this.gridFonts.ReadOnly = true;
            this.gridFonts.RowHeadersVisible = false;
            this.gridFonts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridFonts.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleFontDoubleClick);
            this.gridFonts.SelectionChanged += new System.EventHandler(this.HandleFontSelectionChanged);
            // 
            // stripFontMenu
            // 
            this.stripFontMenu.AllowMerge = false;
            resources.ApplyResources(this.stripFontMenu, "stripFontMenu");
            this.stripFontMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripFontMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnExport,
            this.btnEdit,
            this.btnDelete,
            this.btnReferences});
            this.stripFontMenu.Name = "stripFontMenu";
            // 
            // btnNew
            // 
            this.btnNew.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuNewFromSysFont,
            this.mnuNewFromScreenshot,
            this.mnuNewFromImport});
            resources.ApplyResources(this.btnNew, "btnNew");
            this.btnNew.Name = "btnNew";
            // 
            // mnuNewFromSysFont
            // 
            this.mnuNewFromSysFont.Image = global::BluePrism.CharMatching.Properties.Resources.font_16x16;
            this.mnuNewFromSysFont.Name = "mnuNewFromSysFont";
            resources.ApplyResources(this.mnuNewFromSysFont, "mnuNewFromSysFont");
            this.mnuNewFromSysFont.Click += new System.EventHandler(this.HandleNewFromSystemFont);
            // 
            // mnuNewFromScreenshot
            // 
            this.mnuNewFromScreenshot.Image = global::BluePrism.CharMatching.Properties.Resources.regionvanilla_16x16;
            this.mnuNewFromScreenshot.Name = "mnuNewFromScreenshot";
            resources.ApplyResources(this.mnuNewFromScreenshot, "mnuNewFromScreenshot");
            this.mnuNewFromScreenshot.Click += new System.EventHandler(this.HandleNewFromRegions);
            // 
            // mnuNewFromImport
            // 
            this.mnuNewFromImport.Image = global::BluePrism.CharMatching.Properties.Resources.xmlfile_16x16;
            this.mnuNewFromImport.Name = "mnuNewFromImport";
            resources.ApplyResources(this.mnuNewFromImport, "mnuNewFromImport");
            this.mnuNewFromImport.Click += new System.EventHandler(this.HandleNewFromImport);
            // 
            // btnExport
            // 
            resources.ApplyResources(this.btnExport, "btnExport");
            this.btnExport.Name = "btnExport";
            this.btnExport.Click += new System.EventHandler(this.HandleExportClick);
            // 
            // btnEdit
            // 
            resources.ApplyResources(this.btnEdit, "btnEdit");
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Click += new System.EventHandler(this.HandleEditClick);
            // 
            // btnDelete
            // 
            resources.ApplyResources(this.btnDelete, "btnDelete");
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Click += new System.EventHandler(this.HandleDeleteClick);
            // 
            // btnReferences
            // 
            resources.ApplyResources(this.btnReferences, "btnReferences");
            this.btnReferences.Name = "btnReferences";
            this.btnReferences.Click += new System.EventHandler(this.HandleReferencesClick);
            // 
            // ctxMenuNewFont
            // 
            this.ctxMenuNewFont.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fromSystemFontToolStripMenuItem,
            this.fromScreenshotRegionsToolStripMenuItem});
            this.ctxMenuNewFont.Name = "ctxMenuNewFont";
            resources.ApplyResources(this.ctxMenuNewFont, "ctxMenuNewFont");
            // 
            // fromSystemFontToolStripMenuItem
            // 
            this.fromSystemFontToolStripMenuItem.Name = "fromSystemFontToolStripMenuItem";
            resources.ApplyResources(this.fromSystemFontToolStripMenuItem, "fromSystemFontToolStripMenuItem");
            // 
            // fromScreenshotRegionsToolStripMenuItem
            // 
            this.fromScreenshotRegionsToolStripMenuItem.Name = "fromScreenshotRegionsToolStripMenuItem";
            resources.ApplyResources(this.fromScreenshotRegionsToolStripMenuItem, "fromScreenshotRegionsToolStripMenuItem");
            // 
            // colName
            // 
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.colName, "colName");
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // FontManager
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "FontManager";
            resources.ApplyResources(this, "$this");
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridFonts)).EndInit();
            this.stripFontMenu.ResumeLayout(false);
            this.stripFontMenu.PerformLayout();
            this.ctxMenuNewFont.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gridFonts;
        private System.Windows.Forms.ContextMenuStrip ctxMenuNewFont;
        private System.Windows.Forms.ToolStripMenuItem fromSystemFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromScreenshotRegionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip stripFontMenu;
        private System.Windows.Forms.ToolStripSplitButton btnNew;
        private System.Windows.Forms.ToolStripMenuItem mnuNewFromSysFont;
        private System.Windows.Forms.ToolStripMenuItem mnuNewFromScreenshot;
        private System.Windows.Forms.ToolStripMenuItem mnuNewFromImport;
        private System.Windows.Forms.ToolStripButton btnExport;
        private System.Windows.Forms.ToolStripButton btnEdit;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripButton btnReferences;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
    }
}
