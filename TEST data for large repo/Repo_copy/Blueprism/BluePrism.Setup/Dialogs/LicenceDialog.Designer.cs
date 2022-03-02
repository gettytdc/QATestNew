using System.Windows.Forms;

using WixSharp;
using WixSharp.UI.Forms;

namespace BluePrism.Setup.Dialogs
{
    partial class LicenceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenceDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.Agreement = new BluePrism.Setup.Controls.BluePrismReadOnlyRichTextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.NextButton = new System.Windows.Forms.Button();
            this.Accepted = new System.Windows.Forms.CheckBox();
            this.Subtitle = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.BorderPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.Controls.Add(this.panel1);
            this.BorderPanel.Controls.Add(this.panel2);
            this.BorderPanel.Controls.Add(this.NextButton);
            this.BorderPanel.Controls.Add(this.Accepted);
            this.BorderPanel.Controls.Add(this.Subtitle);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.SetChildIndex(this.Title, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Accepted, 0);
            this.BorderPanel.Controls.SetChildIndex(this.NextButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.panel2, 0);
            this.BorderPanel.Controls.SetChildIndex(this.panel1, 0);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.Agreement);
            this.panel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // Agreement
            // 
            resources.ApplyResources(this.Agreement, "Agreement");
            this.Agreement.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.Agreement.BackColor = System.Drawing.Color.White;
            this.Agreement.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Agreement.ContentsCopied = false;
            this.Agreement.Cursor = System.Windows.Forms.Cursors.Default;
            this.Agreement.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.Agreement.Name = "Agreement";
            this.Agreement.ReadOnly = true;
            this.Agreement.SelectionEnabled = true;
            this.Agreement.TextPadding = new System.Windows.Forms.Padding(56, 23, 56, 8);
            this.Agreement.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.Agreement_LinkClicked);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // NextButton
            // 
            this.NextButton.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.NextButton, "NextButton");
            this.NextButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.NextButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.NextButton.Name = "NextButton";
            this.NextButton.UseVisualStyleBackColor = false;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // Accepted
            // 
            resources.ApplyResources(this.Accepted, "Accepted");
            this.Accepted.BackColor = System.Drawing.Color.White;
            this.Accepted.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.Accepted.Name = "Accepted";
            this.Accepted.UseVisualStyleBackColor = false;
            this.Accepted.CheckedChanged += new System.EventHandler(this.Accepted_CheckedChanged);
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
            // LicenceDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "LicenceDialog";
            this.Load += new System.EventHandler(this.LicenceDialog_Load);
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel panel1;
        private Controls.BluePrismReadOnlyRichTextBox Agreement;
        private Panel panel2;
        internal Button NextButton;
        private CheckBox Accepted;
        private Controls.BluePrismReadOnlyTextBox Subtitle;
        private Controls.BluePrismReadOnlyTextBox Title;
    }
}