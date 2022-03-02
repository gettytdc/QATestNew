namespace BluePrism.CharMatching.UI
{
    partial class FontBrowserPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontBrowserPanel));
            this.label2 = new System.Windows.Forms.Label();
            this.txtEntered = new AutomateControls.Textboxes.StyledTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbSize = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbStyle = new System.Windows.Forms.ComboBox();
            this.lbFontRenders = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnRender = new AutomateControls.Buttons.StandardStyledButton();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFontName = new AutomateControls.Textboxes.StyledTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.picboxPreview = new System.Windows.Forms.PictureBox();
            this.picbox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picboxPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picbox)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // txtEntered
            // 
            resources.ApplyResources(this.txtEntered, "txtEntered");
            this.txtEntered.Name = "txtEntered";
            this.txtEntered.TextChanged += new System.EventHandler(this.HandleTextChanged);
            this.txtEntered.Validated += new System.EventHandler(this.HandleReRenderRequired);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // cmbSize
            // 
            this.cmbSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSize.FormattingEnabled = true;
            this.cmbSize.Items.AddRange(new object[] {
            resources.GetString("cmbSize.Items"),
            resources.GetString("cmbSize.Items1"),
            resources.GetString("cmbSize.Items2"),
            resources.GetString("cmbSize.Items3"),
            resources.GetString("cmbSize.Items4"),
            resources.GetString("cmbSize.Items5"),
            resources.GetString("cmbSize.Items6"),
            resources.GetString("cmbSize.Items7"),
            resources.GetString("cmbSize.Items8"),
            resources.GetString("cmbSize.Items9"),
            resources.GetString("cmbSize.Items10"),
            resources.GetString("cmbSize.Items11"),
            resources.GetString("cmbSize.Items12"),
            resources.GetString("cmbSize.Items13")});
            resources.ApplyResources(this.cmbSize, "cmbSize");
            this.cmbSize.Name = "cmbSize";
            this.cmbSize.SelectedIndexChanged += new System.EventHandler(this.HandleReRenderRequired);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // cmbStyle
            // 
            resources.ApplyResources(this.cmbStyle, "cmbStyle");
            this.cmbStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStyle.FormattingEnabled = true;
            this.cmbStyle.Items.AddRange(new object[] {
            resources.GetString("cmbStyle.Items"),
            resources.GetString("cmbStyle.Items1"),
            resources.GetString("cmbStyle.Items2")});
            this.cmbStyle.Name = "cmbStyle";
            this.cmbStyle.SelectedIndexChanged += new System.EventHandler(this.HandleReRenderRequired);
            // 
            // lbFontRenders
            // 
            resources.ApplyResources(this.lbFontRenders, "lbFontRenders");
            this.lbFontRenders.FormattingEnabled = true;
            this.lbFontRenders.Name = "lbFontRenders";
            this.lbFontRenders.SelectedIndexChanged += new System.EventHandler(this.HandleReRenderRequired);
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // btnRender
            // 
            resources.ApplyResources(this.btnRender, "btnRender");
            this.btnRender.Name = "btnRender";
            this.btnRender.UseVisualStyleBackColor = true;
            this.btnRender.Click += new System.EventHandler(this.HandleRenderList);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // txtFontName
            // 
            resources.ApplyResources(this.txtFontName, "txtFontName");
            this.txtFontName.Name = "txtFontName";
            this.txtFontName.ReadOnly = true;
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // picboxPreview
            // 
            resources.ApplyResources(this.picboxPreview, "picboxPreview");
            this.picboxPreview.BackColor = System.Drawing.Color.AliceBlue;
            this.picboxPreview.Name = "picboxPreview";
            this.picboxPreview.TabStop = false;
            // 
            // picbox
            // 
            resources.ApplyResources(this.picbox, "picbox");
            this.picbox.BackColor = System.Drawing.Color.AliceBlue;
            this.picbox.Name = "picbox";
            this.picbox.TabStop = false;
            // 
            // FontBrowserPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.picboxPreview);
            this.Controls.Add(this.txtFontName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnRender);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lbFontRenders);
            this.Controls.Add(this.cmbStyle);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbSize);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtEntered);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.picbox);
            this.Name = "FontBrowserPanel";
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.picboxPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picbox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picbox;
        private System.Windows.Forms.Label label2;
        private AutomateControls.Textboxes.StyledTextBox txtEntered;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbSize;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbStyle;
        private System.Windows.Forms.ListBox lbFontRenders;
        private System.Windows.Forms.Label label7;
        private AutomateControls.Buttons.StandardStyledButton btnRender;
        private System.Windows.Forms.Label label3;
        private AutomateControls.Textboxes.StyledTextBox txtFontName;
        private System.Windows.Forms.PictureBox picboxPreview;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
    }
}
