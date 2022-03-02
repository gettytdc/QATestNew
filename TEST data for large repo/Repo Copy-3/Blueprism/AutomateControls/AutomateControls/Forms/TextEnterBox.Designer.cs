namespace AutomateControls.Forms
{
    partial class TextEnterBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextEnterBox));
            this.lblPrompt = new System.Windows.Forms.Label();
            this.tbEnteredText = new AutomateControls.Textboxes.StyledTextBox();
            this.btnOk = new AutomateControls.Buttons.StandardStyledButton();
            this.btnCancel = new AutomateControls.Buttons.StandardStyledButton();
            this.SuspendLayout();
            // 
            // lblPrompt
            // 
            resources.ApplyResources(this.lblPrompt, "lblPrompt");
            this.lblPrompt.Name = "lblPrompt";
            // 
            // tbEnteredText
            // 
            resources.ApplyResources(this.tbEnteredText, "tbEnteredText");
            this.tbEnteredText.Name = "tbEnteredText";
            this.tbEnteredText.TextChanged += new System.EventHandler(this.tbEnteredText_TextChanged);
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.CausesValidation = false;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // TextEnterBox
            // 
            this.AcceptButton = this.btnOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.tbEnteredText);
            this.Controls.Add(this.lblPrompt);
            this.Name = "TextEnterBox";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPrompt;
        private AutomateControls.Textboxes.StyledTextBox tbEnteredText;
        private AutomateControls.Buttons.StandardStyledButton btnOk;
        private AutomateControls.Buttons.StandardStyledButton btnCancel;
    }
}
