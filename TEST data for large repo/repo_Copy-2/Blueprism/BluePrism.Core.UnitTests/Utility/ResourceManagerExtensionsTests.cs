#if UNITTESTS

using System.Resources;
using BluePrism.Core.Utility;
using FluentAssertions;
using NUnit.Framework;
namespace BluePrism.Core.UnitTests.Utility
{
    public class ResourceManagerExtensionsTests
    {
        private ResourceManager Resources { get; } = ResourceManagerTestResources.ResourceManager;

        [Test]
        public void GetString_WithValidNameFromTemplate_ShouldReturnString()
        {
            string result = Resources.GetString("Example1_{0}", "Test1");

            result.Should().Be(ResourceManagerTestResources.Example1_Test1);
        }

        [Test]
        public void GetString_WithMissingNameFromTemplate_ShouldReturnNull()
        {
            string result = Resources.GetString("Example1_{0}", "TestXX");

            result.Should().BeNull();
        }
    }
}

#endif