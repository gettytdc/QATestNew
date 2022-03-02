namespace AutomateControls
{
    /// <summary>
    /// The way in which lines should be rendered in a
    /// <see cref="GrippableSplitContainer"/>
    /// </summary>
    public enum GrippableSplitLineMode
    {
        /// <summary>
        /// No lines should be rendered
        /// </summary>
        None = 0,

        /// <summary>
        /// A single central line should be rendered
        /// </summary>
        Single = 1,

        /// <summary>
        /// Dual parallel lines should be rendered
        /// </summary>
        Double = 2
    }
}
