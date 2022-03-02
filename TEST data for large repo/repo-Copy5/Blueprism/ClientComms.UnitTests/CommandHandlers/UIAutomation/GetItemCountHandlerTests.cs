using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Conditions;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetItemCountHandlerTests : UIAutomationHandlerTestBase<GetItemCountHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAGetItemCount");
            }
        }

        [Test]
        public void ExecuteWithInvokePatternReturnsOk()
        {
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(new[] { ElementMock.Object });
            var result = Execute(Query);
            Assert.AreEqual(result.Message, "1");
        }
    }
}