namespace AutomateControls.Forms
{
    partial class UserMessage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserMessage));
            this.txtMessage = new AutomateControls.Textboxes.StyledTextBox();
            this.okButton = new AutomateControls.Buttons.StandardStyledButton();
            this.pnlError = new System.Windows.Forms.Panel();
            this.cancelButton = new AutomateControls.Buttons.StandardStyledButton();
            this.pnlError.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.txtMessage, "txtMessage");
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.TabStop = false;
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // pnlError
            // 
            resources.ApplyResources(this.pnlError, "pnlError");
            this.pnlError.Controls.Add(this.cancelButton);
            this.pnlError.Controls.Add(this.okButton);
            this.pnlError.Name = "pnlError";
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // UserMessage
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.pnlError);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserMessage";
            this.ShowInTaskbar = false;
            this.pnlError.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal AutomateControls.Textboxes.StyledTextBox txtMessage;
        internal AutomateControls.Buttons.StandardStyledButton okButton;
        internal System.Windows.Forms.Panel pnlError;
        internal AutomateControls.Buttons.StandardStyledButton cancelButton;
    }
}
