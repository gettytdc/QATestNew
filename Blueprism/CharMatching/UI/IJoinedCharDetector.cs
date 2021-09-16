namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Interface describing a class which can detect multiple characters assigned to
    /// a glyph, primarily to alter the way that the user interface handles auto
    /// focusing when entering other values.
    /// </summary>
    internal interface IJoinedCharDetector
    {
        /// <summary>
        /// Gets whether joined characters have been detected within the scope of the
        /// implementing object.
        /// </summary>
        bool JoinedCharactersDetected { get; }
    }
}
