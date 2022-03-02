using System;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// Check boxes direction in the TreeListView
    /// </summary>
    [Flags, Serializable]
    internal enum CheckDirection
    {
        /// <summary>
        /// Simply check the item
        /// </summary>
        None = 0,
        /// <summary>
        /// Set the indeterminate state to the parent items
        /// </summary>
        Upwards = 1,
        /// <summary>
        /// Check children items recursively
        /// </summary>
        Downwards = 2,
        /// <summary>
        /// Upwards + Downwards
        /// </summary>
        All = 3,
    }
}
