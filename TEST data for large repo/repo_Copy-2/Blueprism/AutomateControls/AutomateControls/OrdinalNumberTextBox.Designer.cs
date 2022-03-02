namespace AutomateControls
{
    partial class OrdinalNumberTextBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrdinalNumberTextBox));
            this.mOrdinalLabel = new System.Windows.Forms.Label();
            this.mNumberBox = new AutomateControls.Textboxes.StyledTextBox();
            this.SuspendLayout();
            // 
            // mOrdinalLabel
            // 
            resources.ApplyResources(this.mOrdinalLabel, "mOrdinalLabel");
            this.mOrdinalLabel.Name = "mOrdinalLabel";
            // 
            // mNumberBox
            // 
            resources.ApplyResources(this.mNumberBox, "mNumberBox");
            this.mNumberBox.Name = "mNumberBox";
            // 
            // OrdinalNumberTextBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.mNumberBox);
            this.Controls.Add(this.mOrdinalLabel);
            this.Name = "OrdinalNumberTextBox";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mOrdinalLabel;
        private AutomateControls.Textboxes.StyledTextBox mNumberBox;
    }
}
