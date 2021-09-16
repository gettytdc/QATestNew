using System.Windows.Forms;

namespace AutomateControls.Wizard
{
    /// <summary>
    /// Interface that all wizard dialogs must implement.
    /// </summary>
    public interface IWizardDialog
    {
        /// <summary>
        /// The title of the dialog.
        /// </summary>
        string Title { set; }

        /// <summary>
        /// The Navigate Previous control.
        /// </summary>
        Control NavigatePrevious { get; }

        /// <summary>
        /// The Navigate Next control.
        /// </summary>
        Control NavigateNext { get; }

        /// <summary>
        /// The Navigate Cancel control.
        /// </summary>
        Control NavigateCancel { get; }

        /// <summary>
        /// The root control.
        /// </summary>
        Control Root { get; }

        /// <summary>
        /// A close function.
        /// </summary>
        void Close();
    }
}
