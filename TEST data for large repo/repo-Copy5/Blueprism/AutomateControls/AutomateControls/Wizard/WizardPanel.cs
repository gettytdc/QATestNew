using System.Windows.Forms;
using System.ComponentModel;

namespace AutomateControls.Wizard
{
    /// <summary>
    /// Base class for all panels of the Wizard
    /// </summary>
    public class WizardPanel : UserControl, IWizardPanel
    {
        /// <summary>
        /// Holds whether the previous button is enabled.
        /// </summary>
        protected bool m_Previous;

        /// <summary>
        /// Holds whether the next button is enabled.
        /// </summary>
        protected bool m_Next;

        /// <summary>
        /// Holds a reference to the controller of
        /// the wizard.
        /// </summary>
        private WizardController m_Controller;

        /// <summary>
        /// Holds the title of the Wizard panel.
        /// </summary>
        private string m_Title;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public WizardPanel()
        {
        }

        /// <summary>
        /// Provides access to the controller of
        /// the wizard.
        /// </summary>
        public WizardController Controller
        {
            set { m_Controller = value; }
        }

        /// <summary>
        /// Provides access to the Title
        /// </summary>
        public string Title
        {
            set
            {
                m_Title = value;
            }
            get
            {
                return m_Title;
            }
        }

        /// <summary>
        /// Calls the update navigate command on the controller.
        /// </summary>
        public void UpdateNavigate()
        {
            m_Controller?.UpdateNavigate();
        }

        /// <summary>
        /// Provides read only access to
        /// whether the previous button is enabled.
        /// </summary>
        [Browsable(false)]
        public virtual bool ShowNavigatePrevious
        {
            get { return m_Previous; }
        }

        /// <summary>
        /// Provides read only access to
        /// whether the next button is enabled.
        /// </summary>
        [Browsable(false)]
        public virtual bool ShowNavigateNext
        {
            get { return m_Next; }
        }

        /// <summary>
        /// Provides  access to whether the previous
        /// button is enabled.
        /// </summary>
        [Browsable(true)]
        [Category("Wizard")]
        public virtual bool NavigatePrevious
        {
            get { return m_Previous; }
            set { m_Previous = value; }
        }

        /// <summary>
        /// Provides access to whether the next
        /// button is enabled.
        /// </summary>
        [Browsable(true)]
        [Category("Wizard")]
        public virtual bool NavigateNext
        {
            get { return m_Next; }
            set { m_Next = value; }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // WizardPanel
            // 
            this.Name = "WizardPanel";
            this.ResumeLayout(false);

        }
    }
}
