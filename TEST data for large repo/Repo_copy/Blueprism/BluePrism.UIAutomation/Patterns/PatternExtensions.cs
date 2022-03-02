using BluePrism.BPCoreLib.Collections;

namespace BluePrism.UIAutomation.Patterns
{
    /// <summary>
    /// Extension methods dealing with patterns and pattern types.
    /// </summary>
    public static class PatternExtensions
    {
        /// <summary>
        /// A set of unsupported pattern types for this operating system.
        /// </summary>
        private static readonly IBPSet<PatternType> _unsupportedTypes =
            GetSynced.ISet(new clsSet<PatternType>());

        /// <summary>
        /// Checks if the given pattern type is supported by this operating system or
        /// not. Note that the support of the operating system is lazily built, so a
        /// pattern type is considered supported until it is marked otherwise.
        /// </summary>
        /// <param name="this">The pattern type to test</param>
        /// <returns>True if the given pattern type is currently considered supported
        /// in this operating system - ie. it has not yet been marked as unsupported.
        /// False if it is has been so marked.</returns>
        /// <seealso cref="MarkNotSupported"/>
        public static bool IsSupported(this PatternType @this)
        {
            return !_unsupportedTypes.Contains(@this);
        }

        /// <summary>
        /// Marks the given pattern type as unsupported in this operating system
        /// </summary>
        /// <param name="this">The pattern type to mark as not supported.</param>
        public static void MarkNotSupported(this PatternType @this)
        {
            _unsupportedTypes.Add(@this);
        }

    }
}
