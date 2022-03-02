using BluePrism.AutomateAppCore;
using NUnit.Framework;
using System;
using System.ComponentModel;

namespace AutomateAppCore.UnitTests.Server
{
    /// <summary>
    /// Tests the <see cref="ServerConnection"/> class, including the
    /// <see cref="ServerConnection.Mode"/> enum.
    /// </summary>
    [TestFixture]
    public class ServerConnectionTests
    {
        /// <summary>
        /// Tests that every <see cref="ServerConnection.Mode"/> has a
        /// description defined. If this test fails, the mode needs to be added to
        /// the <see cref="ServerConnection.GetDescription"/> function.
        /// </summary>
        [Test]
        public void TestGetDescription()
        {
            foreach (ServerConnection.Mode e in Enum.GetValues(typeof(ServerConnection.Mode)))
                Assert.That(ServerConnection.GetDescription(e), Is.Not.EqualTo(""));
        }

        /// <summary>
        /// Tests that every <see cref="ServerConnection.Mode"/> has help text
        /// defined. If this test fails, the mode needs to be added to the
        /// <see cref="ServerConnection.GetHelpText"/> function.
        /// </summary>
        [Test]
        public void TestGetHelpText()
        {
            foreach (ServerConnection.Mode e in Enum.GetValues(typeof(ServerConnection.Mode)))
                Assert.That(ServerConnection.GetHelpText(e), Is.Not.EqualTo(""));
        }

        /// <summary>
        /// Tests whether all mode enums have a readable description when retrieved
        /// via the enums underlying convertor, which is how the display text is
        /// retrieved in drop down lists.
        /// </summary>
        [Test]
        public void TestAllConnectionModesGetAReadableDescriptionThroughConverter()
        {
            foreach (Enum e in Enum.GetValues(typeof(ServerConnection.Mode)))
                Assert.That(TypeDescriptor.GetConverter(e).ConvertToString(e), Is.Not.EqualTo(""));
        }
    }
}
