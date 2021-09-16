namespace BluePrism.Core.Utility
{
    using System.Drawing;

    /// <summary>
    /// Extension methods for <see cref="System.Drawing"/> objects.
    /// </summary>
    public static class DrawingExtensionMethods
    {
        /// <summary>
        /// Gets the centre of the rectangle
        /// </summary>
        /// <param name="this">The rectangle.</param>
        /// <returns>The centre point of the rectangle</returns>
        public static Point Center(this Rectangle @this) =>
            new Point(
                @this.Left + @this.Width / 2,
                @this.Top + @this.Height / 2);

        /// <summary>
        /// Gets the centre of the size
        /// </summary>
        /// <param name="this">The size.</param>
        /// <returns>The centre point of the size</returns>
        public static Point Center(this Size @this) =>
            new Point(
                @this.Width / 2,
                @this.Height / 2);
                
        /// <summary>
        /// Gets the color represented as a hexadecimal value
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>A string containing a color suitable for css</returns>
        public static string ToWebColor(this Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
