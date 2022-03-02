namespace BluePrism.Api.Setup.Dialogs
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
            this.imgPanel = new System.Windows.Forms.Panel();
            this.textPanel = new System.Windows.Forms.Panel();
            this._languageCombo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.image = new System.Windows.Forms.PictureBox();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.bottomBorder = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.back = new System.Windows.Forms.Button();
            this.next = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.imgPanel.SuspendLayout();
            this.textPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.image)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imgPanel
            // 
            resources.ApplyResources(this.imgPanel, "imgPanel");
            this.imgPanel.Controls.Add(this.textPanel);
            this.imgPanel.Controls.Add(this.image);
            this.imgPanel.Name = "imgPanel";
            // 
            // textPanel
            // 
            resources.ApplyResources(this.textPanel, "textPanel");
            this.textPanel.Controls.Add(this._languageCombo);
            this.textPanel.Controls.Add(this.label1);
            this.textPanel.Controls.Add(this.label2);
            this.textPanel.Name = "textPanel";
            // 
            // _languageCombo
            // 
            this._languageCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._languageCombo.FormattingEnabled = true;
            resources.ApplyResources(this._languageCombo, "_languageCombo");
            this._languageCombo.Name = "_languageCombo";
            this._languageCombo.SelectedIndexChanged += new System.EventHandler(this._languageCombo_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Name = "label2";
            // 
            // image
            // 
            resources.ApplyResources(this.image, "image");
            this.image.BackColor = System.Drawing.Color.White;
            this.image.Name = "image";
            this.image.TabStop = false;
            // 
            // bottomPanel
            // 
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.BackColor = System.Drawing.SystemColors.Control;
            this.bottomPanel.Controls.Add(this.bottomBorder);
            this.bottomPanel.Controls.Add(this.tableLayoutPanel1);
            this.bottomPanel.Name = "bottomPanel";
            // 
            // bottomBorder
            // 
            resources.ApplyResources(this.bottomBorder, "bottomBorder");
            this.bottomBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bottomBorder.Name = "bottomBorder";
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
            this.back.Click += new System.EventHandler(this.back_Click);
            // 
            // next
            // 
            resources.ApplyResources(this.next, "next");
            this.next.Name = "next";
            this.next.UseVisualStyleBackColor = true;
            this.next.Click += new System.EventHandler(this.next_Click);
            // 
            // cancel
            // 
            resources.ApplyResources(this.cancel, "cancel");
            this.cancel.Name = "cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // WelcomeDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.imgPanel);
            this.Controls.Add(this.bottomPanel);
            this.Name = "WelcomeDialog";
            this.Load += new System.EventHandler(this.WelcomeDialog_Load);
            this.imgPanel.ResumeLayout(false);
            this.textPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.image)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox image;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Button back;
        private System.Windows.Forms.Button next;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel imgPanel;
        private System.Windows.Forms.Panel bottomBorder;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel textPanel;
        private System.Windows.Forms.ComboBox _languageCombo;
    }
}
