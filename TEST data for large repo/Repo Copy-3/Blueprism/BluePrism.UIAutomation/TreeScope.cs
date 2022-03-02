namespace BluePrism.UIAutomation
{
    /// <summary>
    /// The scope used for tree navigation
    /// </summary>
    public enum TreeScope
    {
        /// <summary>
        /// Subtree excluded
        /// </summary>
        None = 0,
        /// <summary>
        /// This element
        /// </summary>
        Element = 1,
        /// <summary>
        /// Children of the element
        /// </summary>
        Children = 2,
        /// <summary>
        /// Children and more distant descendants of the element
        /// </summary>
        Descendants = 4,
        /// <summary>
        /// The element and its descendants
        /// </summary>
        Subtree = 7,
        /// <summary>
        /// The element's parent
        /// </summary>
        Parent = 8,
        /// <summary>
        /// The element's parent and more distant ancestors
        /// </summary>
        Ancestors = 16,
    }
}