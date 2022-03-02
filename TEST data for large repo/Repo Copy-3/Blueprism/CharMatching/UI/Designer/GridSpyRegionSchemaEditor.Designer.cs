namespace BluePrism.CharMatching.UI.Designer
{
    partial class GridSpyRegionSchemaEditor
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
            System.Windows.Forms.Label lblShow;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridSpyRegionSchemaEditor));
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            this.cmbVectorType = new System.Windows.Forms.ComboBox();
            this.lstVectors = new System.Windows.Forms.ListView();
            this.colNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSizeType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnAdd = new AutomateControls.Buttons.StandardStyledButton();
            this.btnDelete = new AutomateControls.Buttons.StandardStyledButton();
            this.btnInsert = new AutomateControls.Buttons.StandardStyledButton();
            this.gpSizeType = new System.Windows.Forms.GroupBox();
            this.numProportion = new AutomateControls.StyledNumericUpDown();
            this.numAbsolute = new AutomateControls.StyledNumericUpDown();
            this.rbProportion = new AutomateControls.StyledRadioButton();
            this.rbAbsolute = new AutomateControls.StyledRadioButton();
            lblShow = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.gpSizeType.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numProportion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAbsolute)).BeginInit();
            this.SuspendLayout();
            // 
            // lblShow
            // 
            resources.ApplyResources(lblShow, "lblShow");
            lblShow.Name = "lblShow";
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
            // cmbVectorType
            // 
            resources.ApplyResources(this.cmbVectorType, "cmbVectorType");
            this.cmbVectorType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVectorType.FormattingEnabled = true;
            this.cmbVectorType.Items.AddRange(new object[] {
            resources.GetString("cmbVectorType.Items"),
            resources.GetString("cmbVectorType.Items1")});
            this.cmbVectorType.Name = "cmbVectorType";
            this.cmbVectorType.SelectedIndexChanged += new System.EventHandler(this.HandleVectorTypeChanged);
            // 
            // lstVectors
            // 
            resources.ApplyResources(this.lstVectors, "lstVectors");
            this.lstVectors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colNumber,
            this.colSizeType,
            this.colValue});
            this.lstVectors.FullRowSelect = true;
            this.lstVectors.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstVectors.HideSelection = false;
            this.lstVectors.MultiSelect = false;
            this.lstVectors.Name = "lstVectors";
            this.lstVectors.UseCompatibleStateImageBehavior = false;
            this.lstVectors.View = System.Windows.Forms.View.Details;
            this.lstVectors.SelectedIndexChanged += new System.EventHandler(this.HandleVectorSelectionChanged);
            // 
            // colNumber
            // 
            resources.ApplyResources(this.colNumber, "colNumber");
            // 
            // colSizeType
            // 
            resources.ApplyResources(this.colSizeType, "colSizeType");
            // 
            // colValue
            // 
            resources.ApplyResources(this.colValue, "colValue");
            // 
            // btnAdd
            // 
            resources.ApplyResources(this.btnAdd, "btnAdd");
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            resources.ApplyResources(this.btnDelete, "btnDelete");
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnInsert
            // 
            resources.ApplyResources(this.btnInsert, "btnInsert");
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.UseVisualStyleBackColor = true;
            // 
            // gpSizeType
            // 
            resources.ApplyResources(this.gpSizeType, "gpSizeType");
            this.gpSizeType.Controls.Add(label2);
            this.gpSizeType.Controls.Add(this.numProportion);
            this.gpSizeType.Controls.Add(label1);
            this.gpSizeType.Controls.Add(this.numAbsolute);
            this.gpSizeType.Controls.Add(this.rbProportion);
            this.gpSizeType.Controls.Add(this.rbAbsolute);
            this.gpSizeType.Name = "gpSizeType";
            this.gpSizeType.TabStop = false;
            // 
            // numProportion
            // 
            resources.ApplyResources(this.numProportion, "numProportion");
            this.numProportion.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numProportion.Name = "numProportion";
            this.numProportion.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numProportion.ValueChanged += new System.EventHandler(this.HandleSizeValueChanged);
            // 
            // numAbsolute
            // 
            resources.ApplyResources(this.numAbsolute, "numAbsolute");
            this.numAbsolute.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numAbsolute.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numAbsolute.Name = "numAbsolute";
            this.numAbsolute.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numAbsolute.ValueChanged += new System.EventHandler(this.HandleSizeValueChanged);
            // 
            // rbProportion
            // 
            resources.ApplyResources(this.rbProportion, "rbProportion");
            this.rbProportion.Name = "rbProportion";
            this.rbProportion.TabStop = true;
            this.rbProportion.UseVisualStyleBackColor = true;
            this.rbProportion.CheckedChanged += new System.EventHandler(this.HandleSizeTypeChanged);
            // 
            // rbAbsolute
            // 
            resources.ApplyResources(this.rbAbsolute, "rbAbsolute");
            this.rbAbsolute.Name = "rbAbsolute";
            this.rbAbsolute.TabStop = true;
            this.rbAbsolute.UseVisualStyleBackColor = true;
            this.rbAbsolute.CheckedChanged += new System.EventHandler(this.HandleSizeTypeChanged);
            // 
            // GridSpyRegionSchemaEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.gpSizeType);
            this.Controls.Add(this.btnInsert);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstVectors);
            this.Controls.Add(this.cmbVectorType);
            this.Controls.Add(lblShow);
            resources.ApplyResources(this, "$this");
            this.Name = "GridSpyRegionSchemaEditor";
            this.gpSizeType.ResumeLayout(false);
            this.gpSizeType.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numProportion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAbsolute)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbVectorType;
        private System.Windows.Forms.ListView lstVectors;
        private System.Windows.Forms.ColumnHeader colNumber;
        private System.Windows.Forms.ColumnHeader colSizeType;
        private System.Windows.Forms.ColumnHeader colValue;
        private AutomateControls.Buttons.StandardStyledButton btnAdd;
        private AutomateControls.Buttons.StandardStyledButton btnDelete;
        private AutomateControls.Buttons.StandardStyledButton btnInsert;
        private System.Windows.Forms.GroupBox gpSizeType;
        private AutomateControls.StyledRadioButton rbProportion;
        private AutomateControls.StyledRadioButton rbAbsolute;
        private AutomateControls.StyledNumericUpDown numProportion;
        private AutomateControls.StyledNumericUpDown numAbsolute;

    }
}
