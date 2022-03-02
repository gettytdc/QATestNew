namespace BluePrism.Setup
{
    partial class BaseDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseDialog));
            this.BorderPanel = new System.Windows.Forms.Panel();
            this.BackButton = new System.Windows.Forms.Button();
            this.BackLabel = new System.Windows.Forms.Label();
            this.ExitButton = new System.Windows.Forms.Button();
            this.PrismTitle = new System.Windows.Forms.PictureBox();
            this.BorderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrismTitle)).BeginInit();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BorderPanel.Controls.Add(this.BackButton);
            this.BorderPanel.Controls.Add(this.BackLabel);
            this.BorderPanel.Controls.Add(this.ExitButton);
            this.BorderPanel.Controls.Add(this.PrismTitle);
            resources.ApplyResources(this.BorderPanel, "BorderPanel");
            this.BorderPanel.Name = "BorderPanel";
            this.BorderPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BorderPanel_MouseDown);
            this.BorderPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BorderPanel_MouseMove);
            // 
            // BackButton
            // 
            this.BackButton.FlatAppearance.BorderSize = 0;
            this.BackButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.BackButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.BackButton, "BackButton");
            this.BackButton.Image = global::BluePrism.Setup.Properties.Resources.arrow_left;
            this.BackButton.Name = "BackButton";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.Back_Click);
            // 
            // BackLabel
            // 
            resources.ApplyResources(this.BackLabel, "BackLabel");
            this.BackLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.BackLabel.Name = "BackLabel";
            this.BackLabel.Click += new System.EventHandler(this.Back_Click);
            // 
            // ExitButton
            // 
            this.ExitButton.FlatAppearance.BorderSize = 0;
            this.ExitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.ExitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.ExitButton, "ExitButton");
            this.ExitButton.Image = global::BluePrism.Setup.Properties.Resources.cross_blue;
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // PrismTitle
            // 
            this.PrismTitle.Image = global::BluePrism.Setup.Properties.Resources.prism_full;
            resources.ApplyResources(this.PrismTitle, "PrismTitle");
            this.PrismTitle.Name = "PrismTitle";
            this.PrismTitle.TabStop = false;
            // 
            // BaseDialog
            // 
            this.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.BorderPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "BaseDialog";
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrismTitle)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.PictureBox PrismTitle;
        protected System.Windows.Forms.Panel BorderPanel;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Label BackLabel;
    }
}