namespace AutomateControls.Wizard
{
    partial class StandardWizard : WizardDialog
    {
        public StandardWizard()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
        }

        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.  
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StandardWizard));
            this.CancelledButton = new AutomateControls.Buttons.StandardStyledButton();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.Bluebar = new AutomateControls.TitleBar();
            this.NextButton = new AutomateControls.Buttons.StandardStyledButton();
            this.BackButton = new AutomateControls.Buttons.StandardStyledButton();
            this.SuspendLayout();
            // 
            // CancelledButton
            // 
            resources.ApplyResources(this.CancelledButton, "CancelledButton");
            this.CancelledButton.CausesValidation = false;
            this.CancelledButton.Name = "CancelledButton";
            this.CancelledButton.UseVisualStyleBackColor = true;
            // 
            // Panel1
            // 
            resources.ApplyResources(this.Panel1, "Panel1");
            this.Panel1.Name = "Panel1";
            // 
            // Bluebar
            // 
            resources.ApplyResources(this.Bluebar, "Bluebar");
            this.Bluebar.Name = "Bluebar";
            // 
            // NextButton
            // 
            resources.ApplyResources(this.NextButton, "NextButton");
            this.NextButton.Image = global::AutomateControls.Properties.Resources.next;
            this.NextButton.Name = "NextButton";
            this.NextButton.UseVisualStyleBackColor = true;
            // 
            // BackButton
            // 
            resources.ApplyResources(this.BackButton, "BackButton");
            this.BackButton.CausesValidation = false;
            this.BackButton.Image = global::AutomateControls.Properties.Resources.back;
            this.BackButton.Name = "BackButton";
            this.BackButton.UseVisualStyleBackColor = true;
            // 
            // StandardWizard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.Bluebar);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.CancelledButton);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.Panel1);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StandardWizard";
            this.NavigateCancelControl = this.CancelledButton;
            this.NavigateNextControl = this.NextButton;
            this.NavigatePreviousControl = this.BackButton;
            this.UIRootControl = this.Panel1;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StandardWizard_FormClosing);
            this.ResumeLayout(false);

        }
        internal System.Windows.Forms.Panel Panel1;
        protected TitleBar Bluebar;
        private AutomateControls.Buttons.StandardStyledButton CancelledButton;
        private AutomateControls.Buttons.StandardStyledButton NextButton;
        private AutomateControls.Buttons.StandardStyledButton BackButton;
    }
}
