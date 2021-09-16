#if UNITTESTS
using System;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.Compilation;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.Compilation
{
    public class ClassDefinitionTests
    {
        [Test]
        public void IsEmpty_NoMethodsOrSharedCode_ReturnsTrue()
        {
            var definition = new ClassDefinition("Class1", Array.Empty<MethodDefinition>(), string.Empty, string.Empty);
            definition.IsEmpty.Should().BeTrue();
        }

        [Test]
        public void IsEmpty_MethodsAndNoSharedCode_ReturnsFalse()
        {
            var method = new MethodDefinition("Method1", "Method1.cs", string.Empty, new[] { new MethodParameterDefinition("Param1", DataType.text, true) });
            var definition = new ClassDefinition("Class1", new[] { method }, string.Empty, string.Empty);
            definition.IsEmpty.Should().BeFalse();
        }

        [Test]
        public void IsEmpty_SharedCodeOnly_ReturnsFalse()
        {
            var definition = new ClassDefinition("Class1", Array.Empty<MethodDefinition>(), "public const int i = 0;", "shared.vb");
            definition.IsEmpty.Should().BeFalse();
        }
    }
}
#endif
