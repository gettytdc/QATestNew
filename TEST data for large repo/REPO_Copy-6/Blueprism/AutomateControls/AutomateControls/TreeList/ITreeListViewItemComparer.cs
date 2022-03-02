using System.Windows.Forms;
using System.Collections;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// Interface ITreeListViewItemComparer
    /// </summary>
    public interface ITreeListViewItemComparer : IComparer
    {
        /// <summary>
        /// Sort order
        /// </summary>
        SortOrder SortOrder { get; set; }

        /// <summary>
        /// Column for the comparison
        /// </summary>
        int Column { get; set; }
    }
}
