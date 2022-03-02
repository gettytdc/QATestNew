using WixSharp;
using WixSharp.UI.Forms;

namespace BluePrism.Setup.Dialogs
{
    partial class ProgressDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDialog));
            this.WaitPrompt = new System.Windows.Forms.Label();
            this.OpenPortalButton = new System.Windows.Forms.Button();
            this.CurrentAction = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.StatusLabel = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.ProgressBar = new BluePrism.Setup.Controls.BluePrismColorProgressBar();
            this.PrismImage = new System.Windows.Forms.PictureBox();
            this.Subtitle = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.WaitButton = new System.Windows.Forms.Button();
            this.BorderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrismImage)).BeginInit();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.Controls.Add(this.ProgressBar);
            this.BorderPanel.Controls.Add(this.WaitPrompt);
            this.BorderPanel.Controls.Add(this.OpenPortalButton);
            this.BorderPanel.Controls.Add(this.CurrentAction);
            this.BorderPanel.Controls.Add(this.StatusLabel);
            this.BorderPanel.Controls.Add(this.PrismImage);
            this.BorderPanel.Controls.Add(this.Subtitle);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.Add(this.WaitButton);
            this.BorderPanel.Controls.SetChildIndex(this.WaitButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Title, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle, 0);
            this.BorderPanel.Controls.SetChildIndex(this.PrismImage, 0);
            this.BorderPanel.Controls.SetChildIndex(this.StatusLabel, 0);
            this.BorderPanel.Controls.SetChildIndex(this.CurrentAction, 0);
            this.BorderPanel.Controls.SetChildIndex(this.OpenPortalButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.WaitPrompt, 0);
            this.BorderPanel.Controls.SetChildIndex(this.ProgressBar, 0);
            // 
            // WaitPrompt
            // 
            resources.ApplyResources(this.WaitPrompt, "WaitPrompt");
            this.WaitPrompt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.WaitPrompt.Name = "WaitPrompt";
            this.WaitPrompt.TabStop = true;
            // 
            // OpenPortalButton
            // 
            this.OpenPortalButton.BackColor = System.Drawing.Color.White;
            this.OpenPortalButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.OpenPortalButton, "OpenPortalButton");
            this.OpenPortalButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.OpenPortalButton.Name = "OpenPortalButton";
            this.OpenPortalButton.UseVisualStyleBackColor = false;
            this.OpenPortalButton.Click += new System.EventHandler(this.OpenPortalButton_Click);
            // 
            // CurrentAction
            // 
            this.CurrentAction.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.CurrentAction.BackColor = System.Drawing.Color.White;
            this.CurrentAction.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CurrentAction.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.CurrentAction, "CurrentAction");
            this.CurrentAction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.CurrentAction.Name = "CurrentAction";
            this.CurrentAction.ReadOnly = true;
            this.CurrentAction.SelectionEnabled = false;
            // 
            // StatusLabel
            // 
            this.StatusLabel.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.StatusLabel.BackColor = System.Drawing.Color.White;
            this.StatusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.StatusLabel.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.StatusLabel, "StatusLabel");
            this.StatusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.ReadOnly = true;
            this.StatusLabel.SelectionEnabled = false;
            // 
            // ProgressBar
            // 
            this.ProgressBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.ProgressBar, "ProgressBar");
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // PrismImage
            // 
            this.PrismImage.Image = global::BluePrism.Setup.Properties.Resources.installing_prism;
            resources.ApplyResources(this.PrismImage, "PrismImage");
            this.PrismImage.Name = "PrismImage";
            this.PrismImage.TabStop = false;
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
            // WaitButton
            // 
            this.WaitButton.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.WaitButton, "WaitButton");
            this.WaitButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.WaitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.WaitButton.Name = "WaitButton";
            this.WaitButton.UseVisualStyleBackColor = false;
            this.WaitButton.Click += new System.EventHandler(this.WaitButton_Click);
            // 
            // ProgressDialog
            // 
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Name = "ProgressDialog";
            this.Load += new System.EventHandler(this.ProgressDialog_Load);
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PrismImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label WaitPrompt;
        internal System.Windows.Forms.Button OpenPortalButton;
        private Controls.BluePrismReadOnlyTextBox CurrentAction;
        private Controls.BluePrismReadOnlyTextBox StatusLabel;
        private Controls.BluePrismColorProgressBar ProgressBar;
        private System.Windows.Forms.PictureBox PrismImage;
        private Controls.BluePrismReadOnlyTextBox Subtitle;
        private Controls.BluePrismReadOnlyTextBox Title;
        internal System.Windows.Forms.Button WaitButton;
    }
}