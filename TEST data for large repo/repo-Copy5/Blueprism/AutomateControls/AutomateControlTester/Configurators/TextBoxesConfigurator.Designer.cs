namespace AutomateControlTester.Configurators
{
    partial class TextBoxesConfigurator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextBoxesConfigurator));
            this.guidanceTextBox1 = new AutomateControls.GuidanceTextBox();
            this.filterTextBox1 = new AutomateControls.FilterTextBox();
            this.filterTextBox2 = new AutomateControls.FilterTextBox();
            this.iconTextBox1 = new AutomateControls.IconTextBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // guidanceTextBox1
            // 
            this.guidanceTextBox1.GuidanceColor = System.Drawing.Color.Fuchsia;
            this.guidanceTextBox1.GuidanceText = "Some guidance";
            this.guidanceTextBox1.Location = new System.Drawing.Point(21, 41);
            this.guidanceTextBox1.Name = "guidanceTextBox1";
            this.guidanceTextBox1.Size = new System.Drawing.Size(215, 20);
            this.guidanceTextBox1.TabIndex = 1;
            // 
            // filterTextBox1
            // 
            this.filterTextBox1.ClearTip = "Clearing the tip, please look out";
            this.filterTextBox1.GuidanceText = "Filter something or other";
            this.filterTextBox1.Location = new System.Drawing.Point(21, 15);
            this.filterTextBox1.Name = "filterTextBox1";
            this.filterTextBox1.SearchTip = "This is a search tip";
            this.filterTextBox1.Size = new System.Drawing.Size(215, 20);
            this.filterTextBox1.TabIndex = 0;
            this.filterTextBox1.Text = "Filter something or other";
            this.filterTextBox1.FilterCleared += new System.EventHandler(this.HandleFilterCleared);
            this.filterTextBox1.FilterIconClick += new System.EventHandler(this.HandleFilterIconClick);
            this.filterTextBox1.FilterTextChanged += new AutomateControls.FilterEventHandler(this.HandleFilterTextChanged);
            // 
            // filterTextBox2
            // 
            this.filterTextBox2.GuidanceText = "Mostly Defaulted (apart from this bit)";
            this.filterTextBox2.Location = new System.Drawing.Point(21, 67);
            this.filterTextBox2.Name = "filterTextBox2";
            this.filterTextBox2.Size = new System.Drawing.Size(215, 20);
            this.filterTextBox2.TabIndex = 2;
            this.filterTextBox2.Text = "Mostly Defaulted (apart from this bit)";
            // 
            // iconTextBox1
            // 
            this.iconTextBox1.FarImageDefault = 0;
            this.iconTextBox1.FarImageHover = 1;
            this.iconTextBox1.GuidanceText = "Arbitrary image time!";
            this.iconTextBox1.Images = this.imageList1;
            this.iconTextBox1.Location = new System.Drawing.Point(21, 93);
            this.iconTextBox1.Name = "iconTextBox1";
            this.iconTextBox1.NearImageDefault = 2;
            this.iconTextBox1.NearImageHover = 3;
            this.iconTextBox1.Size = new System.Drawing.Size(215, 20);
            this.iconTextBox1.TabIndex = 3;
            this.iconTextBox1.Text = "Arbitrary image time!";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "16x16 control-room.png");
            this.imageList1.Images.SetKeyName(1, "16x16 question.png");
            this.imageList1.Images.SetKeyName(2, "25-calc.png");
            this.imageList1.Images.SetKeyName(3, "03-businessobjects.png");
            // 
            // TextBoxesConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.iconTextBox1);
            this.Controls.Add(this.filterTextBox2);
            this.Controls.Add(this.guidanceTextBox1);
            this.Controls.Add(this.filterTextBox1);
            this.Name = "TextBoxesConfigurator";
            this.Size = new System.Drawing.Size(380, 217);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AutomateControls.FilterTextBox filterTextBox1;
        private AutomateControls.GuidanceTextBox guidanceTextBox1;
        private AutomateControls.FilterTextBox filterTextBox2;
        private AutomateControls.IconTextBox iconTextBox1;
        private System.Windows.Forms.ImageList imageList1;







    }
}
