#if UNITTESTS

namespace BluePrism.UIAutomation.UnitTests
{
    using System;
    using System.Linq;
    using System.Reflection;

    using NUnit.Framework;

    using BPCoreLib;

    using Patterns;

    [TestFixture]
    public class PatternTests
    {
        /// <summary>
        /// Ensures that all IAutomationPattern implementations defined in this
        /// assembly have an attribute indicating which pattern type they implement.
        /// </summary>
        [Test]
        public void TestThatAllPatternsHaveImplementsPatternAttribute()
        {
            var missingAttrs = Assembly
                .GetExecutingAssembly()
                .GetSubInterfaces<IAutomationPattern>()
                .Where(t =>
                    t.GetCustomAttribute<RepresentsPatternTypeAttribute>() == null);

            Assert.That(missingAttrs.Any(), Is.False,
                "IAutomationPattern(s) found with no RepresentsPatternType: {0}",
                String.Join(", ", missingAttrs.Select(t => t.Name))
            );

        }

    }
}

#endif