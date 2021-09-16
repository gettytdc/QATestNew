namespace AutomateControls.Wizard
{
    /// <summary>
    /// Interface all wizard panels must implement
    /// </summary>
    public interface IWizardPanel
    {
        /// <summary>
        /// The controller of the panel.
        /// </summary>
        WizardController Controller { set; }

        /// <summary>
        /// Whether to show the Navigate Previous button.
        /// </summary>
        bool ShowNavigatePrevious { get; }

        /// <summary>
        /// Whether to show the Navigate Next button.
        /// </summary>
        bool ShowNavigateNext { get; }

        /// <summary>
        /// The title of the Panel.
        /// </summary>
        string Title { set; get; }
    }
}
