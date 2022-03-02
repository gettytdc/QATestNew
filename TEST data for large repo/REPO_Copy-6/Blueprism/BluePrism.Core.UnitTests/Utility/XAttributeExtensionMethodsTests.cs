#if UNITTESTS
using System;
using BluePrism.Core.Utility;
using NUnit.Framework;
using System.Xml.Linq;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Core.UnitTests.Utility
{
    [TestFixture]
    public class XAttributeExtensionMethodsTests
    {
    
        [Test]
        public void XAttribute_ValueOfInt_ShouldReturnCorrectValue()
        {
            var attribute = new XAttribute("test1", "34");
            Assert.That(attribute.Value<int>().Equals(34));
        }

        [Test]
        public void XAttribute_ValueOfInt_ShouldThrowException()
        {
            var attribute = new XAttribute("test1", "notanumber");
            Assert.That(() => attribute.Value<int>(), Throws.InstanceOf<InvalidValueException>());
        }

        [Test]
        public void XAttribute_ValueOfBool_ShouldReturnCorrectValue()
        {
            var attribute = new XAttribute("test1", "true");
            Assert.That(attribute.Value<bool>().Equals(true));
        }

        [Test]
        public void XAttribute_ValueOfBool_ShouldNotReturnCorrectValue()
        {
            var attribute = new XAttribute("test1", "notaboolean");
            Assert.That(() => attribute.Value<bool>(), Throws.InstanceOf<InvalidValueException>());
        }

        [Test]
        public void XAttribute_ValueOfString_ShouldReturnCorrectValue()
        {
            var attribute = new XAttribute("test1", "value1");
            Assert.That(attribute.Value<string>().Equals("value1"));
        }

        [Test]
        public void XAttribute_ValueOfGuid_ShouldReturnCorrectValue()
        {
            var newGuid = Guid.NewGuid();
            var attribute = new XAttribute("test1", newGuid.ToString());
            Assert.That(attribute.Value<Guid>().Equals(newGuid));
        }
        
        [Test]
        public void XAttribute_ValueOfGuid_ShouldThrowException()
        {
            var attribute = new XAttribute("test1", "Not a guid");
            Assert.That(() => attribute.Value<Guid>(), Throws.InstanceOf<InvalidValueException>());
        }                

        [Test]
        public void XAttribute_ValueAsUri_ShouldReturnCorrectValue()
        {
            var attribute = new XAttribute("test1", "https://www.google.com");
            Assert.That(attribute.ValueAsUri().Equals(new Uri("https://www.google.com")));
        }

        [Test]
        public void XAttribute_ValueAsUri_ShouldThrowException()
        {
            var attribute = new XAttribute("test1", "not a uri");
            Assert.That(() => attribute.ValueAsUri(), Throws.InstanceOf<InvalidValueException>());
        }

    }
}

#endif
