namespace BluePrism.UIAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;

    using Patterns;

    using UIAutomationClient;

    /// <summary>
    /// Contains extension methods for use only in BluePrism.UIA
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Gets the pattern of a specified type from a UI Automation element,
        /// ensuring that such a pattern type is supported in this operating system.
        /// </summary>
        /// <param name="this">The automation element from which to get the current
        /// pattern of the specified type</param>
        /// <param name="type">The type of pattern to retrieve from the element.
        /// </param>
        /// <returns>The automation pattern from the given element of the specified
        /// type, or null if the type is not supported by the element or by the
        /// operating system.</returns>
        internal static TPattern CheckAndGetCurrentPattern<TPattern>(
            this IUIAutomationElement @this, PatternType type)
        {
            return (TPattern)CheckAndGetCurrentPattern(@this, type);
        }

        /// <summary>
        /// Gets the pattern of a specified type from a UI Automation element,
        /// ensuring that such a pattern type is supported in this operating system.
        /// </summary>
        /// <param name="this">The automation element from which to get the current
        /// pattern of the specified type</param>
        /// <param name="type">The type of pattern to retrieve from the element.
        /// </param>
        /// <returns>The automation pattern from the given element of the specified
        /// type, or null if the type is not supported by the element or by the
        /// operating system.</returns>
        internal static object CheckAndGetCurrentPattern(
            this IUIAutomationElement @this, PatternType type)
        {
            // Check if the operating system supports this type first.
            if (!type.IsSupported()) return null;

            try
            {
                return @this.GetCurrentPattern((int)type);
            }
            catch(Exception ex)
            {
                switch (ex)
                {
                    case ArgumentException _:
                    case COMException _:
                        // Mark it so we don't need to jump through this hoop in future
                        type.MarkNotSupported();

                        // And, by definition, it's not supported... so...
                        return null;

                    default:
                        throw;
                }
            }
        }
        /// <summary>
        /// Converts a <see cref="IUIAutomationElementArray"/> to an enumerable.
        /// </summary>
        /// <param name="this">The object to convert.</param>
        /// <returns>An <see cref="IEnumerable{IUIAutomationElementArray}"/> object.</returns>
        public static IEnumerable<IUIAutomationElement> ToEnumerable(this IUIAutomationElementArray @this)
        {
            for (var i = 0; i < @this.Length; ++i)
                yield return ComHelper.TryGetComValue(() => @this.GetElement(i));
        }

        /// <summary>
        /// Converts a <see cref="IUIAutomationTextRangeArray"/> to an enumerable.
        /// </summary>
        /// <param name="this">The object to convert.</param>
        /// <returns>An <see cref="IEnumerable{IUIAutomationElementArray}"/> object.</returns>
        public static IEnumerable<IUIAutomationTextRange> ToEnumerable(this IUIAutomationTextRangeArray @this)
        {
            for (var i = 0; i < @this.Length; ++i)
                yield return ComHelper.TryGetComValue(() => @this.GetElement(i));
        }

        /// <summary>
        /// Converts a <see cref="tagRECT"/> to a <see cref="Rectangle"/>
        /// </summary>
        /// <param name="this">The rectangle to convert.</param>
        /// <returns>A <see cref="Rectangle"/></returns>
        public static Rectangle ToRectangle(this tagRECT @this) =>
            new Rectangle(@this.left, @this.top, @this.right - @this.left, @this.bottom - @this.top);

        /// <summary>
        /// Converts a <see cref="Point"/> to a <see cref="tagPOINT"/>
        /// </summary>
        /// <param name="this">The point to convert.</param>
        /// <returns></returns>
        public static tagPOINT ToTagPoint(this Point @this) =>
            new tagPOINT {x = @this.X, y = @this.Y};
    }
}
