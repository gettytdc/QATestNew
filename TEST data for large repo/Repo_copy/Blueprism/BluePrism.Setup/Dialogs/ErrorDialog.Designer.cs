namespace BluePrism.Setup.Dialogs
{
    partial class ErrorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorDialog));
            this.CopyLink = new System.Windows.Forms.LinkLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.CloseButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ErrorsText = new BluePrism.Setup.Controls.BluePrismReadOnlyRichTextBox();
            this.Subtitle = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.BorderPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.Controls.Add(this.CopyLink);
            this.BorderPanel.Controls.Add(this.panel2);
            this.BorderPanel.Controls.Add(this.CloseButton);
            this.BorderPanel.Controls.Add(this.panel1);
            this.BorderPanel.Controls.Add(this.Subtitle);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.SetChildIndex(this.Title, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle, 0);
            this.BorderPanel.Controls.SetChildIndex(this.panel1, 0);
            this.BorderPanel.Controls.SetChildIndex(this.CloseButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.panel2, 0);
            this.BorderPanel.Controls.SetChildIndex(this.CopyLink, 0);
            // 
            // CopyLink
            // 
            this.CopyLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            resources.ApplyResources(this.CopyLink, "CopyLink");
            this.CopyLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.CopyLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.CopyLink.Name = "CopyLink";
            this.CopyLink.TabStop = true;
            this.CopyLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CopyLink_LinkClicked);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // CloseButton
            // 
            this.CloseButton.BackColor = System.Drawing.Color.White;
            this.CloseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.CloseButton, "CloseButton");
            this.CloseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.UseVisualStyleBackColor = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.ErrorsText);
            this.panel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // ErrorsText
            // 
            resources.ApplyResources(this.ErrorsText, "ErrorsText");
            this.ErrorsText.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.ErrorsText.BackColor = System.Drawing.Color.White;
            this.ErrorsText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ErrorsText.ContentsCopied = false;
            this.ErrorsText.Cursor = System.Windows.Forms.Cursors.Default;
            this.ErrorsText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.ErrorsText.Name = "ErrorsText";
            this.ErrorsText.ReadOnly = true;
            this.ErrorsText.SelectionEnabled = true;
            this.ErrorsText.TextPadding = new System.Windows.Forms.Padding(37, 15, 37, 15);
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
            // ErrorDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "ErrorDialog";
            this.Load += new System.EventHandler(this.Dialog_Load);
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel CopyLink;
        private System.Windows.Forms.Panel panel2;
        internal System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Panel panel1;
        private Controls.BluePrismReadOnlyRichTextBox ErrorsText;
        private Controls.BluePrismReadOnlyTextBox Subtitle;
        private Controls.BluePrismReadOnlyTextBox Title;
    }
}