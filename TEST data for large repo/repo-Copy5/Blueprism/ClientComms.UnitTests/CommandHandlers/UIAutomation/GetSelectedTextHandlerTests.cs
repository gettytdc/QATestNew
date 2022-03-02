using System;
using System.Collections.Generic;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetSelectedTextHandlerTests: UIAutomationHandlerTestBase<GetSelectedTextHandler>
    {
        private clsQuery Query => clsQuery.Parse("UIAGetSelectedText");

        [Test]
        public void Execute_GetTextReturnsExpected()
        {
            const string testText = "TestText";

            var automationTextRangeMock = new Mock<IAutomationTextRange>();
            automationTextRangeMock.Setup(m => m.GetText(It.IsAny<int>())).Returns(testText);

            var iEnumerableOfAutomationTextRangeMock = new List<IAutomationTextRange> {automationTextRangeMock.Object};

            var textPatternMock = ElementMock.MockPattern<ITextPattern>();
            textPatternMock.Setup(m => m.GetSelection()).Returns(iEnumerableOfAutomationTextRangeMock);

            var reply = Execute(Query);

            Assert.That(reply.Message, Is.EqualTo(testText));
        }
    }
}
