namespace BluePrism.Api.Setup.Dialogs
{
    partial class ExitDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExitDialog));
            this.imgPanel = new System.Windows.Forms.Panel();
            this.textPanel = new System.Windows.Forms.Panel();
            this.title = new System.Windows.Forms.Label();
            this.description = new System.Windows.Forms.Label();
            this.image = new System.Windows.Forms.PictureBox();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.viewLog = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.back = new System.Windows.Forms.Button();
            this.next = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.border1 = new System.Windows.Forms.Panel();
            this.textPanelFailure = new System.Windows.Forms.Panel();
            this.failureTitle = new System.Windows.Forms.Label();
            this.failureDescription = new System.Windows.Forms.Label();
            this.textPanelCancel = new System.Windows.Forms.Panel();
            this.cancelText = new System.Windows.Forms.Label();
            this.cancelDescription = new System.Windows.Forms.Label();
            this.imgPanel.SuspendLayout();
            this.textPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.image)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.textPanelFailure.SuspendLayout();
            this.textPanelCancel.SuspendLayout();
            this.SuspendLayout();
            // 
            // imgPanel
            // 
            resources.ApplyResources(this.imgPanel, "imgPanel");
            this.imgPanel.Controls.Add(this.textPanelCancel);
            this.imgPanel.Controls.Add(this.textPanelFailure);
            this.imgPanel.Controls.Add(this.textPanel);
            this.imgPanel.Controls.Add(this.image);
            this.imgPanel.Name = "imgPanel";
            // 
            // textPanel
            // 
            this.textPanel.Controls.Add(this.title);
            this.textPanel.Controls.Add(this.description);
            resources.ApplyResources(this.textPanel, "textPanel");
            this.textPanel.Name = "textPanel";
            // 
            // title
            // 
            resources.ApplyResources(this.title, "title");
            this.title.BackColor = System.Drawing.Color.Transparent;
            this.title.Name = "title";
            // 
            // description
            // 
            resources.ApplyResources(this.description, "description");
            this.description.BackColor = System.Drawing.Color.Transparent;
            this.description.Name = "description";
            // 
            // image
            // 
            resources.ApplyResources(this.image, "image");
            this.image.Name = "image";
            this.image.TabStop = false;
            // 
            // bottomPanel
            // 
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.BackColor = System.Drawing.SystemColors.Control;
            this.bottomPanel.Controls.Add(this.viewLog);
            this.bottomPanel.Controls.Add(this.tableLayoutPanel1);
            this.bottomPanel.Controls.Add(this.border1);
            this.bottomPanel.Name = "bottomPanel";
            // 
            // viewLog
            // 
            resources.ApplyResources(this.viewLog, "viewLog");
            this.viewLog.BackColor = System.Drawing.Color.Transparent;
            this.viewLog.Name = "viewLog";
            this.viewLog.TabStop = true;
            this.viewLog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.viewLog_LinkClicked);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.back, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.next, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.cancel, 4, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // back
            // 
            resources.ApplyResources(this.back, "back");
            this.back.Name = "back";
            this.back.UseVisualStyleBackColor = true;
            // 
            // next
            // 
            resources.ApplyResources(this.next, "next");
            this.next.Name = "next";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new System.EventHandler(this.finish_Click);
            // 
            // cancel
            // 
            resources.ApplyResources(this.cancel, "cancel");
            this.cancel.Name = "cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // border1
            // 
            resources.ApplyResources(this.border1, "border1");
            this.border1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.border1.Name = "border1";
            // 
            // textPanelFailure
            // 
            this.textPanelFailure.Controls.Add(this.failureTitle);
            this.textPanelFailure.Controls.Add(this.failureDescription);
            resources.ApplyResources(this.textPanelFailure, "textPanelFailure");
            this.textPanelFailure.Name = "textPanelFailure";
            // 
            // failureTitle
            // 
            resources.ApplyResources(this.failureTitle, "failureTitle");
            this.failureTitle.BackColor = System.Drawing.Color.Transparent;
            this.failureTitle.Name = "failureTitle";
            // 
            // failureDescription
            // 
            resources.ApplyResources(this.failureDescription, "failureDescription");
            this.failureDescription.BackColor = System.Drawing.Color.Transparent;
            this.failureDescription.Name = "failureDescription";
            // 
            // textPanelCancel
            // 
            this.textPanelCancel.Controls.Add(this.cancelText);
            this.textPanelCancel.Controls.Add(this.cancelDescription);
            resources.ApplyResources(this.textPanelCancel, "textPanelCancel");
            this.textPanelCancel.Name = "textPanelCancel";
            // 
            // cancelText
            // 
            resources.ApplyResources(this.cancelText, "cancelText");
            this.cancelText.BackColor = System.Drawing.Color.Transparent;
            this.cancelText.Name = "cancelText";
            // 
            // cancelDescription
            // 
            resources.ApplyResources(this.cancelDescription, "cancelDescription");
            this.cancelDescription.BackColor = System.Drawing.Color.Transparent;
            this.cancelDescription.Name = "cancelDescription";
            // 
            // ExitDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.imgPanel);
            this.Controls.Add(this.bottomPanel);
            this.Name = "ExitDialog";
            this.Load += new System.EventHandler(this.ExitDialog_Load);
            this.imgPanel.ResumeLayout(false);
            this.textPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.image)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.textPanelFailure.ResumeLayout(false);
            this.textPanelCancel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label description;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.PictureBox image;
        private System.Windows.Forms.LinkLabel viewLog;
        private System.Windows.Forms.Panel imgPanel;
        private System.Windows.Forms.Panel border1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button back;
        private System.Windows.Forms.Button next;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Panel textPanel;
        private System.Windows.Forms.Panel textPanelCancel;
        private System.Windows.Forms.Label cancelText;
        private System.Windows.Forms.Label cancelDescription;
        private System.Windows.Forms.Panel textPanelFailure;
        private System.Windows.Forms.Label failureTitle;
        private System.Windows.Forms.Label failureDescription;
    }
}
