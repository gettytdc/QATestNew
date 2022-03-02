using System.Xml.Linq;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Conditions;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetAllItemsHandlerTests : UIAutomationHandlerTestBase<GetAllItemsHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAGetAllItems");
            }
        }

        [Test]
        public void ExecuteReturnsOk()
        {
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(new[] { ElementMock.Object });
            var result = Execute(Query);
            Assert.IsTrue(result.IsResult);
        }

        [Test]
        public void ExecuteReturnsExpectedXml()
        {
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(new[] { ElementMock.Object });
            ElementMock.SetupGet(m => m.CurrentName).Returns("Test");
            var result = Execute(Query);
            var xml = XDocument.Parse(result.Message);
            var collection = xml.Root;
            var row = collection.Element("row");
            var field = row.Element("field");
            var nameAttribute = field.Attribute("name");
            var typeAttribute = field.Attribute("type");
            var valueAttribute = field.Attribute("value");
            Assert.AreEqual("Test", valueAttribute.Value);
        }
    }
}