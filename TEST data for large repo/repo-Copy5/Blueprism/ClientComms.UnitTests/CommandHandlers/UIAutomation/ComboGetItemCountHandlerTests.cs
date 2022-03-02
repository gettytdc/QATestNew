using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using BluePrism.Utilities.Functional;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class ComboGetItemCountHandlerTests : UIAutomationHandlerTestBase<ComboGetItemCountHandler>
    {
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void ExecuteComboGetItemCount(int count)
        {
            mTestMocks = new[] { new Mock<IAutomationElement>().Tee(SetupMockElement("Combobox1", "Combo 1",count)) };
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>(), It.IsAny<IAutomationCacheRequest>())).Returns(TestElementCollection);            
            ElementMock.MockPattern<ISelectionItemPattern>();
            ElementMock.MockPattern<IExpandCollapsePattern>();
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionPattern))).Returns(true);         
            var result = Execute(Query);
            Assert.IsTrue(result.IsResult);
            Assert.AreEqual(result.Message , count.ToString());
        }

        private clsQuery Query => clsQuery.Parse("UIAComboGetItemCount");

        private Action<Mock<IAutomationElement>> SetupMockElement(string name, string text,int count)
        {
            return (mock) =>
            {
                mock.SetupGet(m => m.CurrentName).Returns(text);
                mock.SetupGet(m => m.CurrentClassName).Returns(name);
                mock.SetupSequence(m => m.FindAll(It.IsAny<TreeScope>(), 
                                    It.IsAny<IAutomationCacheRequest>())).Returns(CreateInnderList(count));
                mock.Setup(m => m.PatternIsSupported(It.IsAny<PatternType>())).Returns(true);                
            };
        }

        private IReadOnlyCollection<IAutomationElement> CreateInnderList(int count)
        {
            var list = new List<IAutomationElement>();
            for (int i=0;i<count;i++)
            {
                var innerMock = new Mock<IAutomationElement>();
                innerMock.Setup(m => m.PatternIsSupported(It.IsAny<PatternType>())).Returns(true);
                list.Add(innerMock.Object);
            }
            return list.AsReadOnly();
        }

        private IReadOnlyCollection<IAutomationElement> TestElementCollection => mTestMocks.Select(x => x.Object).ToList();

        private IReadOnlyCollection<Mock<IAutomationElement>> mTestMocks;
    }
}
