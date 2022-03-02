namespace AutomateControlTester.Configurators
{
    partial class ComboBoxConfigurator
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbStyle = new AutomateControls.StyledComboBox();
            this.cmbNormal = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cmbStyle
            // 
            this.cmbStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbStyle.FormattingEnabled = true;
            this.cmbStyle.Location = new System.Drawing.Point(3, 3);
            this.cmbStyle.Name = "cmbStyle";
            this.cmbStyle.Size = new System.Drawing.Size(258, 21);
            this.cmbStyle.TabIndex = 0;
            // 
            // cmbNormal
            // 
            this.cmbNormal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbNormal.FormattingEnabled = true;
            this.cmbNormal.Location = new System.Drawing.Point(3, 30);
            this.cmbNormal.Name = "cmbNormal";
            this.cmbNormal.Size = new System.Drawing.Size(258, 21);
            this.cmbNormal.TabIndex = 1;
            // 
            // ComboBoxConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmbNormal);
            this.Controls.Add(this.cmbStyle);
            this.Name = "ComboBoxConfigurator";
            this.Size = new System.Drawing.Size(264, 191);
            this.ResumeLayout(false);

        }

        #endregion

        private AutomateControls.StyledComboBox cmbStyle;
        private System.Windows.Forms.ComboBox cmbNormal;
    }
}
