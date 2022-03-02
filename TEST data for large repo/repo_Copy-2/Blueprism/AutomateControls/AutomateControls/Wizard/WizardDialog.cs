using System.Windows.Forms;
using System.ComponentModel;

namespace AutomateControls.Wizard
{
    /// <summary>
    /// Base class for all Wizard dialogs.
    /// </summary>
    public class WizardDialog : Forms.HelpButtonForm, IWizardDialog
    {
        /// <summary>
        /// Control that represents the previous button.
        /// </summary>
        protected Control m_PreviousButton;

        /// <summary>
        /// /// Control that represents the next button.
        /// </summary>
        protected Control m_NextButton;

        /// <summary>
        /// Control that represents the cancel button.
        /// </summary>
        protected Control m_CancelButton;

        /// <summary>
        /// The root control of the dialog, this should
        /// be set to a container such as a panel, which
        /// can then be swapped for different panels, as
        /// the wizard progresses.
        /// </summary>
        protected Control m_Root;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public WizardDialog()
        {
        }

        /// <summary>
        /// Provides access to the previous
        /// button is control.
        /// </summary>
        [Browsable(false)]
        public virtual Control NavigatePrevious
        {
            get { return m_PreviousButton; }
        }

        /// <summary>
        /// Provides access to the next
        /// button is control.
        /// </summary>
        [Browsable(false)]
        public virtual Control NavigateNext
        {
            get { return m_NextButton; }
        }

        /// <summary>
        /// Provides access to the cancel
        /// button is control.
        /// </summary>
        [Browsable(false)]
        public virtual Control NavigateCancel
        {
            get { return m_CancelButton; }
        }

        /// <summary>
        /// Provides access to the root control
        /// of the dialog.
        /// </summary>
        [Browsable(false)]
        public virtual Control UIRoot
        {
            get { return m_Root; }
        }

        /// <summary>
        /// Provides access to the root control
        /// of the dialog
        /// </summary>
        Control IWizardDialog.Root
        {
            get { return UIRoot; }
        }

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        public void CloseDialog()
        {
            base.Close();
        }

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        void IWizardDialog.Close()
        {
            CloseDialog();
        }

        /// <summary>
        /// Provides access to the previous
        /// button is control.
        /// </summary>
        [Browsable(true)]
        [Category("Wizard")]
        public virtual Control NavigatePreviousControl
        {
            get { return m_PreviousButton; }
            set { m_PreviousButton = value; }
        }

        /// <summary>
        /// Provides access to the next
        /// button is control.
        /// </summary>
        [Browsable(true)]
        [Category("Wizard")]
        public virtual Control NavigateNextControl
        {
            get { return m_NextButton; }
            set { m_NextButton = value; }
        }

        /// <summary>
        /// Provides access to the cancel
        /// button is control.
        /// </summary>
        [Browsable(true)]
        [Category("Wizard")]
        public virtual Control NavigateCancelControl
        {
            get { return m_CancelButton; }
            set { m_CancelButton = value; }
        }

        /// <summary>
        /// Provides access to the root control of the dialog,
        /// this should be set to a container such as a panel,
        /// which can then be swapped for different panels, as
        /// the wizard progresses.
        /// </summary>
        [Browsable(true)]
        [Category("Wizard")]
        public virtual Control UIRootControl
        {
            get { return m_Root; }
            set { m_Root = value; }
        }

        /// <summary>
        /// "Abstract" Title property. Cannot
        /// really be abstract because then
        /// the class must be abstract which
        /// causes trouble with the forms
        /// designer.
        /// </summary>
        public virtual string Title
        {
            set { }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardDialog));
            this.SuspendLayout();
            // 
            // WizardDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "WizardDialog";
            this.ResumeLayout(false);

        }
    }
}
