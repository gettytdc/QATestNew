namespace BluePrism.CharMatching.UI
{
    partial class CharacterPair
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CharacterPair));
            this.txtLetter = new AutomateControls.Textboxes.StyledTextBox();
            this.SuspendLayout();
            // 
            // txtLetter
            // 
            resources.ApplyResources(this.txtLetter, "txtLetter");
            this.txtLetter.Name = "txtLetter";
            this.txtLetter.TextChanged += new System.EventHandler(this.HandleTextChanged);
            this.txtLetter.Enter += new System.EventHandler(this.HandleLetterEntered);
            this.txtLetter.Validated += new System.EventHandler(this.HandleTextValidated);
            // 
            // CharacterPair
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.txtLetter);
            this.Name = "CharacterPair";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AutomateControls.Textboxes.StyledTextBox txtLetter;
    }
}
