#if UNITTESTS

namespace BluePrism.UIAutomation.UnitTests
{
    using System;

    using Conditions;
    using Moq;

    using NUnit.Framework;
    using BluePrism.Utilities.Testing;

    public class AutomationHelperTests : UnitTestBase<AutomationHelper>
    {

        [Test]
        public void GetWindowHandleCacheValid()
        {
            var expectedResult = new IntPtr(1234);

            var automationFactoryMock = GetMock<IAutomationFactory>();
            automationFactoryMock
                .Setup(m => m.CreateNotCondition(It.IsAny<IAutomationCondition>()))
                .Returns(() => GetMock<IAutomationCondition>().Object);
            automationFactoryMock
                .Setup(m => m.CreateTreeWalker(It.IsAny<IAutomationCondition>()))
                .Returns(() => GetMock<IAutomationTreeWalker>().Object);
            automationFactoryMock
                .Setup(m => m.CreateCacheRequest())
                .Returns(() => GetMock<IAutomationCacheRequest>().Object);

            var automationTreeWalkerMock = GetMock<IAutomationTreeWalker>();
            automationTreeWalkerMock
                .Setup(m => m.Normalize(
                    It.IsAny<IAutomationElement>(),
                    It.IsAny<IAutomationCacheRequest>()))
                .Returns(() => GetMock<IAutomationElement>().Object);

            var automationElementMock = GetMock<IAutomationElement>();
            automationElementMock
                .Setup(m => m.CachedNativeWindowHandle)
                .Returns(expectedResult);

            var result =
                ClassUnderTest.GetWindowHandle(GetMock<IAutomationElement>().Object);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetWindowHandleCacheInvalid()
        {
            var expectedResult = new IntPtr(1234);

            var automationFactoryMock = GetMock<IAutomationFactory>();
            automationFactoryMock
                .Setup(m => m.CreateNotCondition(It.IsAny<IAutomationCondition>()))
                .Returns(() => GetMock<IAutomationCondition>().Object);
            automationFactoryMock
                .Setup(m => m.CreateTreeWalker(It.IsAny<IAutomationCondition>()))
                .Returns(() => GetMock<IAutomationTreeWalker>().Object);
            automationFactoryMock
                .Setup(m => m.CreateCacheRequest())
                .Returns(() => GetMock<IAutomationCacheRequest>().Object);

            var automationTreeWalkerMock = GetMock<IAutomationTreeWalker>();
            automationTreeWalkerMock
                .Setup(m => m.Normalize(
                    It.IsAny<IAutomationElement>(),
                    It.IsAny<IAutomationCacheRequest>()))
                .Returns(() => GetMock<IAutomationElement>().Object);

            var automationElementMock = GetMock<IAutomationElement>();
            automationElementMock
                .Setup(m => m.CachedNativeWindowHandle)
                .Throws<Exception>();
            automationElementMock
                .Setup(m => m.CurrentNativeWindowHandle)
                .Returns(expectedResult);

            var result =
                ClassUnderTest.GetWindowHandle(GetMock<IAutomationElement>().Object);

            Assert.AreEqual(expectedResult, result);
        }
    }
}

#endif