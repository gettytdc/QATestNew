namespace BluePrism.CharMatching.UI
{
    partial class RegionMapper
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
            System.Windows.Forms.ToolStripSeparator sep1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegionMapper));
            System.Windows.Forms.ToolStripSeparator sep2;
            System.Windows.Forms.ToolStripSeparator sep3;
            this.toolstripCont = new System.Windows.Forms.ToolStripContainer();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.picbox = new AutomateControls.ZoomingScrollablePictureBox();
            this.propsElement = new System.Windows.Forms.PropertyGrid();
            this.ctxMenuPropsGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuResetProperty = new System.Windows.Forms.ToolStripMenuItem();
            this.cmbRegions = new System.Windows.Forms.ComboBox();
            this.ctxToolstrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuShowText = new System.Windows.Forms.ToolStripMenuItem();
            this.toolstrip = new System.Windows.Forms.ToolStrip();
            this.btnOpenImage = new System.Windows.Forms.ToolStripSplitButton();
            this.menuSpy = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnPointer = new System.Windows.Forms.ToolStripButton();
            this.btnRegion = new System.Windows.Forms.ToolStripButton();
            this.btnList = new System.Windows.Forms.ToolStripButton();
            this.btnGrid = new System.Windows.Forms.ToolStripButton();
            this.btnCut = new System.Windows.Forms.ToolStripButton();
            this.btnCopy = new System.Windows.Forms.ToolStripButton();
            this.btnPaste = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.cmbZoom = new System.Windows.Forms.ToolStripComboBox();
            sep1 = new System.Windows.Forms.ToolStripSeparator();
            sep2 = new System.Windows.Forms.ToolStripSeparator();
            sep3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolstripCont.ContentPanel.SuspendLayout();
            this.toolstripCont.TopToolStripPanel.SuspendLayout();
            this.toolstripCont.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.ctxMenuPropsGrid.SuspendLayout();
            this.ctxToolstrip.SuspendLayout();
            this.toolstrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // sep1
            // 
            sep1.Name = "sep1";
            resources.ApplyResources(sep1, "sep1");
            // 
            // toolstripCont
            // 
            // 
            // toolstripCont.ContentPanel
            // 
            this.toolstripCont.ContentPanel.Controls.Add(this.splitMain);
            resources.ApplyResources(this.toolstripCont.ContentPanel, "toolstripCont.ContentPanel");
            resources.ApplyResources(this.toolstripCont, "toolstripCont");
            this.toolstripCont.Name = "toolstripCont";
            // 
            // toolstripCont.TopToolStripPanel
            // 
            this.toolstripCont.TopToolStripPanel.ContextMenuStrip = this.ctxToolstrip;
            this.toolstripCont.TopToolStripPanel.Controls.Add(this.toolstrip);
            // 
            // splitMain
            // 
            resources.ApplyResources(this.splitMain, "splitMain");
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.picbox);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.propsElement);
            this.splitMain.Panel2.Controls.Add(this.cmbRegions);
            // 
            // picbox
            // 
            resources.ApplyResources(this.picbox, "picbox");
            this.picbox.Image = null;
            this.picbox.Name = "picbox";
            this.picbox.ZoomFactor = 1F;
            this.picbox.PicturePaint += new System.Windows.Forms.PaintEventHandler(this.HandlePicturePaint);
            this.picbox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandlePicboxMouseDown);
            this.picbox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandlePicboxMouseMove);
            this.picbox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.HandlePicboxMouseUp);
            // 
            // propsElement
            // 
            this.propsElement.ContextMenuStrip = this.ctxMenuPropsGrid;
            resources.ApplyResources(this.propsElement, "propsElement");
            this.propsElement.LineColor = System.Drawing.SystemColors.ControlDark;
            this.propsElement.Name = "propsElement";
            this.propsElement.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propsElement.ToolbarVisible = false;
            // 
            // ctxMenuPropsGrid
            // 
            this.ctxMenuPropsGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuResetProperty});
            this.ctxMenuPropsGrid.Name = "ctxMenuPropsGrid";
            resources.ApplyResources(this.ctxMenuPropsGrid, "ctxMenuPropsGrid");
            // 
            // menuResetProperty
            // 
            this.menuResetProperty.Name = "menuResetProperty";
            resources.ApplyResources(this.menuResetProperty, "menuResetProperty");
            this.menuResetProperty.Click += new System.EventHandler(this.HandleResetProperty);
            // 
            // cmbRegions
            // 
            resources.ApplyResources(this.cmbRegions, "cmbRegions");
            this.cmbRegions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRegions.FormattingEnabled = true;
            this.cmbRegions.Name = "cmbRegions";
            this.cmbRegions.Sorted = true;
            // 
            // ctxToolstrip
            // 
            this.ctxToolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuShowText});
            this.ctxToolstrip.Name = "ctxToolstrip";
            resources.ApplyResources(this.ctxToolstrip, "ctxToolstrip");
            // 
            // mnuShowText
            // 
            this.mnuShowText.CheckOnClick = true;
            this.mnuShowText.Name = "mnuShowText";
            resources.ApplyResources(this.mnuShowText, "mnuShowText");
            this.mnuShowText.Click += new System.EventHandler(this.HandleShowToobarTextClicked);
            // 
            // toolstrip
            // 
            resources.ApplyResources(this.toolstrip, "toolstrip");
            this.toolstrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenImage,
            this.toolStripSeparator1,
            this.btnPointer,
            sep1,
            this.btnRegion,
            this.btnList,
            this.btnGrid,
            sep2,
            this.btnCut,
            this.btnCopy,
            this.btnPaste,
            this.btnDelete,
            sep3,
            this.cmbZoom});
            this.toolstrip.Name = "toolstrip";
            // 
            // btnOpenImage
            // 
            this.btnOpenImage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuSpy,
            this.menuFile});
            this.btnOpenImage.Image = global::BluePrism.CharMatching.Properties.Resources.palette_16x16;
            resources.ApplyResources(this.btnOpenImage, "btnOpenImage");
            this.btnOpenImage.Name = "btnOpenImage";
            this.btnOpenImage.ButtonClick += new System.EventHandler(this.HandleGetImageFromWindow);
            // 
            // menuSpy
            // 
            this.menuSpy.Image = global::BluePrism.CharMatching.Properties.Resources.mag_16x16;
            this.menuSpy.Name = "menuSpy";
            resources.ApplyResources(this.menuSpy, "menuSpy");
            this.menuSpy.Click += new System.EventHandler(this.HandleGetImageFromWindow);
            // 
            // menuFile
            // 
            this.menuFile.Image = global::BluePrism.CharMatching.Properties.Resources.file_16x16;
            this.menuFile.Name = "menuFile";
            resources.ApplyResources(this.menuFile, "menuFile");
            this.menuFile.Click += new System.EventHandler(this.HandleGetImageFromFile);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // btnPointer
            // 
            this.btnPointer.CheckOnClick = true;
            this.btnPointer.Image = global::BluePrism.CharMatching.Properties.Resources.pointer_16x16;
            resources.ApplyResources(this.btnPointer, "btnPointer");
            this.btnPointer.Name = "btnPointer";
            this.btnPointer.CheckedChanged += new System.EventHandler(this.HandleModeToolbarButton);
            // 
            // btnRegion
            // 
            this.btnRegion.CheckOnClick = true;
            this.btnRegion.Image = global::BluePrism.CharMatching.Properties.Resources.regionvanilla_16x16;
            resources.ApplyResources(this.btnRegion, "btnRegion");
            this.btnRegion.Name = "btnRegion";
            this.btnRegion.CheckedChanged += new System.EventHandler(this.HandleModeToolbarButton);
            // 
            // btnList
            // 
            this.btnList.CheckOnClick = true;
            this.btnList.Image = global::BluePrism.CharMatching.Properties.Resources.regionlist_16x16;
            resources.ApplyResources(this.btnList, "btnList");
            this.btnList.Name = "btnList";
            this.btnList.CheckedChanged += new System.EventHandler(this.HandleModeToolbarButton);
            // 
            // btnGrid
            // 
            this.btnGrid.CheckOnClick = true;
            resources.ApplyResources(this.btnGrid, "btnGrid");
            this.btnGrid.Image = global::BluePrism.CharMatching.Properties.Resources.regiongrid_16x16;
            this.btnGrid.Name = "btnGrid";
            this.btnGrid.CheckedChanged += new System.EventHandler(this.HandleModeToolbarButton);
            // 
            // sep2
            // 
            sep2.Name = "sep2";
            resources.ApplyResources(sep2, "sep2");
            // 
            // btnCut
            // 
            this.btnCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCut.Image = global::BluePrism.CharMatching.Properties.Resources.cut_16x16;
            resources.ApplyResources(this.btnCut, "btnCut");
            this.btnCut.Name = "btnCut";
            this.btnCut.Click += new System.EventHandler(this.HandleCut);
            // 
            // btnCopy
            // 
            this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopy.Image = global::BluePrism.CharMatching.Properties.Resources.copy_16x16;
            resources.ApplyResources(this.btnCopy, "btnCopy");
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Click += new System.EventHandler(this.HandleCopy);
            // 
            // btnPaste
            // 
            this.btnPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPaste.Image = global::BluePrism.CharMatching.Properties.Resources.paste_16x16;
            resources.ApplyResources(this.btnPaste, "btnPaste");
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Click += new System.EventHandler(this.HandlePaste);
            // 
            // btnDelete
            // 
            this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDelete.Image = global::BluePrism.CharMatching.Properties.Resources.delete_16x16;
            resources.ApplyResources(this.btnDelete, "btnDelete");
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Click += new System.EventHandler(this.HandleDelete);
            // 
            // sep3
            // 
            sep3.Name = "sep3";
            resources.ApplyResources(sep3, "sep3");
            // 
            // cmbZoom
            // 
            this.cmbZoom.Name = "cmbZoom";
            resources.ApplyResources(this.cmbZoom, "cmbZoom");
            // 
            // RegionMapper
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.toolstripCont);
            this.Name = "RegionMapper";
            this.toolstripCont.ContentPanel.ResumeLayout(false);
            this.toolstripCont.TopToolStripPanel.ResumeLayout(false);
            this.toolstripCont.TopToolStripPanel.PerformLayout();
            this.toolstripCont.ResumeLayout(false);
            this.toolstripCont.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.ctxMenuPropsGrid.ResumeLayout(false);
            this.ctxToolstrip.ResumeLayout(false);
            this.toolstrip.ResumeLayout(false);
            this.toolstrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private AutomateControls.ZoomingScrollablePictureBox picbox;
        private System.Windows.Forms.ToolStripContainer toolstripCont;
        private System.Windows.Forms.ToolStrip toolstrip;
        private System.Windows.Forms.ToolStripButton btnPointer;
        private System.Windows.Forms.ToolStripButton btnRegion;
        private System.Windows.Forms.ToolStripButton btnGrid;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.PropertyGrid propsElement;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.ComboBox cmbRegions;
        private System.Windows.Forms.ToolStripComboBox cmbZoom;
        private System.Windows.Forms.ToolStripButton btnCut;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripButton btnPaste;
        private System.Windows.Forms.ContextMenuStrip ctxMenuPropsGrid;
        private System.Windows.Forms.ToolStripMenuItem menuResetProperty;
        private System.Windows.Forms.ToolStripButton btnList;
        private System.Windows.Forms.ContextMenuStrip ctxToolstrip;
        private System.Windows.Forms.ToolStripMenuItem mnuShowText;
        private System.Windows.Forms.ToolStripSplitButton btnOpenImage;
        private System.Windows.Forms.ToolStripMenuItem menuSpy;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}
