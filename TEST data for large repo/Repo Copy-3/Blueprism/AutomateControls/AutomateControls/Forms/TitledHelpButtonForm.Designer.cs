namespace AutomateControls.Forms
{
    partial class TitledHelpButtonForm
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
            this.objBluebar = new AutomateControls.TitleBar();
            this.SuspendLayout();
            // 
            // objBluebar
            // 
            this.objBluebar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objBluebar.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.objBluebar.Location = new System.Drawing.Point(0, 0);
            this.objBluebar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.objBluebar.Name = "objBluebar";
            this.objBluebar.Size = new System.Drawing.Size(784, 74);
            this.objBluebar.TabIndex = 106;
            this.objBluebar.WrapTitle = false;
            // 
            // TitledHelpButtonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.objBluebar);
            this.Name = "TitledHelpButtonForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TitledHelpButtonForm";
            this.ResumeLayout(false);

        }

        #endregion

        protected TitleBar objBluebar;
    }
}