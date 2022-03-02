namespace BluePrism.Setup.Dialogs
{
    partial class VersionCheckDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VersionCheckDialog));
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.WarningText = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.PrismImage = new System.Windows.Forms.PictureBox();
            this.okButton = new System.Windows.Forms.Button();
            this.BorderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrismImage)).BeginInit();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.Controls.Add(this.okButton);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.Add(this.WarningText);
            this.BorderPanel.Controls.Add(this.PrismImage);
            this.BorderPanel.Controls.SetChildIndex(this.PrismImage, 0);
            this.BorderPanel.Controls.SetChildIndex(this.WarningText, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Title, 0);
            this.BorderPanel.Controls.SetChildIndex(this.okButton, 0);
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
            // WarningText
            // 
            resources.ApplyResources(this.WarningText, "WarningText");
            this.WarningText.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.WarningText.BackColor = System.Drawing.Color.White;
            this.WarningText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.WarningText.Cursor = System.Windows.Forms.Cursors.Default;
            this.WarningText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.WarningText.Name = "WarningText";
            this.WarningText.ReadOnly = true;
            this.WarningText.SelectionEnabled = false;
            this.WarningText.ShortcutsEnabled = false;
            // 
            // PrismImage
            // 
            this.PrismImage.Image = global::BluePrism.Setup.Properties.Resources.stop_prism;
            resources.ApplyResources(this.PrismImage, "PrismImage");
            this.PrismImage.Name = "PrismImage";
            this.PrismImage.TabStop = false;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.okButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.okButton.Name = "okButton";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // VersionCheckDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            resources.ApplyResources(this, "$this");
            this.Name = "VersionCheckDialog";
            this.ShowInTaskbar = false;
            this.Shown += new System.EventHandler(this.VersionCheckDialog_Shown);
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrismImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Controls.BluePrismReadOnlyTextBox Title;
        private Controls.BluePrismReadOnlyTextBox WarningText;
        private System.Windows.Forms.PictureBox PrismImage;
        private System.Windows.Forms.Button okButton;
    }
}