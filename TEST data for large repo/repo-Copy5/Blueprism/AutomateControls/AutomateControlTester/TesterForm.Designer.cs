namespace AutomateControlTester
{
    partial class TesterForm
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
            System.Windows.Forms.Label label2;
            this.btnCancel = new AutomateControls.Buttons.StandardStyledButton();
            this.btnOK = new AutomateControls.Buttons.StandardStyledButton();
            this.grippableSplitContainer1 = new AutomateControls.GrippableSplitContainer();
            this.gridControls = new AutomateControls.DataGridViews.RowBasedDataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gpConfig = new System.Windows.Forms.GroupBox();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            label2 = new System.Windows.Forms.Label();
            this.grippableSplitContainer1.Panel1.SuspendLayout();
            this.grippableSplitContainer1.Panel2.SuspendLayout();
            this.grippableSplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControls)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 9);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(198, 13);
            label2.TabIndex = 1;
            label2.Text = "Choose the control you would like to test";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(505, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(424, 280);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // grippableSplitContainer1
            // 
            this.grippableSplitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grippableSplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.grippableSplitContainer1.Location = new System.Drawing.Point(15, 25);
            this.grippableSplitContainer1.Name = "grippableSplitContainer1";
            // 
            // grippableSplitContainer1.Panel1
            // 
            this.grippableSplitContainer1.Panel1.Controls.Add(this.gridControls);
            // 
            // grippableSplitContainer1.Panel2
            // 
            this.grippableSplitContainer1.Panel2.Controls.Add(this.gpConfig);
            this.grippableSplitContainer1.Size = new System.Drawing.Size(565, 249);
            this.grippableSplitContainer1.SplitterDistance = 128;
            this.grippableSplitContainer1.TabIndex = 6;
            this.grippableSplitContainer1.TabStop = false;
            // 
            // gridControls
            // 
            this.gridControls.AllowUserToAddRows = false;
            this.gridControls.AllowUserToDeleteRows = false;
            this.gridControls.AllowUserToResizeRows = false;
            this.gridControls.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridControls.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridControls.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridControls.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName});
            this.gridControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControls.Location = new System.Drawing.Point(0, 0);
            this.gridControls.MultiSelect = false;
            this.gridControls.Name = "gridControls";
            this.gridControls.RowHeadersVisible = false;
            this.gridControls.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridControls.ShowCellErrors = false;
            this.gridControls.ShowEditingIcon = false;
            this.gridControls.ShowRowErrors = false;
            this.gridControls.Size = new System.Drawing.Size(128, 249);
            this.gridControls.TabIndex = 5;
            // 
            // colName
            // 
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // gpConfig
            // 
            this.gpConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gpConfig.Location = new System.Drawing.Point(0, 0);
            this.gpConfig.Name = "gpConfig";
            this.gpConfig.Size = new System.Drawing.Size(433, 249);
            this.gpConfig.TabIndex = 0;
            this.gpConfig.TabStop = false;
            this.gpConfig.Text = "Configuration";
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.HeaderText = "Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // TesterForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(592, 311);
            this.Controls.Add(this.grippableSplitContainer1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(label2);
            this.MinimumSize = new System.Drawing.Size(400, 200);
            this.Name = "TesterForm";
            this.Text = "Automate Controls Tester";
            this.grippableSplitContainer1.Panel1.ResumeLayout(false);
            this.grippableSplitContainer1.Panel2.ResumeLayout(false);
            this.grippableSplitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControls)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AutomateControls.Buttons.StandardStyledButton btnCancel;
        private AutomateControls.Buttons.StandardStyledButton btnOK;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private AutomateControls.DataGridViews.RowBasedDataGridView gridControls;
        private AutomateControls.GrippableSplitContainer grippableSplitContainer1;
        private System.Windows.Forms.GroupBox gpConfig;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
    }
}

