using System;

namespace AutomateControls.TreeList
{
    /// <summary>
    /// TreeListView actions
    /// </summary>
    [Serializable]
    public enum TreeListViewAction
    {
        /// <summary>
        /// By Keyboard
        /// </summary>
        ByKeyboard,
        /// <summary>
        /// ByMouse
        /// </summary>
        ByMouse,
        /// <summary>
        /// Collapse
        /// </summary>
        Collapse,
        /// <summary>
        /// Expand
        /// </summary>
        Expand,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }
}
