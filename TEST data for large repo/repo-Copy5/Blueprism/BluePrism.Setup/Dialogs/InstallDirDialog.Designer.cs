using WixSharp;
using WixSharp.UI.Forms;

namespace BluePrism.Setup.Dialogs
{
    partial class InstallDirDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallDirDialog));
            this.ChangeDirLabel = new System.Windows.Forms.Label();
            this.AdvancedInstall = new System.Windows.Forms.CheckBox();
            this.InstallPathPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.InstallDir = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Subtitle = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.NextButton = new System.Windows.Forms.Button();
            this.BorderPanel.SuspendLayout();
            this.InstallPathPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.Controls.Add(this.ChangeDirLabel);
            this.BorderPanel.Controls.Add(this.AdvancedInstall);
            this.BorderPanel.Controls.Add(this.InstallPathPanel);
            this.BorderPanel.Controls.Add(this.Subtitle);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.Add(this.NextButton);
            this.BorderPanel.Controls.SetChildIndex(this.NextButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Title, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle, 0);
            this.BorderPanel.Controls.SetChildIndex(this.InstallPathPanel, 0);
            this.BorderPanel.Controls.SetChildIndex(this.AdvancedInstall, 0);
            this.BorderPanel.Controls.SetChildIndex(this.ChangeDirLabel, 0);
            // 
            // ChangeDirLabel
            // 
            resources.ApplyResources(this.ChangeDirLabel, "ChangeDirLabel");
            this.ChangeDirLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.ChangeDirLabel.Name = "ChangeDirLabel";
            this.ChangeDirLabel.Click += new System.EventHandler(this.ChangeDirLabel_Click);
            // 
            // AdvancedInstall
            // 
            resources.ApplyResources(this.AdvancedInstall, "AdvancedInstall");
            this.AdvancedInstall.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.AdvancedInstall.Name = "AdvancedInstall";
            this.AdvancedInstall.UseVisualStyleBackColor = true;
            this.AdvancedInstall.CheckedChanged += new System.EventHandler(this.AdvancedInstall_CheckedChanged);
            // 
            // InstallPathPanel
            // 
            this.InstallPathPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.InstallPathPanel.Controls.Add(this.InstallDir);
            resources.ApplyResources(this.InstallPathPanel, "InstallPathPanel");
            this.InstallPathPanel.Name = "InstallPathPanel";
            // 
            // InstallDir
            // 
            this.InstallDir.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.InstallDir.BackColor = System.Drawing.Color.White;
            this.InstallDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InstallDir.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.InstallDir, "InstallDir");
            this.InstallDir.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.InstallDir.Name = "InstallDir";
            this.InstallDir.ReadOnly = true;
            this.InstallDir.SelectionEnabled = false;
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
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // InstallDirDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "InstallDirDialog";
            this.Load += new System.EventHandler(this.InstallDirDialog_Load);
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            this.InstallPathPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label ChangeDirLabel;
        private System.Windows.Forms.CheckBox AdvancedInstall;
        private System.Windows.Forms.FlowLayoutPanel InstallPathPanel;
        private Controls.BluePrismReadOnlyTextBox InstallDir;
        private Controls.BluePrismReadOnlyTextBox Subtitle;
        private Controls.BluePrismReadOnlyTextBox Title;
        internal System.Windows.Forms.Button NextButton;
    }
}