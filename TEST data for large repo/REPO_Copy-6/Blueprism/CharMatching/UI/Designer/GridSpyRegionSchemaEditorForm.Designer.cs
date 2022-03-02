namespace BluePrism.CharMatching.UI.Designer
{
    partial class GridSpyRegionSchemaEditorForm
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
            System.Windows.Forms.Panel panel1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridSpyRegionSchemaEditorForm));
            this.btnCancel = new AutomateControls.Buttons.StandardStyledButton();
            this.btnOk = new AutomateControls.Buttons.StandardStyledButton();
            this.editor = new BluePrism.CharMatching.UI.Designer.GridSpyRegionSchemaEditor();
            panel1 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(this.btnCancel);
            panel1.Controls.Add(this.btnOk);
            resources.ApplyResources(panel1, "panel1");
            panel1.Name = "panel1";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.HandleCancel);
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.HandleOk);
            // 
            // editor
            // 
            resources.ApplyResources(this.editor, "editor");
            this.editor.Name = "editor";
            // 
            // GridSpyRegionSchemaEditorForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.editor);
            this.Controls.Add(panel1);
            this.Name = "GridSpyRegionSchemaEditorForm";
            this.ShowIcon = false;
            panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GridSpyRegionSchemaEditor editor;
        private AutomateControls.Buttons.StandardStyledButton btnCancel;
        private AutomateControls.Buttons.StandardStyledButton btnOk;
    }
}
