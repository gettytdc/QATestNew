using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using BluePrism.Utilities.Functional;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class TreeAddToSelectionHandlerTests : UIAutomationHandlerTestBase<TreeAddToSelectionHandler>
    {
        public override void Setup()
        {
            base.Setup();
            mTestMocks = new[] { new Mock<IAutomationElement>().Tee(SetupMockElement("item1", "Item 1")), new Mock<IAutomationElement>().Tee(SetupMockElement("item2", "Item 2")), new Mock<IAutomationElement>().Tee(SetupMockElement("item3", "Item 3")), new Mock<IAutomationElement>().Tee(SetupMockElement("item4", "Item 4")), new Mock<IAutomationElement>().Tee(SetupMockElement("item5", "Item 5")), new Mock<IAutomationElement>().Tee(SetupMockElement("sameName1", "Same name")), new Mock<IAutomationElement>().Tee(SetupMockElement("sameName2", "Same name")) };
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>())).Returns(TestElementCollection);
        }

        [Test]
        public void ExecuteWithValidElementReturnsOk()
        {
            var result = Execute(GetQuery("Item 1"));
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteWithValidElementAddsToSelection()
        {
            Execute(GetQuery("Item 1"));
            GetMock<ISelectionItemPattern>().Verify(m => m.AddToSelection(), Times.AtLeastOnce());
        }

        [Test]
        public void ExecuteFollowsMatchIndex()
        {
            Execute(GetQuery(null, 7));
            FindTreeNodeMock("sameName2").Verify(m => m.GetCurrentPattern<ISelectionItemPattern>(), Times.Once());
        }

        private Mock<IAutomationElement> FindTreeNodeMock(string name)
        {
            return mTestMocks.Single(x => (x.Object.CurrentClassName ?? "") == (name ?? ""));
        }

        private static clsQuery GetQuery(string name, int matchIndex = 1)
        {
            return clsQuery.Parse($"UIATreeAddToSelection IDName=\"{name}\" Position={matchIndex}");
        }

        private Action<Mock<IAutomationElement>> SetupMockElement(string name, string text)
        {
            return (mock) =>
                            {
                                mock.SetupGet(m => m.CurrentName).Returns(text);
                                mock.SetupGet(m => m.CurrentClassName).Returns(name);
                                mock.Setup(m => m.GetCurrentPattern<ISelectionItemPattern>()).Returns(GetMock<ISelectionItemPattern>().Object);
                                mock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionItemPattern))).Returns(true);
                            };
        }

        private IReadOnlyCollection<IAutomationElement> TestElementCollection
        {
            get
            {
                return mTestMocks.Select(x => x.Object).ToList();
            }
        }

        private IReadOnlyCollection<Mock<IAutomationElement>> mTestMocks;
    }
}