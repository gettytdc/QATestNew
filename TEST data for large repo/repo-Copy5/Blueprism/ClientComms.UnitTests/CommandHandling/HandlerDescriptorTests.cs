using System;
using BluePrism.ApplicationManager.CommandHandling;
using NUnit.Framework;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling
{
    public class HandlerDescriptorTests
    {
        [Test]
        public void Constructor_WithInvalidType_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => { var descriptor = new HandlerDescriptor(typeof(string), "test"); });
        }
    }
}