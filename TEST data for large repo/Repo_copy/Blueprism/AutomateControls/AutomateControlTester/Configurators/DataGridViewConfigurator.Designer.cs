namespace AutomateControlTester.Configurators
{
    partial class DataGridViewConfigurator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataGridViewConfigurator));
            this.imgList = new System.Windows.Forms.ImageList(this.components);
            this.imgList32 = new System.Windows.Forms.ImageList(this.components);
            this.gridTest = new AutomateControls.DataGridViews.RowBasedDataGridView();
            this.colmage = new AutomateControls.DataGridViews.ImageListColumn();
            this.colItemStatus = new AutomateControls.DataGridViews.ItemStatusColumn();
            this.colInfo = new AutomateControls.DataGridViews.DataGridViewItemHeaderColumn();
            this.colSpinner = new AutomateControls.DataGridViews.DataGridViewNumericUpDownColumn();
            this.colStart = new AutomateControls.DataGridViews.DateColumn();
            this.colEnd = new AutomateControls.DataGridViews.DateColumn();
            this.colReadOnlyDate = new AutomateControls.DataGridViews.DateColumn();
            this.colTest = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridTest)).BeginInit();
            this.SuspendLayout();
            // 
            // imgList
            // 
            this.imgList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList.ImageStream")));
            this.imgList.TransparentColor = System.Drawing.Color.Transparent;
            this.imgList.Images.SetKeyName(0, "dataitem");
            this.imgList.Images.SetKeyName(1, "box");
            this.imgList.Images.SetKeyName(2, "boxclosed");
            this.imgList.Images.SetKeyName(3, "calendar");
            this.imgList.Images.SetKeyName(4, "clock");
            this.imgList.Images.SetKeyName(5, "database");
            this.imgList.Images.SetKeyName(6, "font");
            this.imgList.Images.SetKeyName(7, "queue");
            this.imgList.Images.SetKeyName(8, "queuegroup");
            // 
            // imgList32
            // 
            this.imgList32.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList32.ImageStream")));
            this.imgList32.TransparentColor = System.Drawing.Color.Transparent;
            this.imgList32.Images.SetKeyName(0, "ClosedBox");
            this.imgList32.Images.SetKeyName(1, "OpenBox");
            this.imgList32.Images.SetKeyName(2, "Calendar");
            this.imgList32.Images.SetKeyName(3, "Class");
            this.imgList32.Images.SetKeyName(4, "Clock");
            this.imgList32.Images.SetKeyName(5, "Label");
            this.imgList32.Images.SetKeyName(6, "DateTime");
            this.imgList32.Images.SetKeyName(7, "Project");
            // 
            // gridTest
            // 
            this.gridTest.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.gridTest.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridTest.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridTest.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colmage,
            this.colItemStatus,
            this.colInfo,
            this.colSpinner,
            this.colStart,
            this.colEnd,
            this.colReadOnlyDate,
            this.colTest});
            this.gridTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTest.Location = new System.Drawing.Point(0, 0);
            this.gridTest.Name = "gridTest";
            this.gridTest.Size = new System.Drawing.Size(534, 324);
            this.gridTest.TabIndex = 0;
            // 
            // colmage
            // 
            this.colmage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colmage.FillWeight = 1F;
            this.colmage.HeaderText = "Image";
            this.colmage.ImageList = this.imgList;
            this.colmage.MinimumWidth = 16;
            this.colmage.Name = "colmage";
            this.colmage.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colmage.Width = 42;
            // 
            // colItemStatus
            // 
            this.colItemStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colItemStatus.HeaderText = "Item Status";
            this.colItemStatus.MinimumWidth = 24;
            this.colItemStatus.Name = "colItemStatus";
            this.colItemStatus.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colItemStatus.Width = 66;
            // 
            // colInfo
            // 
            this.colInfo.HeaderText = "Info";
            this.colInfo.ImageList = this.imgList32;
            this.colInfo.Name = "colInfo";
            // 
            // colSpinner
            // 
            this.colSpinner.DecimalPlaces = 1;
            this.colSpinner.HeaderText = "%";
            this.colSpinner.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.colSpinner.Name = "colSpinner";
            this.colSpinner.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // colStart
            // 
            this.colStart.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colStart.DateFormat = "dd/MM/yyyy";
            this.colStart.HeaderText = "Start Date";
            this.colStart.Name = "colStart";
            this.colStart.ShowBoundaryDates = true;
            this.colStart.Width = 80;
            // 
            // colEnd
            // 
            this.colEnd.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colEnd.DateFormat = "ddd MMM yyyy";
            this.colEnd.HeaderText = "End Date";
            this.colEnd.Name = "colEnd";
            this.colEnd.ShowBoundaryDates = true;
            this.colEnd.Width = 77;
            // 
            // colReadOnlyDate
            // 
            this.colReadOnlyDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colReadOnlyDate.DateFormat = null;
            this.colReadOnlyDate.HeaderText = "Read Only Date";
            this.colReadOnlyDate.Name = "colReadOnlyDate";
            this.colReadOnlyDate.Width = 108;
            // 
            // colTest
            // 
            this.colTest.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTest.HeaderText = "Text";
            this.colTest.Name = "colTest";
            // 
            // DataGridViewConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridTest);
            this.Name = "DataGridViewConfigurator";
            this.Size = new System.Drawing.Size(534, 324);
            ((System.ComponentModel.ISupportInitialize)(this.gridTest)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AutomateControls.DataGridViews.RowBasedDataGridView gridTest;
        private System.Windows.Forms.ImageList imgList;
        private System.Windows.Forms.ImageList imgList32;
        private AutomateControls.DataGridViews.ImageListColumn colmage;
        private AutomateControls.DataGridViews.ItemStatusColumn colItemStatus;
        private AutomateControls.DataGridViews.DataGridViewItemHeaderColumn colInfo;
        private AutomateControls.DataGridViews.DataGridViewNumericUpDownColumn colSpinner;
        private AutomateControls.DataGridViews.DateColumn colStart;
        private AutomateControls.DataGridViews.DateColumn colEnd;
        private AutomateControls.DataGridViews.DateColumn colReadOnlyDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTest;

    }
}
