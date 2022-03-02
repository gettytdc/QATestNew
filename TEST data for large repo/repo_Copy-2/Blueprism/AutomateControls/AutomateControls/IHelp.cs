/// Project  : AutomateControls
/// Interface    : IHelp
/// 
/// <summary>
/// This interface defines the GetHelpFile function which is called if the control
/// has a help file associated with it.
/// </summary>
namespace AutomateControls
{
    public interface IHelp
    {
        /// <summary>
        /// Called when the user presses F1 clicks help in the main menu, or a help button
        /// </summary>
        /// <returns>A string containing the filename, not path of the help file.</returns>
        string GetHelpFile();
    }
}