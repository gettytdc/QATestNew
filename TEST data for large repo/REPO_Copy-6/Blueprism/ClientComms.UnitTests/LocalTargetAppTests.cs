using System;
using BluePrism.ApplicationManager;
using BluePrism.BPCoreLib.DependencyInjection;
using BluePrism.Utilities.Testing;
using NUnit.Framework;

namespace ClientComms.UnitTests
{
    [TestFixture()]
    public class LocalTargetAppTests : UnitTestBase<clsLocalTargetApp>
    {
        public override void Setup()
        {
            base.Setup();
            DependencyResolver.SetContainer(Container);
        }

        [Test]
        public void TestGetPageSegmentationModeFromString()
        {
            var app = new clsLocalTargetApp();

            // Test the correct values are returned for single character (10) and Auto (3),
            // that the parameter is case Insensitive and that the inputted string is 
            // trimmed of white spaces
            // Auto = 3
            Assert.That(app.GetPageSegmentationModeFromString("Auto "), Is.EqualTo(3));
            Assert.That(app.GetPageSegmentationModeFromString("  auto  "), Is.EqualTo(3));
            // Character = 10
            Assert.That(app.GetPageSegmentationModeFromString(" character"), Is.EqualTo(10));
            Assert.That(app.GetPageSegmentationModeFromString("  Character   "), Is.EqualTo(10));

            // Test the other values
            // OSD = 0
            Assert.That(app.GetPageSegmentationModeFromString("osd"), Is.EqualTo(0));
            // AutoWithOSD = 1
            Assert.That(app.GetPageSegmentationModeFromString("AutoWITHosd "), Is.EqualTo(1));
            // AutoNoOCR = 2
            Assert.That(app.GetPageSegmentationModeFromString(" autoNOOCR"), Is.EqualTo(2));
            // Column = 4
            Assert.That(app.GetPageSegmentationModeFromString(" column "), Is.EqualTo(4));
            // VerticalBlock = 5
            Assert.That(app.GetPageSegmentationModeFromString("verticalblock"), Is.EqualTo(5));
            // Block = 6
            Assert.That(app.GetPageSegmentationModeFromString("block"), Is.EqualTo(6));
            // Line = 7
            Assert.That(app.GetPageSegmentationModeFromString("Line"), Is.EqualTo(7));
            // Word = 8
            Assert.That(app.GetPageSegmentationModeFromString("Word"), Is.EqualTo(8));
            // CircledWord = 9
            Assert.That(app.GetPageSegmentationModeFromString("Circledword "), Is.EqualTo(9));

            // Test that values not contained in the enum PageSegMode throw an argument exception
            Assert.That(() => app.GetPageSegmentationModeFromString("notEvenARealValue"), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => app.GetPageSegmentationModeFromString(""), Throws.InstanceOf<ArgumentException>());
        }
    }
}

// #If UNITTESTS