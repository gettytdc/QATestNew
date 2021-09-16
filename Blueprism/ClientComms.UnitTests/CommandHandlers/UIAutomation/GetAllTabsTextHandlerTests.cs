using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Conditions;
using BluePrism.Utilities.Functional;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetAllTabsTextHandlerTests : UIAutomationHandlerTestBase<GetAllTabsTextHandler>
    {
        public override void Setup()
        {
            base.Setup();
            mTestMocks = new[] { new Mock<IAutomationElement>().Tee(SetupMockElement("elm1", "Elm 1")), 
                                 new Mock<IAutomationElement>().Tee(SetupMockElement("elm2", "Elm 2")), 
                                 new Mock<IAutomationElement>().Tee(SetupMockElement("elm3", "Elm 3")) };

            ElementMock.Setup(m => m.FindAll(It.Is<TreeScope>(t => t == TreeScope.Children),
                                             It.IsAny<IAutomationCondition>())).Returns(TestElementCollection);
        }

        [Test]
        public void ExecuteGetAllTabsText()
        {
            var result = Execute(Query);
            Assert.IsTrue(result.IsResult);
            Assert.AreEqual(result.Message, TestElementCollection.AsCollectionXml());
        }

        private clsQuery Query => clsQuery.Parse("UIAGetAllTabsText");

        private Action<Mock<IAutomationElement>> SetupMockElement(string name, string text)
        {
            return (mock) =>
            {
                mock.SetupGet(m => m.CurrentName).Returns(text);
                mock.SetupGet(m => m.CurrentClassName).Returns(name);
            };
        }

        private IReadOnlyCollection<IAutomationElement> TestElementCollection => mTestMocks.Select(x => x.Object).ToList();       
        private IReadOnlyCollection<Mock<IAutomationElement>> mTestMocks;
    }
}
