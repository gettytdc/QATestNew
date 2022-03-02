using System.Drawing;

namespace AutomateControls
{
    /// <summary>
    /// Interface describing a component which honours the environment colouring,
    /// configurable within the client application.
    /// </summary>
    public interface IEnvironmentColourManager
    {
        /// <summary>
        /// Gets or sets the environment-specific back colour in use in this environment.
        /// Only set to the database-held values after login. Note that this only affects
        /// the UI owned directly by this component - ie. setting the colour here will
        /// not update the database.
        /// </summary>
        Color EnvironmentBackColor { get; set; }

        /// <summary>
        /// Gets or sets the environment-specific fore colour in use in this environment.
        /// Only set to the database-held values after login. Note that this only affects
        /// the UI owned directly by this component - ie. setting the colour here will
        /// not update the database.
        /// </summary>
        Color EnvironmentForeColor { get; set; }
    }
}
