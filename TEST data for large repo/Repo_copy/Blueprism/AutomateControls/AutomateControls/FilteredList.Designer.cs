namespace AutomateControls
{
    partial class FilteredList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilteredList));
            this.flowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lview = new AutomateControls.ScrollFiringListView();
            this.SuspendLayout();
            // 
            // flowPanel
            // 
            resources.ApplyResources(this.flowPanel, "flowPanel");
            this.flowPanel.Name = "flowPanel";
            // 
            // lview
            // 
            resources.ApplyResources(this.lview, "lview");
            this.lview.FullRowSelect = true;
            this.lview.GridLines = true;
            this.lview.Name = "lview";
            this.lview.UseCompatibleStateImageBehavior = false;
            this.lview.View = System.Windows.Forms.View.Details;
            // 
            // FilteredList
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.lview);
            this.Controls.Add(this.flowPanel);
            this.Name = "FilteredList";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowPanel;
        private AutomateControls.ScrollFiringListView lview;
    }
}
