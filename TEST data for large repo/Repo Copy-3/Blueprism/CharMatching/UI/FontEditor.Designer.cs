namespace BluePrism.CharMatching.UI
{
    partial class FontEditor
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
            System.Windows.Forms.Label label1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontEditor));
            System.Windows.Forms.Label label2;
            this.txtName = new AutomateControls.Textboxes.StyledTextBox();
            this.txtVer = new AutomateControls.Textboxes.StyledTextBox();
            this.splitPanel = new System.Windows.Forms.SplitContainer();
            this.cmbForeground = new AutomateControls.ColorComboBox();
            this.btnExtract = new AutomateControls.Buttons.StandardStyledButton();
            this.cbAutoTrim = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleRegionEditor = new AutomateControls.Buttons.StandardStyledButton();
            this.btnMerge = new AutomateControls.Buttons.StandardStyledButton();
            this.btnDelete = new AutomateControls.Buttons.StandardStyledButton();
            this.label4 = new System.Windows.Forms.Label();
            this.numSpaceWidth = new AutomateControls.StyledNumericUpDown();
            this.regMapper = new BluePrism.CharMatching.UI.RegionMapper();
            this.charEditor = new BluePrism.CharMatching.UI.CharacterEditor();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel)).BeginInit();
            this.splitPanel.Panel1.SuspendLayout();
            this.splitPanel.Panel2.SuspendLayout();
            this.splitPanel.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSpaceWidth)).BeginInit();
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
            // 
            // txtName
            // 
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.Name = "txtName";
            this.txtName.Validating += new System.ComponentModel.CancelEventHandler(this.HandleNameTextValidating);
            this.txtName.Validated += new System.EventHandler(this.HandleNameValidated);
            // 
            // txtVer
            // 
            resources.ApplyResources(this.txtVer, "txtVer");
            this.txtVer.Name = "txtVer";
            this.txtVer.Validated += new System.EventHandler(this.HandleVerTextValidated);
            // 
            // splitPanel
            // 
            resources.ApplyResources(this.splitPanel, "splitPanel");
            this.splitPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitPanel.Name = "splitPanel";
            // 
            // splitPanel.Panel1
            // 
            this.splitPanel.Panel1.Controls.Add(this.regMapper);
            this.splitPanel.Panel1.Controls.Add(this.cmbForeground);
            this.splitPanel.Panel1.Controls.Add(this.btnExtract);
            this.splitPanel.Panel1.Controls.Add(this.cbAutoTrim);
            this.splitPanel.Panel1.Controls.Add(this.label3);
            // 
            // splitPanel.Panel2
            // 
            this.splitPanel.Panel2.Controls.Add(this.flowLayoutPanel1);
            this.splitPanel.Panel2.Controls.Add(this.charEditor);
            // 
            // cmbForeground
            // 
            resources.ApplyResources(this.cmbForeground, "cmbForeground");
            this.cmbForeground.FormattingEnabled = true;
            this.cmbForeground.Name = "cmbForeground";
            this.cmbForeground.SelectedColor = System.Drawing.Color.Empty;
            // 
            // btnExtract
            // 
            resources.ApplyResources(this.btnExtract, "btnExtract");
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.HandleExtractClick);
            // 
            // cbAutoTrim
            // 
            resources.ApplyResources(this.cbAutoTrim, "cbAutoTrim");
            this.cbAutoTrim.Checked = true;
            this.cbAutoTrim.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoTrim.Name = "cbAutoTrim";
            this.cbAutoTrim.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnToggleRegionEditor);
            this.flowLayoutPanel1.Controls.Add(this.btnMerge);
            this.flowLayoutPanel1.Controls.Add(this.btnDelete);
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // cbToggleRegionEditor
            // 
            resources.ApplyResources(this.btnToggleRegionEditor, "cbToggleRegionEditor");
            this.btnToggleRegionEditor.Name = "cbToggleRegionEditor";
            this.btnToggleRegionEditor.UseVisualStyleBackColor = true;
            this.btnToggleRegionEditor.Click += new System.EventHandler(this.HandleToggleRegionEditor);
            // 
            // btnMerge
            // 
            resources.ApplyResources(this.btnMerge, "btnMerge");
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.HandleMergeClick);
            // 
            // btnDelete
            // 
            resources.ApplyResources(this.btnDelete, "btnDelete");
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.HandleDeleteClick);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // numSpaceWidth
            // 
            resources.ApplyResources(this.numSpaceWidth, "numSpaceWidth");
            this.numSpaceWidth.Name = "numSpaceWidth";
            this.numSpaceWidth.ValueChanged += new System.EventHandler(this.numSpaceWidth_ValueChanged);
            // 
            // regMapper
            // 
            resources.ApplyResources(this.regMapper, "regMapper");
            this.regMapper.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.regMapper.Name = "regMapper";
            this.regMapper.SingleRegionMode = true;
            this.regMapper.SpyButtonAvailable = true;
            this.regMapper.SpyRequested += new BluePrism.CharMatching.UI.SpyRequestEventHandler(this.HandleSpyRequested);
            this.regMapper.RegionSelected += new BluePrism.CharMatching.UI.SpyRegionEventHandler(this.HandleRegionChanged);
            this.regMapper.RegionLayoutChanged += new BluePrism.CharMatching.UI.SpyRegionEventHandler(this.HandleRegionChanged);
            // 
            // charEditor
            // 
            resources.ApplyResources(this.charEditor, "charEditor");
            this.charEditor.Name = "charEditor";
            this.charEditor.SelectionChanged += new System.EventHandler(this.HandleCharSelectionChanged);
            // 
            // FontEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(label1);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtVer);
            this.Controls.Add(label2);
            this.Controls.Add(this.numSpaceWidth);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.splitPanel);
            resources.ApplyResources(this, "$this");
            this.Name = "FontEditor";
            this.splitPanel.Panel1.ResumeLayout(false);
            this.splitPanel.Panel1.PerformLayout();
            this.splitPanel.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel)).EndInit();
            this.splitPanel.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSpaceWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AutomateControls.Textboxes.StyledTextBox txtName;
        private AutomateControls.Textboxes.StyledTextBox txtVer;
        private CharacterEditor charEditor;
        private System.Windows.Forms.SplitContainer splitPanel;
        private RegionMapper regMapper;
        private AutomateControls.ColorComboBox cmbForeground;
        private AutomateControls.Buttons.StandardStyledButton btnExtract;
        private System.Windows.Forms.CheckBox cbAutoTrim;
        private System.Windows.Forms.Label label3;
        private AutomateControls.Buttons.StandardStyledButton btnToggleRegionEditor;
        private AutomateControls.Buttons.StandardStyledButton btnMerge;
        private System.Windows.Forms.Label label4;
        private AutomateControls.StyledNumericUpDown numSpaceWidth;
        private AutomateControls.Buttons.StandardStyledButton btnDelete;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
