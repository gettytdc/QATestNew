namespace BluePrism.Setup.Forms
{
    partial class DiskUsageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiskUsageForm));
            this.BorderPanel = new System.Windows.Forms.Panel();
            this.ErrorDetailsBtn = new System.Windows.Forms.Button();
            this.lblAvailabe = new System.Windows.Forms.Label();
            this.lblDiskSize = new System.Windows.Forms.Label();
            this.lblRequired = new System.Windows.Forms.Label();
            this.lblVolume = new System.Windows.Forms.Label();
            this.ListPanel = new System.Windows.Forms.Panel();
            this.ExitButton = new System.Windows.Forms.Button();
            this.Subtitle = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.NextButton = new System.Windows.Forms.Button();
            this.BorderPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.BackColor = System.Drawing.Color.White;
            this.BorderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BorderPanel.Controls.Add(this.ErrorDetailsBtn);
            this.BorderPanel.Controls.Add(this.lblAvailabe);
            this.BorderPanel.Controls.Add(this.lblDiskSize);
            this.BorderPanel.Controls.Add(this.lblRequired);
            this.BorderPanel.Controls.Add(this.lblVolume);
            this.BorderPanel.Controls.Add(this.ListPanel);
            this.BorderPanel.Controls.Add(this.ExitButton);
            this.BorderPanel.Controls.Add(this.Subtitle);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.Add(this.NextButton);
            resources.ApplyResources(this.BorderPanel, "BorderPanel");
            this.BorderPanel.Name = "BorderPanel";
            this.BorderPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BorderPanel_MouseDown);
            this.BorderPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BorderPanel_MouseMove);
            this.BorderPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BorderPanel_MouseUp);
            // 
            // ErrorDetailsBtn
            // 
            this.ErrorDetailsBtn.BackColor = System.Drawing.Color.White;
            this.ErrorDetailsBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.ErrorDetailsBtn, "ErrorDetailsBtn");
            this.ErrorDetailsBtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.ErrorDetailsBtn.Name = "ErrorDetailsBtn";
            this.ErrorDetailsBtn.UseVisualStyleBackColor = false;
            this.ErrorDetailsBtn.Click += new System.EventHandler(this.ErrorDetailsBtn_Click);
            // 
            // lblAvailabe
            // 
            resources.ApplyResources(this.lblAvailabe, "lblAvailabe");
            this.lblAvailabe.Name = "lblAvailabe";
            // 
            // lblDiskSize
            // 
            resources.ApplyResources(this.lblDiskSize, "lblDiskSize");
            this.lblDiskSize.Name = "lblDiskSize";
            // 
            // lblRequired
            // 
            resources.ApplyResources(this.lblRequired, "lblRequired");
            this.lblRequired.Name = "lblRequired";
            // 
            // lblVolume
            // 
            resources.ApplyResources(this.lblVolume, "lblVolume");
            this.lblVolume.Name = "lblVolume";
            // 
            // ListPanel
            // 
            resources.ApplyResources(this.ListPanel, "ListPanel");
            this.ListPanel.Name = "ListPanel";
            // 
            // ExitButton
            // 
            this.ExitButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.ExitButton.FlatAppearance.BorderSize = 0;
            this.ExitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.ExitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.ExitButton, "ExitButton");
            this.ExitButton.ForeColor = System.Drawing.Color.White;
            this.ExitButton.Image = global::BluePrism.Setup.Properties.Resources.cross_blue;
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // Subtitle
            // 
            this.Subtitle.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.Subtitle.BackColor = System.Drawing.Color.White;
            this.Subtitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Subtitle.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.Subtitle, "Subtitle");
            this.Subtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.Subtitle.Name = "Subtitle";
            this.Subtitle.ReadOnly = true;
            this.Subtitle.SelectionEnabled = false;
            // 
            // Title
            // 
            resources.ApplyResources(this.Title, "Title");
            this.Title.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.Title.BackColor = System.Drawing.Color.White;
            this.Title.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Title.Cursor = System.Windows.Forms.Cursors.Default;
            this.Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.Title.Name = "Title";
            this.Title.ReadOnly = true;
            this.Title.SelectionEnabled = false;
            // 
            // NextButton
            // 
            this.NextButton.BackColor = System.Drawing.Color.White;
            this.NextButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.NextButton, "NextButton");
            this.NextButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.NextButton.Name = "NextButton";
            this.NextButton.UseVisualStyleBackColor = false;
            this.NextButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // DiskUsageForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.BorderPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DiskUsageForm";
            this.Load += new System.EventHandler(this.DiskUsageForm_Load);
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel BorderPanel;
        private System.Windows.Forms.Panel ListPanel;
        private System.Windows.Forms.Button ExitButton;
        private Controls.BluePrismReadOnlyTextBox Subtitle;
        private Controls.BluePrismReadOnlyTextBox Title;
        internal System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Label lblAvailabe;
        private System.Windows.Forms.Label lblDiskSize;
        private System.Windows.Forms.Label lblRequired;
        private System.Windows.Forms.Label lblVolume;
        internal System.Windows.Forms.Button ErrorDetailsBtn;
    }
}