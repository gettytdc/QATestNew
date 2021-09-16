using System.Windows.Forms;
using NUnit.Framework;
using FluentAssertions;
using BluePrism.AutomateAppCore.EnvironmentFunctions;
using BluePrism.AutomateProcessCore;
using BluePrism.Core.Utility;
using BluePrism.Utilities.Testing;
using Moq;

namespace AutomateAppCore.UnitTests.EnvironmentFunctions
{
    [TestFixture]
    public class GetClipboardFunctionTests : UnitTestBase<GetClipboardFunction>
    {
        private static string ClipboardContents => "Test";
        private Mock<IDataObject> _dataObjectMock;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _dataObjectMock = GetMock<IDataObject>();
            _dataObjectMock.Setup(o => o.GetDataPresent(DataFormats.Text)).Returns(true);
            _dataObjectMock.Setup(o => o.GetDataPresent(DataFormats.UnicodeText)).Returns(true);
            _dataObjectMock.Setup(o => o.GetData(DataFormats.UnicodeText)).Returns(ClipboardContents);

            GetMock<IClipboard>().Setup(o => o.GetDataObject()).Returns(_dataObjectMock.Object);
        }

        [Test]
        public void Evaluate_WithUnicodeTextData_ReturnsUnicodeText()
        {
            _dataObjectMock.Setup(o => o.GetDataPresent(DataFormats.Text)).Returns(false);

            var clipboardData = ClassUnderTest.Evaluate(null, null);

            clipboardData.Should().Be(new clsProcessValue(ClipboardContents));
        }

        [Test]
        public void Evaluate_WithTextData_ReturnsUnicodeText()
        {
            _dataObjectMock.Setup(o => o.GetDataPresent(DataFormats.UnicodeText)).Returns(false);

            var clipboardData = ClassUnderTest.Evaluate(null, null);

            clipboardData.Should().Be(new clsProcessValue(ClipboardContents));
        }

        [Test]
        public void Evaluate_WithNoTextAndUnicodeTextData_ReturnsEmptyString()
        {
            _dataObjectMock.Setup(o => o.GetDataPresent(DataFormats.Text)).Returns(false);
            _dataObjectMock.Setup(o => o.GetDataPresent(DataFormats.UnicodeText)).Returns(false);

            var clipboardData = ClassUnderTest.Evaluate(null, null);

            clipboardData.Should().Be(new clsProcessValue(string.Empty));
        }
    }
}