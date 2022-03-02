using System.Linq;
using System.Xml.Linq;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetSelectedItemsHandlerTests : UIAutomationHandlerTestBase<GetSelectedItemsHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAGetSelectedItems");
            }
        }

        [Test]
        public void ExecuteReturnsOk()
        {
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>())).Returns(new[] { SetupListItemMock("Item 1", false) });
            var result = Execute(Query);
            Assert.IsTrue(result.IsResult);
        }

        [Test]
        public void ExecuteReturnsExpectedXml()
        {
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>())).Returns(new[] { SetupListItemMock("Item 1", false), SetupListItemMock("Item 2", true), SetupListItemMock("Item 3", false), SetupListItemMock("Item 4", true), SetupListItemMock("Item 5", false) });
            ElementMock.SetupGet(m => m.CurrentName).Returns("Test");
            var result = Execute(Query);
            var xml = XDocument.Parse(result.Message);
            var collection = xml.Root;
            var row = collection.Element("row");
            var field = row.Element("field");
            var nameAttribute = field.Attribute("name");
            var typeAttribute = field.Attribute("type");
            var valueAttribute = field.Attribute("value");
            Assert.AreEqual(2, collection.Elements("row").Count());
            Assert.AreEqual("Item 2", valueAttribute.Value);
        }

        private static IAutomationElement SetupListItemMock(string value, bool isSelected)
        {
            var selectionItemPatternMock = new Mock<ISelectionItemPattern>();
            selectionItemPatternMock.SetupGet(m => m.CurrentIsSelected).Returns(isSelected);
            var mock = new Mock<IAutomationElement>();
            mock.SetupGet(m => m.CurrentName).Returns(value);
            mock.Setup(m => m.GetCurrentPattern<ISelectionItemPattern>()).Returns(selectionItemPatternMock.Object);
            mock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionItemPattern))).Returns(true);
            return mock.Object;
        }
    }
}