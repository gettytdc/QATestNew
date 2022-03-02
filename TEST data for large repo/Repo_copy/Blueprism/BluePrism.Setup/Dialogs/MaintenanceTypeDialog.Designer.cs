using WixSharp;
using WixSharp.UI.Forms;

namespace BluePrism.Setup.Dialogs
{
    partial class MaintenanceTypeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MaintenanceTypeDialog));
            this.ChooseLanguagePictureBox = new System.Windows.Forms.PictureBox();
            this.ChooseLanguageLinkLabel = new System.Windows.Forms.LinkLabel();
            this.RemoveLink = new System.Windows.Forms.LinkLabel();
            this.RepairLink = new System.Windows.Forms.LinkLabel();
            this.ChangeLink = new System.Windows.Forms.LinkLabel();
            this.CancelButton = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.Subtitle1 = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Subtitle2 = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Subtitle3 = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.BorderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ChooseLanguagePictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.Controls.Add(this.Subtitle3);
            this.BorderPanel.Controls.Add(this.Subtitle2);
            this.BorderPanel.Controls.Add(this.ChooseLanguagePictureBox);
            this.BorderPanel.Controls.Add(this.ChooseLanguageLinkLabel);
            this.BorderPanel.Controls.Add(this.RemoveLink);
            this.BorderPanel.Controls.Add(this.RepairLink);
            this.BorderPanel.Controls.Add(this.ChangeLink);
            this.BorderPanel.Controls.Add(this.CancelButton);
            this.BorderPanel.Controls.Add(this.panel3);
            this.BorderPanel.Controls.Add(this.panel4);
            this.BorderPanel.Controls.Add(this.panel2);
            this.BorderPanel.Controls.Add(this.Subtitle1);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.SetChildIndex(this.Title, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle1, 0);
            this.BorderPanel.Controls.SetChildIndex(this.panel2, 0);
            this.BorderPanel.Controls.SetChildIndex(this.panel4, 0);
            this.BorderPanel.Controls.SetChildIndex(this.panel3, 0);
            this.BorderPanel.Controls.SetChildIndex(this.CancelButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.ChangeLink, 0);
            this.BorderPanel.Controls.SetChildIndex(this.RepairLink, 0);
            this.BorderPanel.Controls.SetChildIndex(this.RemoveLink, 0);
            this.BorderPanel.Controls.SetChildIndex(this.ChooseLanguageLinkLabel, 0);
            this.BorderPanel.Controls.SetChildIndex(this.ChooseLanguagePictureBox, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle2, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle3, 0);
            // 
            // ChooseLanguagePictureBox
            // 
            this.ChooseLanguagePictureBox.Image = global::BluePrism.Setup.Properties.Resources.icon_colour_2x;
            resources.ApplyResources(this.ChooseLanguagePictureBox, "ChooseLanguagePictureBox");
            this.ChooseLanguagePictureBox.Name = "ChooseLanguagePictureBox";
            this.ChooseLanguagePictureBox.TabStop = false;
            this.ChooseLanguagePictureBox.Click += new System.EventHandler(this.ChooseLanguagePictureBox_Click);
            // 
            // ChooseLanguageLinkLabel
            // 
            resources.ApplyResources(this.ChooseLanguageLinkLabel, "ChooseLanguageLinkLabel");
            this.ChooseLanguageLinkLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.ChooseLanguageLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.ChooseLanguageLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.ChooseLanguageLinkLabel.Name = "ChooseLanguageLinkLabel";
            this.ChooseLanguageLinkLabel.TabStop = true;
            this.ChooseLanguageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ChooseLanguageLinkLabel_LinkClicked);
            // 
            // RemoveLink
            // 
            resources.ApplyResources(this.RemoveLink, "RemoveLink");
            this.RemoveLink.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.RemoveLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.RemoveLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.RemoveLink.Name = "RemoveLink";
            this.RemoveLink.TabStop = true;
            this.RemoveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RemoveLink_LinkClicked);
            // 
            // RepairLink
            // 
            resources.ApplyResources(this.RepairLink, "RepairLink");
            this.RepairLink.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.RepairLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.RepairLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.RepairLink.Name = "RepairLink";
            this.RepairLink.TabStop = true;
            this.RepairLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RepairLink_LinkClicked);
            // 
            // ChangeLink
            // 
            resources.ApplyResources(this.ChangeLink, "ChangeLink");
            this.ChangeLink.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.ChangeLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.ChangeLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.ChangeLink.Name = "ChangeLink";
            this.ChangeLink.TabStop = true;
            this.ChangeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ChangeLink_LinkClicked);
            // 
            // CancelButton
            // 
            this.CancelButton.BackColor = System.Drawing.Color.White;
            this.CancelButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.CancelButton, "CancelButton");
            this.CancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.UseVisualStyleBackColor = false;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // Subtitle1
            // 
            this.Subtitle1.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.Subtitle1.BackColor = System.Drawing.Color.White;
            this.Subtitle1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Subtitle1.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.Subtitle1, "Subtitle1");
            this.Subtitle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.Subtitle1.Name = "Subtitle1";
            this.Subtitle1.ReadOnly = true;
            this.Subtitle1.SelectionEnabled = false;
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
            // Subtitle2
            // 
            this.Subtitle2.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.Subtitle2.BackColor = System.Drawing.Color.White;
            this.Subtitle2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Subtitle2.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.Subtitle2, "Subtitle2");
            this.Subtitle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.Subtitle2.Name = "Subtitle2";
            this.Subtitle2.ReadOnly = true;
            this.Subtitle2.SelectionEnabled = false;
            // 
            // Subtitle3
            // 
            this.Subtitle3.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.Subtitle3.BackColor = System.Drawing.Color.White;
            this.Subtitle3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Subtitle3.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.Subtitle3, "Subtitle3");
            this.Subtitle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.Subtitle3.Name = "Subtitle3";
            this.Subtitle3.ReadOnly = true;
            this.Subtitle3.SelectionEnabled = false;
            // 
            // MaintenanceTypeDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "MaintenanceTypeDialog";
            this.Load += new System.EventHandler(this.MaintenanceTypeDialog_Load);
            this.Shown += new System.EventHandler(this.MaintenanceTypeDialog_Shown);
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ChooseLanguagePictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox ChooseLanguagePictureBox;
        private System.Windows.Forms.LinkLabel ChooseLanguageLinkLabel;
        private System.Windows.Forms.LinkLabel RemoveLink;
        private System.Windows.Forms.LinkLabel RepairLink;
        private System.Windows.Forms.LinkLabel ChangeLink;
        internal System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel2;
        private Controls.BluePrismReadOnlyTextBox Subtitle1;
        private Controls.BluePrismReadOnlyTextBox Title;
        private Controls.BluePrismReadOnlyTextBox Subtitle2;
        private Controls.BluePrismReadOnlyTextBox Subtitle3;
    }
}