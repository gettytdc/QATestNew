using BluePrism.Core.Resources;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using static BluePrism.Core.Extensions.ResourceAttributeExtensions;

namespace BluePrism.Core.UnitTests.Extensions
{
    [TestFixture]
    public class ResourceAttributesExtensionsTests
    {
        [Test]
        public void TestResourcesAttributeEnum_ToIntList_Or()
        {
            var attributeList = new List<ResourceAttribute>() {
                ResourceAttribute.Retired,
                ResourceAttribute.Local,
                ResourceAttribute.Debug,
                ResourceAttribute.Pool,
                ResourceAttribute.LoginAgent,
                ResourceAttribute.Private
            };

            foreach (var attrA in attributeList)
            {
                foreach (var attrB in attributeList.Where(x => x != attrA))
                {
                    var attribute = attrA | attrB;
                    var intList = attribute.ToIntList();

                    Assert.IsTrue(intList.Contains((int)attrA));
                    Assert.IsTrue(intList.Contains((int)attrB));
                }
            }
        }

        [Test]
        public void TestResourcesAttributeEnum_ToIntList()
        {
            var attributeList = new List<ResourceAttribute>() {
                ResourceAttribute.Retired,
                ResourceAttribute.Local,
                ResourceAttribute.Debug,
                ResourceAttribute.Pool,
                ResourceAttribute.LoginAgent,
                ResourceAttribute.Private
            };

            foreach (var attribute in attributeList)
            {
                var intList = attribute.ToIntList();

                Assert.IsTrue(intList.Contains((int)attribute));
            }
        }
    }
}
