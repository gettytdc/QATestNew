using System;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// Check boxes types for TreeListView control
    /// </summary>
    [Serializable]
    public enum CheckBoxesTypes
    {
        /// <summary>
        /// No CheckBoxes
        /// </summary>
        None,
        /// <summary>
        /// Simple CheckBoxes
        /// </summary>
        Simple,
        /// <summary>
        /// Check children recursively and set indeterminate state for the parents
        /// </summary>
        Recursive,
    }
}
