#if UNITTESTS
using System;
using BluePrism.AutomateProcessCore.Compilation;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.Compilation
{
    public class AssemblyDefinitionTests
    {
        [Test]
        public void IsEmpty_WithEmptyClasses_ReturnsTrue()
        {
            var classDefinition = new ClassDefinition("Class1", Array.Empty<MethodDefinition>(), string.Empty, string.Empty);
            var definition = new AssemblyDefinition("Test", CodeLanguage.CSharp, Array.Empty<string>(), Array.Empty<string>(), new[] { classDefinition });
            definition.IsEmpty.Should().BeTrue();
        }

        [Test]
        public void IsEmpty_WithNonEmptyClasses_ReturnsFalse()
        {
            var classDefinition = new ClassDefinition("Class1", Array.Empty<MethodDefinition>(), string.Empty, string.Empty);
            var definition = new AssemblyDefinition("Test", CodeLanguage.CSharp, Array.Empty<string>(), Array.Empty<string>(), new[] { classDefinition });
            definition.IsEmpty.Should().BeTrue();
        }
    }
}
#endif
