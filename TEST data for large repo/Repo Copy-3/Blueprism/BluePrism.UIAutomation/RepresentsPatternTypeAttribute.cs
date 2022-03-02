using System;
using System.Collections.Concurrent;
using System.Linq;
using BluePrism.UIAutomation.Patterns;

namespace BluePrism.UIAutomation
{
    /// <summary>
    /// Attribute indicating which pattern type that a pattern represents.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Interface, Inherited = Inheritable, AllowMultiple = true)]
    public class RepresentsPatternTypeAttribute : Attribute
    {
        // Indicates if this attribute is inheritable or not. Affects the
        // AttributeUsage and the static GetPatternType helper method.
        private const bool Inheritable = false;

        // A cache of pattern types keyed against their corresponding types
        private static readonly ConcurrentDictionary<Type, PatternType>
            _patternTypeCache = new ConcurrentDictionary<Type, PatternType>();

        /// <summary>
        /// Gets the <see cref="PatternType">pattern type</see> corresponding to a
        /// given <see cref="IAutomationPattern"/> implementation.
        /// </summary>
        /// <typeparam name="T">The type of the automation pattern for which the
        /// pattern type value is required.
        /// </typeparam>
        /// <returns>The pattern type enum value corresponding to the given pattern
        /// type or <c>default(PatternType)</c> if the type had no attribute declared
        /// on it indicating which pattern type it represented.</returns>
        internal static PatternType GetPatternType<T>() where T : IAutomationPattern
        {
            return GetPatternType(typeof(T));
        }

        /// <summary>
        /// Gets the <see cref="PatternType">pattern type</see> corresponding to a
        /// given <see cref="IAutomationPattern"/> implementation.
        /// </summary>
        /// <param name="T">The type of the <see cref="IAutomationPattern"/>
        /// implementation for which the pattern type value is required.
        /// </typeparam>
        /// <returns>The pattern type enum value corresponding to the given pattern
        /// type or <c>default(PatternType)</c> if the type had no attribute declared
        /// on it indicating which pattern type it represented.</returns>
        /// <seealso cref="Inheritable"/>
        public static PatternType GetPatternType(Type netType)
        {
            return _patternTypeCache.GetOrAdd(netType, type => {
                // Create a local method to test if a given type extends
                // IAutomationPattern
                Func<Type, bool> extendsAutomationPattern = subInterfaceType =>
                     subInterfaceType != typeof(IAutomationPattern) &&
                     typeof(IAutomationPattern).IsAssignableFrom(subInterfaceType);

                // Get the interface type that extends IAutomationPattern - either
                // the type itself, or the interface which extends IAutomationPattern
                // that the type implements.
                var interfaceType =
                    type.IsInterface && extendsAutomationPattern(type)
                    ? type
                    : type.GetInterfaces()
                      .Where(tp => extendsAutomationPattern(tp))
                      .SingleOrDefault();

                // Get the 'RepresentsPatternType' attribute from the interface type,
                // if we have one
                var attr = interfaceType
                    ?.GetCustomAttributes(typeof(RepresentsPatternTypeAttribute), Inheritable)
                    .OfType<RepresentsPatternTypeAttribute>()
                    .SingleOrDefault();

                // If we have an attribute, get its pattern, otherwise, default to 0
                return (attr == null ? default(PatternType) : attr.Pattern);
                
            });

        }

        /// <summary>
        /// Gets the pattern type that the decorated class implements
        /// </summary>
        public PatternType Pattern { get; }

        /// <summary>
        /// Creates the attribute indicating which pattern type the decorated class
        /// implements.
        /// </summary>
        /// <param name="type">The pattern type which the decorated class implements.
        /// </param>
        public RepresentsPatternTypeAttribute(PatternType type)
        {
            if (type == default(PatternType)) throw new ArgumentNullException(nameof(type),
                "Cannot create an RepresentsPatternType attribute with no type");

            Pattern = type;
        }
    }
}
