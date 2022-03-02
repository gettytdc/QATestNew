namespace BluePrism.CharMatching.UI.Designer
{
    partial class GridSpyRegionSchemaEditorDropDown
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridSpyRegionSchemaEditorDropDown));
            this.llEdit = new System.Windows.Forms.LinkLabel();
            this.llAddCol = new System.Windows.Forms.LinkLabel();
            this.llAddRow = new System.Windows.Forms.LinkLabel();
            this.llDelCol = new System.Windows.Forms.LinkLabel();
            this.llDelRow = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // llEdit
            // 
            resources.ApplyResources(this.llEdit, "llEdit");
            this.llEdit.Name = "llEdit";
            this.llEdit.TabStop = true;
            this.llEdit.UseMnemonic = false;
            this.llEdit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleEdit);
            // 
            // llAddCol
            // 
            resources.ApplyResources(this.llAddCol, "llAddCol");
            this.llAddCol.Name = "llAddCol";
            this.llAddCol.TabStop = true;
            this.llAddCol.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleAddColumn);
            // 
            // llAddRow
            // 
            resources.ApplyResources(this.llAddRow, "llAddRow");
            this.llAddRow.Name = "llAddRow";
            this.llAddRow.TabStop = true;
            this.llAddRow.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleAddRow);
            // 
            // llDelCol
            // 
            resources.ApplyResources(this.llDelCol, "llDelCol");
            this.llDelCol.Name = "llDelCol";
            this.llDelCol.TabStop = true;
            this.llDelCol.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleDeleteColumn);
            // 
            // llDelRow
            // 
            resources.ApplyResources(this.llDelRow, "llDelRow");
            this.llDelRow.Name = "llDelRow";
            this.llDelRow.TabStop = true;
            this.llDelRow.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleDeleteRow);
            // 
            // GridSpyRegionSchemaEditorDropDown
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.llDelCol);
            this.Controls.Add(this.llDelRow);
            this.Controls.Add(this.llAddCol);
            this.Controls.Add(this.llAddRow);
            this.Controls.Add(this.llEdit);
            this.Name = "GridSpyRegionSchemaEditorDropDown";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel llEdit;
        private System.Windows.Forms.LinkLabel llAddCol;
        private System.Windows.Forms.LinkLabel llAddRow;
        private System.Windows.Forms.LinkLabel llDelCol;
        private System.Windows.Forms.LinkLabel llDelRow;
    }
}
