namespace BluePrism.Api.Setup.Dialogs
{
    using System.Windows.Forms;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenceDialog));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topBorder = new System.Windows.Forms.Panel();
            this.agreement = new System.Windows.Forms.RichTextBox();
            this.topPanel = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.titleLbl = new System.Windows.Forms.Label();
            this.banner = new System.Windows.Forms.PictureBox();
            this.accepted = new System.Windows.Forms.CheckBox();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cancel = new System.Windows.Forms.Button();
            this.print = new System.Windows.Forms.Button();
            this.back = new System.Windows.Forms.Button();
            this.next = new System.Windows.Forms.Button();
            this.border1 = new System.Windows.Forms.Panel();
            this.middlePanel = new System.Windows.Forms.Panel();
            this.contextMenuStrip1.SuspendLayout();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.banner)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.middlePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // topBorder
            // 
            resources.ApplyResources(this.topBorder, "topBorder");
            this.topBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.topBorder.Name = "topBorder";
            // 
            // agreement
            // 
            resources.ApplyResources(this.agreement, "agreement");
            this.agreement.BackColor = System.Drawing.Color.White;
            this.agreement.ContextMenuStrip = this.contextMenuStrip1;
            this.agreement.Name = "agreement";
            this.agreement.ReadOnly = true;
            // 
            // topPanel
            // 
            resources.ApplyResources(this.topPanel, "topPanel");
            this.topPanel.Controls.Add(this.label2);
            this.topPanel.Controls.Add(this.titleLbl);
            this.topPanel.Controls.Add(this.banner);
            this.topPanel.Name = "topPanel";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Name = "label2";
            // 
            // titleLbl
            // 
            resources.ApplyResources(this.titleLbl, "titleLbl");
            this.titleLbl.BackColor = System.Drawing.Color.Transparent;
            this.titleLbl.Name = "titleLbl";
            // 
            // banner
            // 
            resources.ApplyResources(this.banner, "banner");
            this.banner.BackColor = System.Drawing.Color.White;
            this.banner.Name = "banner";
            this.banner.TabStop = false;
            // 
            // accepted
            // 
            resources.ApplyResources(this.accepted, "accepted");
            this.accepted.BackColor = System.Drawing.Color.Transparent;
            this.accepted.Name = "accepted";
            this.accepted.UseVisualStyleBackColor = false;
            this.accepted.CheckedChanged += new System.EventHandler(this.accepted_CheckedChanged);
            // 
            // bottomPanel
            // 
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.BackColor = System.Drawing.SystemColors.Control;
            this.bottomPanel.Controls.Add(this.tableLayoutPanel1);
            this.bottomPanel.Controls.Add(this.border1);
            this.bottomPanel.Name = "bottomPanel";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.cancel, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.print, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.back, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.next, 3, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // cancel
            // 
            resources.ApplyResources(this.cancel, "cancel");
            this.cancel.Name = "cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // print
            // 
            resources.ApplyResources(this.print, "print");
            this.print.Name = "print";
            this.print.UseVisualStyleBackColor = true;
            this.print.Click += new System.EventHandler(this.print_Click);
            // 
            // back
            // 
            resources.ApplyResources(this.back, "back");
            this.back.Name = "back";
            this.back.UseVisualStyleBackColor = true;
            this.back.Click += new System.EventHandler(this.back_Click);
            // 
            // next
            // 
            resources.ApplyResources(this.next, "next");
            this.next.Name = "next";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new System.EventHandler(this.next_Click);
            // 
            // border1
            // 
            this.border1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.border1, "border1");
            this.border1.Name = "border1";
            // 
            // middlePanel
            // 
            resources.ApplyResources(this.middlePanel, "middlePanel");
            this.middlePanel.Controls.Add(this.agreement);
            this.middlePanel.Controls.Add(this.accepted);
            this.middlePanel.Name = "middlePanel";
            // 
            // LicenceDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.middlePanel);
            this.Controls.Add(this.topBorder);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.bottomPanel);
            this.Name = "LicenceDialog";
            this.Load += new System.EventHandler(this.LicenceDialog_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.banner)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.middlePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        Panel bottomPanel;
        Button back;
        Button next;
        CheckBox accepted;
        Panel topPanel;
        Label label2;
        Label titleLbl;
        PictureBox banner;
        RichTextBox agreement;
        Button cancel;

        #endregion
        private Button print;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem copyToolStripMenuItem;
        private Panel border1;
        private Panel topBorder;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel middlePanel;
    }
}