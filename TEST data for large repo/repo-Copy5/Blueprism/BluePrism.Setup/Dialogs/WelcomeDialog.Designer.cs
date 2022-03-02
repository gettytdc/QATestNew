using WixSharp;
using WixSharp.UI.Forms;

namespace BluePrism.Setup.Dialogs
{
    partial class WelcomeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomeDialog));
            this.ChooseLanguagePictureBox = new System.Windows.Forms.PictureBox();
            this.ChooseLanguagelinkLabel = new System.Windows.Forms.LinkLabel();
            this.Subtitle = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.WarningText = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.NextButton = new System.Windows.Forms.Button();
            this.PrismImage = new System.Windows.Forms.PictureBox();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.BorderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ChooseLanguagePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PrismImage)).BeginInit();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.Controls.Add(this.ChooseLanguagePictureBox);
            this.BorderPanel.Controls.Add(this.ChooseLanguagelinkLabel);
            this.BorderPanel.Controls.Add(this.Subtitle);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.Add(this.WarningText);
            this.BorderPanel.Controls.Add(this.NextButton);
            this.BorderPanel.Controls.Add(this.PrismImage);
            this.BorderPanel.Controls.Add(this.UpdateButton);
            this.BorderPanel.Controls.SetChildIndex(this.UpdateButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.PrismImage, 0);
            this.BorderPanel.Controls.SetChildIndex(this.NextButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.WarningText, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Title, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle, 0);
            this.BorderPanel.Controls.SetChildIndex(this.ChooseLanguagelinkLabel, 0);
            this.BorderPanel.Controls.SetChildIndex(this.ChooseLanguagePictureBox, 0);
            // 
            // ChooseLanguagePictureBox
            // 
            this.ChooseLanguagePictureBox.Image = global::BluePrism.Setup.Properties.Resources.icon_colour_2x;
            resources.ApplyResources(this.ChooseLanguagePictureBox, "ChooseLanguagePictureBox");
            this.ChooseLanguagePictureBox.Name = "ChooseLanguagePictureBox";
            this.ChooseLanguagePictureBox.TabStop = false;
            this.ChooseLanguagePictureBox.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // ChooseLanguagelinkLabel
            // 
            resources.ApplyResources(this.ChooseLanguagelinkLabel, "ChooseLanguagelinkLabel");
            this.ChooseLanguagelinkLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.ChooseLanguagelinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.ChooseLanguagelinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.ChooseLanguagelinkLabel.Name = "ChooseLanguagelinkLabel";
            this.ChooseLanguagelinkLabel.TabStop = true;
            this.ChooseLanguagelinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
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
            // NextButton
            // 
            this.NextButton.BackColor = System.Drawing.Color.White;
            this.NextButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.NextButton, "NextButton");
            this.NextButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.NextButton.Name = "NextButton";
            this.NextButton.UseVisualStyleBackColor = false;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // PrismImage
            // 
            this.PrismImage.Image = global::BluePrism.Setup.Properties.Resources.start_logo;
            resources.ApplyResources(this.PrismImage, "PrismImage");
            this.PrismImage.Name = "PrismImage";
            this.PrismImage.TabStop = false;
            // 
            // UpdateButton
            // 
            this.UpdateButton.BackColor = System.Drawing.Color.White;
            this.UpdateButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.UpdateButton, "UpdateButton");
            this.UpdateButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.UseVisualStyleBackColor = false;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // WelcomeDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this, "$this");
            this.Name = "WelcomeDialog";
            this.Load += new System.EventHandler(this.WelcomeDialog_Load);
            this.Shown += new System.EventHandler(this.WelcomeDialog_Shown);
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ChooseLanguagePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PrismImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox ChooseLanguagePictureBox;
        private System.Windows.Forms.LinkLabel ChooseLanguagelinkLabel;
        public Controls.BluePrismReadOnlyTextBox Subtitle;
        private Controls.BluePrismReadOnlyTextBox Title;
        private Controls.BluePrismReadOnlyTextBox WarningText;
        internal System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.PictureBox PrismImage;
        internal System.Windows.Forms.Button UpdateButton;
    }
}