using System.Windows.Forms;

using WixSharp;
using WixSharp.UI.Forms;

namespace BluePrism.Setup.Dialogs
{
    partial class FeaturesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FeaturesDialog));
            this.NextButton = new System.Windows.Forms.Button();
            this.OuterPanel = new System.Windows.Forms.Panel();
            this.InnerPanel = new System.Windows.Forms.Panel();
            this.FeaturesToInstallLbl = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.featuresTree = new BluePrism.Setup.Controls.BluePrismTreeView();
            this.Subtitle = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.Title = new BluePrism.Setup.Controls.BluePrismReadOnlyTextBox();
            this.DiskUsageButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.BorderPanel.SuspendLayout();
            this.OuterPanel.SuspendLayout();
            this.InnerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BorderPanel
            // 
            this.BorderPanel.Controls.Add(this.btnHelp);
            this.BorderPanel.Controls.Add(this.ResetButton);
            this.BorderPanel.Controls.Add(this.DiskUsageButton);
            this.BorderPanel.Controls.Add(this.NextButton);
            this.BorderPanel.Controls.Add(this.OuterPanel);
            this.BorderPanel.Controls.Add(this.Subtitle);
            this.BorderPanel.Controls.Add(this.Title);
            this.BorderPanel.Controls.SetChildIndex(this.Title, 0);
            this.BorderPanel.Controls.SetChildIndex(this.Subtitle, 0);
            this.BorderPanel.Controls.SetChildIndex(this.OuterPanel, 0);
            this.BorderPanel.Controls.SetChildIndex(this.NextButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.DiskUsageButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.ResetButton, 0);
            this.BorderPanel.Controls.SetChildIndex(this.btnHelp, 0);
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
            // OuterPanel
            // 
            this.OuterPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(117)))), ((int)(((byte)(183)))));
            this.OuterPanel.Controls.Add(this.InnerPanel);
            resources.ApplyResources(this.OuterPanel, "OuterPanel");
            this.OuterPanel.Name = "OuterPanel";
            // 
            // InnerPanel
            // 
            this.InnerPanel.BackColor = System.Drawing.Color.White;
            this.InnerPanel.Controls.Add(this.FeaturesToInstallLbl);
            this.InnerPanel.Controls.Add(this.featuresTree);
            resources.ApplyResources(this.InnerPanel, "InnerPanel");
            this.InnerPanel.Name = "InnerPanel";
            // 
            // FeaturesToInstallLbl
            // 
            this.FeaturesToInstallLbl.AccessibleRole = System.Windows.Forms.AccessibleRole.StaticText;
            this.FeaturesToInstallLbl.BackColor = System.Drawing.Color.White;
            this.FeaturesToInstallLbl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FeaturesToInstallLbl.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.FeaturesToInstallLbl, "FeaturesToInstallLbl");
            this.FeaturesToInstallLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.FeaturesToInstallLbl.Name = "FeaturesToInstallLbl";
            this.FeaturesToInstallLbl.ReadOnly = true;
            this.FeaturesToInstallLbl.SelectionEnabled = false;
            // 
            // featuresTree
            // 
            resources.ApplyResources(this.featuresTree, "featuresTree");
            this.featuresTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.featuresTree.CheckBoxes = true;
            this.featuresTree.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(74)))), ((int)(((byte)(79)))));
            this.featuresTree.Name = "featuresTree";
            this.featuresTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.FeaturesTree_AfterCheck);
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
            // DiskUsageButton
            // 
            this.DiskUsageButton.BackColor = System.Drawing.Color.White;
            this.DiskUsageButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.DiskUsageButton, "DiskUsageButton");
            this.DiskUsageButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.DiskUsageButton.Name = "DiskUsageButton";
            this.DiskUsageButton.UseVisualStyleBackColor = false;
            this.DiskUsageButton.Click += new System.EventHandler(this.DiskUsageButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.BackColor = System.Drawing.Color.White;
            this.ResetButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.ResetButton, "ResetButton");
            this.ResetButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.UseVisualStyleBackColor = false;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.BackColor = System.Drawing.Color.White;
            this.btnHelp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            resources.ApplyResources(this.btnHelp, "btnHelp");
            this.btnHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(126)))), ((int)(((byte)(194)))));
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // FeaturesDialog
            // 
            resources.ApplyResources(this, "$this");
            this.KeyPreview = true;
            this.Name = "FeaturesDialog";
            this.Load += new System.EventHandler(this.FeaturesDialog_Load);
            this.BorderPanel.ResumeLayout(false);
            this.BorderPanel.PerformLayout();
            this.OuterPanel.ResumeLayout(false);
            this.InnerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal Button NextButton;
        private Panel OuterPanel;
        private Panel InnerPanel;
        private Controls.BluePrismReadOnlyTextBox FeaturesToInstallLbl;
        private BluePrism.Setup.Controls.BluePrismTreeView featuresTree;
        private Controls.BluePrismReadOnlyTextBox Subtitle;
        private Controls.BluePrismReadOnlyTextBox Title;
        internal Button DiskUsageButton;
        internal Button ResetButton;
        internal Button btnHelp;
    }
}