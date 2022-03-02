using System;
using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class CheckExistsHandlerTests : UIAutomationHandlerTestBase<CheckExistsHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIACheckExists");
            }
        }

        [Test]
        public void ExecuteWithNoExceptionReturnsTrue()
        {
            var result = Execute(Query);
            Assert.AreEqual(Reply.True, result);
        }

        [Test]
        public void ExecuteWithApplicationExceptionReturnsFalse()
        {
            GetMock<IUIAutomationIdentifierHelper>().Setup(m => m.FindUIAutomationElement(It.IsAny<clsQuery>(), It.IsAny<int>())).Throws<ApplicationException>();
            var result = Execute(Query);
            Assert.AreEqual(Reply.False, result);
        }

        [Test]
        public void ExecuteWithOtherExceptionThrowsException()
        {
            GetMock<IUIAutomationIdentifierHelper>().Setup(m => m.FindUIAutomationElement(It.IsAny<clsQuery>(), It.IsAny<int>())).Throws<InvalidOperationException>();          
            Assert.Throws<InvalidOperationException>(() => Execute(Query));
        }
    }
}