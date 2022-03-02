namespace AutomateControls.Trees
{
    partial class TreeViewAndFilter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TreeViewAndFilter));
            this.tvTree = new AutomateControls.Trees.FilterableTreeView();
            this.txtFilter = new AutomateControls.FilterTextBox();
            this.SuspendLayout();
            // 
            // tvTree
            // 
            resources.ApplyResources(this.tvTree, "tvTree");
            this.tvTree.Name = "tvTree";
            // 
            // txtFilter
            // 
            this.txtFilter.AlwaysShowHandOnFarHover = true;
            this.txtFilter.AlwaysShowHandOnNearHover = true;
            resources.ApplyResources(this.txtFilter, "txtFilter");
            this.txtFilter.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.FilterTextChanged += new AutomateControls.FilterEventHandler(this.HandleApplyFilter);
            this.txtFilter.FilterIconClick += new System.EventHandler(this.HandleSearchClick);
            this.txtFilter.FilterCleared += new System.EventHandler(this.HandleClearFilterClick);
            // 
            // TreeViewAndFilter
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tvTree);
            this.Controls.Add(this.txtFilter);
            this.Name = "TreeViewAndFilter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FilterableTreeView tvTree;
        private FilterTextBox txtFilter;
    }
}
