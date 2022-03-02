using System.Windows.Forms;

namespace AutomateControls.Trees
{
    /// <summary>
    /// Delegate used to filter a treeview, given a search term
    /// </summary>
    /// <param name="searchTerm">The search term being applied</param>
    /// <param name="node">The node being tested to see if it should pass the
    /// filter.</param>
    /// <returns>true to indicate that the treenode passes the filter; false to
    /// indicate that it should be filtered out.</returns>
    public delegate bool TreeNodeFilter(string searchTerm, TreeNode node);

}
