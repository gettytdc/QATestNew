namespace AutomateControlTester.Configurators
{
    partial class TreeListViewConfigurator
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
            AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer treeListViewItemCollectionComparer1 = new AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer();
            this.tvFilms = new AutomateControls.TreeList.TreeListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colScore = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imgsSmall = new System.Windows.Forms.ImageList(this.components);
            this.imgsLarge = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // tvFilms
            // 
            this.tvFilms.AllowDrop = true;
            this.tvFilms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvFilms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colDescription,
            this.colScore});
            treeListViewItemCollectionComparer1.Column = 0;
            treeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending;
            this.tvFilms.Comparer = treeListViewItemCollectionComparer1;
            this.tvFilms.HideSelection = false;
            this.tvFilms.Location = new System.Drawing.Point(3, 0);
            this.tvFilms.Name = "tvFilms";
            this.tvFilms.ShowGroups = false;
            this.tvFilms.Size = new System.Drawing.Size(447, 273);
            this.tvFilms.SmallImageList = this.imgsSmall;
            this.tvFilms.TabIndex = 0;
            this.tvFilms.UseCompatibleStateImageBehavior = false;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 150;
            // 
            // colDescription
            // 
            this.colDescription.Text = "Description";
            this.colDescription.Width = 200;
            // 
            // colScore
            // 
            this.colScore.Text = "Rotten Tomatoes Score";
            // 
            // imgsSmall
            // 
            this.imgsSmall.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imgsSmall.ImageSize = new System.Drawing.Size(16, 16);
            this.imgsSmall.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imgsLarge
            // 
            this.imgsLarge.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imgsLarge.ImageSize = new System.Drawing.Size(32, 32);
            this.imgsLarge.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // TreeListViewConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvFilms);
            this.Name = "TreeListViewConfigurator";
            this.Size = new System.Drawing.Size(450, 273);
            this.ResumeLayout(false);

        }

        #endregion

        private AutomateControls.TreeList.TreeListView tvFilms;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colDescription;
        private System.Windows.Forms.ColumnHeader colScore;
        private System.Windows.Forms.ImageList imgsSmall;
        private System.Windows.Forms.ImageList imgsLarge;
    }
}
