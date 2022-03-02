using BluePrism.BPServer.Properties;
namespace BluePrism.BPServer
{
    partial class frmManageUrlReservations
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmManageUrlReservations));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvUrlReservations = new System.Windows.Forms.DataGridView();
            this.colUrlPrefix = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblIntroduction = new System.Windows.Forms.Label();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.llEdit = new AutomateControls.BulletedLinkLabel();
            this.llDelete = new AutomateControls.BulletedLinkLabel();
            this.llAdd = new AutomateControls.BulletedLinkLabel();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnClose = new AutomateControls.Buttons.StandardStyledButton();
            this.txtURLPermissionInformation = new AutomateControls.Textboxes.StyledTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUrlReservations)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvUrlReservations
            // 
            this.dgvUrlReservations.AllowUserToAddRows = false;
            this.dgvUrlReservations.AllowUserToDeleteRows = false;
            resources.ApplyResources(this.dgvUrlReservations, "dgvUrlReservations");
            this.dgvUrlReservations.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUrlReservations.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvUrlReservations.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvUrlReservations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUrlReservations.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colUrlPrefix,
            this.colUserName});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvUrlReservations.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvUrlReservations.MultiSelect = false;
            this.dgvUrlReservations.Name = "dgvUrlReservations";
            this.dgvUrlReservations.RowHeadersVisible = false;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvUrlReservations.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvUrlReservations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUrlReservations.SelectionChanged += new System.EventHandler(this.HandleGridSelectionChanged);
            // 
            // colUrlPrefix
            // 
            this.colUrlPrefix.DataPropertyName = "UrlDescription";
            this.colUrlPrefix.FillWeight = 25F;
            resources.ApplyResources(this.colUrlPrefix, "colUrlPrefix");
            this.colUrlPrefix.Name = "colUrlPrefix";
            // 
            // colUserName
            // 
            this.colUserName.DataPropertyName = "UsersSummary";
            this.colUserName.FillWeight = 40F;
            resources.ApplyResources(this.colUserName, "colUserName");
            this.colUserName.Name = "colUserName";
            this.colUserName.ReadOnly = true;
            // 
            // lblIntroduction
            // 
            resources.ApplyResources(this.lblIntroduction, "lblIntroduction");
            this.lblIntroduction.Name = "lblIntroduction";
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = typeof(BluePrism.BPServer.Properties.Resources).Name;
            this.dataGridViewTextBoxColumn1.FillWeight = 25F;
            resources.ApplyResources(this.dataGridViewTextBoxColumn1, "dataGridViewTextBoxColumn1");
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "PathName";
            this.dataGridViewTextBoxColumn2.FillWeight = 40F;
            resources.ApplyResources(this.dataGridViewTextBoxColumn2, "dataGridViewTextBoxColumn2");
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // llEdit
            // 
            resources.ApplyResources(this.llEdit, "llEdit");
            this.llEdit.LinkColor = System.Drawing.SystemColors.ControlText;
            this.llEdit.Name = "llEdit";
            this.llEdit.TabStop = true;
            this.llEdit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleEdit);
            // 
            // llDelete
            // 
            resources.ApplyResources(this.llDelete, "llDelete");
            this.llDelete.LinkColor = System.Drawing.SystemColors.ControlText;
            this.llDelete.Name = "llDelete";
            this.llDelete.TabStop = true;
            this.llDelete.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleDelete);
            // 
            // llAdd
            // 
            resources.ApplyResources(this.llAdd, "llAdd");
            this.llAdd.LinkColor = System.Drawing.SystemColors.ControlText;
            this.llAdd.Name = "llAdd";
            this.llAdd.TabStop = true;
            this.llAdd.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleAdd);
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "AllowListen";
            this.dataGridViewTextBoxColumn3.FillWeight = 22F;
            resources.ApplyResources(this.dataGridViewTextBoxColumn3, "dataGridViewTextBoxColumn3");
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "AllowDelegate";
            this.dataGridViewTextBoxColumn4.FillWeight = 23F;
            resources.ApplyResources(this.dataGridViewTextBoxColumn4, "dataGridViewTextBoxColumn4");
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // txtURLPermissionInformation
            // 
            this.txtURLPermissionInformation.AcceptsReturn = true;
            resources.ApplyResources(this.txtURLPermissionInformation, "txtURLPermissionInformation");
            this.txtURLPermissionInformation.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtURLPermissionInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtURLPermissionInformation.Name = "txtURLPermissionInformation";
            this.txtURLPermissionInformation.ReadOnly = true;
            // 
            // frmManageUrlReservations
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.txtURLPermissionInformation);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.llEdit);
            this.Controls.Add(this.llDelete);
            this.Controls.Add(this.llAdd);
            this.Controls.Add(this.dgvUrlReservations);
            this.Controls.Add(this.lblIntroduction);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmManageUrlReservations";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            ((System.ComponentModel.ISupportInitialize)(this.dgvUrlReservations)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvUrlReservations;
        private System.Windows.Forms.Label lblIntroduction;
        private AutomateControls.BulletedLinkLabel llDelete;
        private AutomateControls.BulletedLinkLabel llAdd;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private AutomateControls.BulletedLinkLabel llEdit;
        private AutomateControls.Buttons.StandardStyledButton btnClose;
        private AutomateControls.Textboxes.StyledTextBox txtURLPermissionInformation;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUrlPrefix;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUserName;
    }
}