namespace BluePrism.Api.Setup.Dialogs
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
            this.topBorder = new System.Windows.Forms.Panel();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.currentAction = new System.Windows.Forms.Label();
            this.topPanel = new System.Windows.Forms.Panel();
            this.dialogText = new System.Windows.Forms.Label();
            this.banner = new System.Windows.Forms.PictureBox();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.back = new System.Windows.Forms.Button();
            this.next = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.bottomBorder = new System.Windows.Forms.Panel();
            this.description = new System.Windows.Forms.Label();
            this.currentActionLabel = new System.Windows.Forms.Label();
            this.waitPrompt = new System.Windows.Forms.Label();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.banner)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // topBorder
            // 
            resources.ApplyResources(this.topBorder, "topBorder");
            this.topBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.topBorder.Name = "topBorder";
            // 
            // progress
            // 
            resources.ApplyResources(this.progress, "progress");
            this.progress.Name = "progress";
            this.progress.Step = 1;
            // 
            // currentAction
            // 
            resources.ApplyResources(this.currentAction, "currentAction");
            this.currentAction.Name = "currentAction";
            // 
            // topPanel
            // 
            resources.ApplyResources(this.topPanel, "topPanel");
            this.topPanel.BackColor = System.Drawing.SystemColors.Control;
            this.topPanel.Controls.Add(this.dialogText);
            this.topPanel.Controls.Add(this.banner);
            this.topPanel.Name = "topPanel";
            // 
            // dialogText
            // 
            resources.ApplyResources(this.dialogText, "dialogText");
            this.dialogText.BackColor = System.Drawing.Color.Transparent;
            this.dialogText.Name = "dialogText";
            // 
            // banner
            // 
            resources.ApplyResources(this.banner, "banner");
            this.banner.BackColor = System.Drawing.Color.White;
            this.banner.Name = "banner";
            this.banner.TabStop = false;
            // 
            // bottomPanel
            // 
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.BackColor = System.Drawing.SystemColors.Control;
            this.bottomPanel.Controls.Add(this.tableLayoutPanel1);
            this.bottomPanel.Controls.Add(this.bottomBorder);
            this.bottomPanel.Name = "bottomPanel";
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
            // 
            // cancel
            // 
            resources.ApplyResources(this.cancel, "cancel");
            this.cancel.Name = "cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // bottomBorder
            // 
            resources.ApplyResources(this.bottomBorder, "bottomBorder");
            this.bottomBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bottomBorder.Name = "bottomBorder";
            // 
            // description
            // 
            resources.ApplyResources(this.description, "description");
            this.description.BackColor = System.Drawing.Color.Transparent;
            this.description.Name = "description";
            // 
            // currentActionLabel
            // 
            this.currentActionLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.currentActionLabel, "currentActionLabel");
            this.currentActionLabel.Name = "currentActionLabel";
            // 
            // waitPrompt
            // 
            resources.ApplyResources(this.waitPrompt, "waitPrompt");
            this.waitPrompt.ForeColor = System.Drawing.Color.Blue;
            this.waitPrompt.Name = "waitPrompt";
            this.waitPrompt.TabStop = true;
            // 
            // ProgressDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.waitPrompt);
            this.Controls.Add(this.topBorder);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.currentAction);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.description);
            this.Controls.Add(this.currentActionLabel);
            this.Name = "ProgressDialog";
            this.Load += new System.EventHandler(this.ProgressDialog_Load);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.banner)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox banner;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Label dialogText;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Label description;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.Label currentAction;
        private System.Windows.Forms.Label currentActionLabel;
        private System.Windows.Forms.Panel bottomBorder;
        private System.Windows.Forms.Panel topBorder;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button back;
        private System.Windows.Forms.Button next;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Label waitPrompt;
    }
}