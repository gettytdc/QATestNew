namespace BluePrism.Setup.Dialogs
{
    partial class MessageDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageDialog));
            this.ignoreButton = new System.Windows.Forms.Button();
            this.abortButton = new System.Windows.Forms.Button();
            this.retryButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.yesButton = new System.Windows.Forms.Button();
            this.noButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.Subtitle = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.WarningText = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.PrismImage = new System.Windows.Forms.PictureBox();
            this.BorderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrismImage)).BeginInit();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.Controls.Add(this.ignoreButton);
            this.BorderPanel.Controls.Add(this.abortButton);
            this.BorderPanel.Controls.Add(this.retryButton);
            this.BorderPanel.Controls.Add(this.cancelButton);
            this.BorderPanel.Controls.Add(this.okButton);
            this.BorderPanel.Controls.Add(this.yesButton);
            this.BorderPanel.Controls.Add(this.noButton);
            this.BorderPanel.Controls.Add(this.panel2);
            this.BorderPanel.Controls.Add(this.Subtitle);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.Add(this.WarningText);
            this.BorderPanel.Controls.Add(this.PrismImage);
            this.BorderPanel.Controls.SetChildIndex(this.PrismImage, 0);
            this.BorderPanel.Controls.SetChildIndex(this.WarningText, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Title, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle, 0);
            this.BorderPanel.Controls.SetChildIndex(this.panel2, 0);
            this.BorderPanel.Controls.SetChildIndex(this.noButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.yesButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.okButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.cancelButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.retryButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.abortButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.ignoreButton, 0);
            // 
            // ignoreButton
            // 
            this.ignoreButton.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.ignoreButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.ignoreButton, "ignoreButton");
            this.ignoreButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.ignoreButton.Name = "ignoreButton";
            this.ignoreButton.Text = global::BluePrism.Setup.Properties.Resources.MessageDialogIgnoreButtonText;
            // 
            // abortButton
            // 
            this.abortButton.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.abortButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.abortButton, "abortButton");
            this.abortButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.abortButton.Name = "abortButton";
            this.abortButton.Text = global::BluePrism.Setup.Properties.Resources.MessageDialogAbortButtonText;
            // 
            // retryButton
            // 
            this.retryButton.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.retryButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.retryButton, "retryButton");
            this.retryButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.retryButton.Name = "retryButton";
            this.retryButton.Text = global::BluePrism.Setup.Properties.Resources.MessageDialogRetryButtonText;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Text = global::BluePrism.Setup.Properties.Resources.MessageDialogCancelButtonText;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.okButton.Name = "okButton";
            this.okButton.Text = global::BluePrism.Setup.Properties.Resources.MessageDialogOkButtonText;
            // 
            // yesButton
            // 
            this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.yesButton, "yesButton");
            this.yesButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.yesButton.Name = "yesButton";
            this.yesButton.Text = global::BluePrism.Setup.Properties.Resources.MessageDialogYesButtonText;
            // 
            // noButton
            // 
            this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.noButton, "noButton");
            this.noButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.noButton.Name = "noButton";
            this.noButton.Text = global::BluePrism.Setup.Properties.Resources.MessageDialogNoButtonText;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
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
            // MessageDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            resources.ApplyResources(this, "$this");
            this.Name = "MessageDialog";
            this.ShowInTaskbar = false;
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrismImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ignoreButton;
        private System.Windows.Forms.Button abortButton;
        private System.Windows.Forms.Button retryButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button yesButton;
        private System.Windows.Forms.Button noButton;
        private System.Windows.Forms.Panel panel2;
        public Controls.BluePrismReadOnlyTextBox Subtitle;
        private Controls.BluePrismReadOnlyTextBox Title;
        private Controls.BluePrismReadOnlyTextBox WarningText;
        private System.Windows.Forms.PictureBox PrismImage;
    }
}