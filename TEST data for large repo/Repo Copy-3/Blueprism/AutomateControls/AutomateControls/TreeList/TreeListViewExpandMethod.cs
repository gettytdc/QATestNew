using System;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// Expand / Collapse method
    /// </summary>
    [Serializable]
    public enum TreeListViewExpandMethod
    {
        /// <summary>
        /// Expand when double clicking on the icon
        /// </summary>
        DoubleClickIcon,
        /// <summary>
        /// Expand when double clicking on the entire item
        /// </summary>
        DoubleClickEntireItem,
        /// <summary>
        /// Expand when double clicking on the item only
        /// </summary>
        DoubleClickItemOnly,
        /// <summary>
        /// None
        /// </summary>
        None
    }
}
