namespace BluePrism.CharMatching.UI
{
    partial class RegionEditorForm
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
            System.Windows.Forms.Label label3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegionEditorForm));
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Panel panButtons;
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnDeleteChars = new AutomateControls.Buttons.StandardStyledButton();
            this.cbToggleChars = new AutomateControls.Buttons.StandardStyledButton();
            this.btnMerge = new AutomateControls.Buttons.StandardStyledButton();
            this.btnOK = new AutomateControls.Buttons.StandardStyledButton();
            this.btnCancel = new AutomateControls.Buttons.StandardStyledButton();
            this.btnApply = new AutomateControls.Buttons.StandardStyledButton();
            this.lblFontCount = new System.Windows.Forms.Label();
            this.lblFontName = new System.Windows.Forms.Label();
            this.splitPane = new System.Windows.Forms.SplitContainer();
            this.regMapper = new BluePrism.CharMatching.UI.RegionMapper();
            this.flowFontInfo = new System.Windows.Forms.FlowLayoutPanel();
            this.chkShowAll = new System.Windows.Forms.CheckBox();
            this.charList = new BluePrism.CharMatching.UI.CharacterListPanel();
            label3 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            panButtons = new System.Windows.Forms.Panel();
            panButtons.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPane)).BeginInit();
            this.splitPane.Panel1.SuspendLayout();
            this.splitPane.Panel2.SuspendLayout();
            this.splitPane.SuspendLayout();
            this.flowFontInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // panButtons
            // 
            panButtons.Controls.Add(this.flowLayoutPanel1);
            panButtons.Controls.Add(this.btnOK);
            panButtons.Controls.Add(this.btnCancel);
            panButtons.Controls.Add(this.btnApply);
            resources.ApplyResources(panButtons, "panButtons");
            panButtons.Name = "panButtons";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.btnDeleteChars);
            this.flowLayoutPanel1.Controls.Add(this.cbToggleChars);
            this.flowLayoutPanel1.Controls.Add(this.btnMerge);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // btnDeleteChars
            // 
            resources.ApplyResources(this.btnDeleteChars, "btnDeleteChars");
            this.btnDeleteChars.Name = "btnDeleteChars";
            this.btnDeleteChars.UseVisualStyleBackColor = true;
            this.btnDeleteChars.Click += new System.EventHandler(this.HandleDeleteClick);
            // 
            // cbToggleChars
            // 
            resources.ApplyResources(this.cbToggleChars, "cbToggleChars");
            this.cbToggleChars.Name = "cbToggleChars";
            this.cbToggleChars.UseVisualStyleBackColor = true;
            this.cbToggleChars.Click += new System.EventHandler(this.HandleToggleCharList);
            // 
            // btnMerge
            // 
            resources.ApplyResources(this.btnMerge, "btnMerge");
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.HandleMergeClick);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.HandleOkClick);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.HandleCancelClick);
            // 
            // btnApply
            // 
            resources.ApplyResources(this.btnApply, "btnApply");
            this.btnApply.Name = "btnApply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.HandleApplyClick);
            // 
            // lblFontCount
            // 
            resources.ApplyResources(this.lblFontCount, "lblFontCount");
            this.lblFontCount.Name = "lblFontCount";
            // 
            // lblFontName
            // 
            resources.ApplyResources(this.lblFontName, "lblFontName");
            this.lblFontName.Name = "lblFontName";
            // 
            // splitPane
            // 
            resources.ApplyResources(this.splitPane, "splitPane");
            this.splitPane.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitPane.Name = "splitPane";
            // 
            // splitPane.Panel1
            // 
            this.splitPane.Panel1.Controls.Add(this.regMapper);
            // 
            // splitPane.Panel2
            // 
            this.splitPane.Panel2.Controls.Add(this.flowFontInfo);
            this.splitPane.Panel2.Controls.Add(this.charList);
            // 
            // regMapper
            // 
            resources.ApplyResources(this.regMapper, "regMapper");
            this.regMapper.Name = "regMapper";
            this.regMapper.RegionSelected += new BluePrism.CharMatching.UI.SpyRegionEventHandler(this.HandleRegionSelected);
            this.regMapper.RegionLayoutChanged += new BluePrism.CharMatching.UI.SpyRegionEventHandler(this.HandleRegionLayoutChanged);
            this.regMapper.RegionCharDataChanged += new BluePrism.CharMatching.UI.SpyRegionEventHandler(this.HandleRegionCharDataChanged);
            // 
            // flowFontInfo
            // 
            resources.ApplyResources(this.flowFontInfo, "flowFontInfo");
            this.flowFontInfo.Controls.Add(label1);
            this.flowFontInfo.Controls.Add(this.lblFontName);
            this.flowFontInfo.Controls.Add(label3);
            this.flowFontInfo.Controls.Add(this.lblFontCount);
            this.flowFontInfo.Controls.Add(this.chkShowAll);
            this.flowFontInfo.Name = "flowFontInfo";
            // 
            // chkShowAll
            // 
            resources.ApplyResources(this.chkShowAll, "chkShowAll");
            this.chkShowAll.Checked = true;
            this.chkShowAll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowAll.Name = "chkShowAll";
            this.chkShowAll.UseVisualStyleBackColor = true;
            this.chkShowAll.CheckedChanged += new System.EventHandler(this.HandleShowAllChanged);
            // 
            // charList
            // 
            resources.ApplyResources(this.charList, "charList");
            this.charList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.charList.MultiSelect = true;
            this.charList.Name = "charList";
            this.charList.SelectionChanged += new System.EventHandler(this.HandleCharSelectionChanged);
            // 
            // RegionEditorForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.splitPane);
            this.Controls.Add(panButtons);
            this.Name = "RegionEditorForm";
            panButtons.ResumeLayout(false);
            panButtons.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.splitPane.Panel1.ResumeLayout(false);
            this.splitPane.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPane)).EndInit();
            this.splitPane.ResumeLayout(false);
            this.flowFontInfo.ResumeLayout(false);
            this.flowFontInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblFontCount;
        private System.Windows.Forms.Label lblFontName;
        private System.Windows.Forms.SplitContainer splitPane;
        protected RegionMapper regMapper;
        private System.Windows.Forms.CheckBox chkShowAll;
        private AutomateControls.Buttons.StandardStyledButton btnMerge;
        private CharacterListPanel charList;
        private AutomateControls.Buttons.StandardStyledButton cbToggleChars;
        private AutomateControls.Buttons.StandardStyledButton btnOK;
        private AutomateControls.Buttons.StandardStyledButton btnCancel;
        private AutomateControls.Buttons.StandardStyledButton btnApply;
        private System.Windows.Forms.FlowLayoutPanel flowFontInfo;
        private AutomateControls.Buttons.StandardStyledButton btnDeleteChars;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
