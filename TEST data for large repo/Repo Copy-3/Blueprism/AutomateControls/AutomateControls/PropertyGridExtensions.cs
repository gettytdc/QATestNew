using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace AutomateControls
{
    /// <summary>
    /// Utility extension methods for PropertyGrid controls
    /// </summary>
    public static class PropertyGridExtensions
    {

        /// <summary>
        /// Sets size of description area within a PropertyGrid control
        /// </summary>
        /// <param name="grid">The PropertyGrid instance</param>
        /// <param name="lines">The number of lines of text to base size on</param>
        public static void ResizeDescriptionArea(this PropertyGrid grid, int lines)
        {
            try
            {
                foreach (var control in grid.Controls)
                {
                    var type = control.GetType();
                    if (type.Name == "DocComment")
                    {
                        // Reflection is the only way
                        // https://stackoverflow.com/questions/2248229/how-do-you-programmatically-adjust-the-horizontal-divider-of-a-propertygrid-cont
                        var field = type.GetField("userSized", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (field != null)
                        {
                            field.SetValue(control, true);
                        }
                        var linesProperty = type.GetProperty("Lines");
                        if (linesProperty != null)
                        {
                            linesProperty.SetValue(control, lines, null);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        
    }
}