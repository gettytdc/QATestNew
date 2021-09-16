#if UNITTESTS

namespace BluePrism.UIAutomation.UnitTests
{
    using Moq;

    using NUnit.Framework;
    using BluePrism.Utilities.Testing;

    public class AutomationTreeWalkerTests : UnitTestBase<AutomationTreeWalker>
    {
        protected override AutomationTreeWalker TestClassConstructor() =>
            new AutomationTreeWalker(
                GetMock<UIAutomationClient.IUIAutomationTreeWalker>().Object,
                GetMock<IAutomationFactory>().Object);

        [Test]
        public void GetParentWhenExists()
        {
            var uiAutomationTreeWalkerMock = GetMock<UIAutomationClient.IUIAutomationTreeWalker>();
            uiAutomationTreeWalkerMock
                .Setup(m => m.GetParentElement(It.IsAny<UIAutomationClient.IUIAutomationElement>()))
                .Returns(() => GetMock<UIAutomationClient.IUIAutomationElement>().Object);

            var automationFactoryMock = GetMock<IAutomationFactory>();
            automationFactoryMock
                .Setup(m => m.FromUIAutomationElement(It.IsAny<UIAutomationClient.IUIAutomationElement>()))
                .Returns(() => GetMock<IAutomationElement>().Object);

            var result = ClassUnderTest.GetParent(GetMock<IAutomationElement>().Object);

            GetMock<UIAutomationClient.IUIAutomationTreeWalker>()
                .Verify(m => m.GetParentElement(It.IsAny<UIAutomationClient.IUIAutomationElement>()), Times.Once);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetParentWhenNotExists()
        {
            var uiAutomationTreeWalkerMock = GetMock<UIAutomationClient.IUIAutomationTreeWalker>();
            uiAutomationTreeWalkerMock
                .Setup(m => m.GetParentElement(It.IsAny<UIAutomationClient.IUIAutomationElement>()))
                .Returns((UIAutomationClient.IUIAutomationElement)null);

            var result = ClassUnderTest.GetParent(GetMock<IAutomationElement>().Object);

            Assert.IsNull(result);
        }

        [Test]
        public void GetParentWithCachingWhenExists()
        {
            var uiAutomationTreeWalkerMock = GetMock<UIAutomationClient.IUIAutomationTreeWalker>();
            uiAutomationTreeWalkerMock
                .Setup(
                    m => m.GetParentElementBuildCache(
                        It.IsAny<UIAutomationClient.IUIAutomationElement>(),
                        It.IsAny<UIAutomationClient.IUIAutomationCacheRequest>()))
                .Returns(() => GetMock<UIAutomationClient.IUIAutomationElement>().Object);

            var automationFactoryMock = GetMock<IAutomationFactory>();
            automationFactoryMock
                .Setup(m => m.FromUIAutomationElement(It.IsAny<UIAutomationClient.IUIAutomationElement>()))
                .Returns(() => GetMock<IAutomationElement>().Object);

            var result = ClassUnderTest.GetParent(
                GetMock<IAutomationElement>().Object,
                GetMock<IAutomationCacheRequest>().Object);

            uiAutomationTreeWalkerMock
                .Verify(
                    m => m.GetParentElementBuildCache(
                        It.IsAny<UIAutomationClient.IUIAutomationElement>(),
                        It.IsAny<UIAutomationClient.IUIAutomationCacheRequest>()),
                    Times.Once);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetParentWithCachingWhenNotExists()
        {
            var uiAutomationTreeWalkerMock = GetMock<UIAutomationClient.IUIAutomationTreeWalker>();
            uiAutomationTreeWalkerMock
                .Setup(
                    m => m.GetParentElementBuildCache(
                        It.IsAny<UIAutomationClient.IUIAutomationElement>(),
                        It.IsAny<UIAutomationClient.IUIAutomationCacheRequest>()))
                .Returns((UIAutomationClient.IUIAutomationElement)null);

            var result = ClassUnderTest.GetParent(
                GetMock<IAutomationElement>().Object,
                GetMock<IAutomationCacheRequest>().Object);

            Assert.IsNull(result);
        }
        [Test]
        public void NormalizeWithCachingWhenExists()
        {
            var uiAutomationTreeWalkerMock = GetMock<UIAutomationClient.IUIAutomationTreeWalker>();
            uiAutomationTreeWalkerMock
                .Setup(
                    m => m.NormalizeElementBuildCache(
                        It.IsAny<UIAutomationClient.IUIAutomationElement>(),
                        It.IsAny<UIAutomationClient.IUIAutomationCacheRequest>()))
                .Returns(() => GetMock<UIAutomationClient.IUIAutomationElement>().Object);

            var automationFactoryMock = GetMock<IAutomationFactory>();
            automationFactoryMock
                .Setup(m => m.FromUIAutomationElement(It.IsAny<UIAutomationClient.IUIAutomationElement>()))
                .Returns(() => GetMock<IAutomationElement>().Object);

            var result = ClassUnderTest.Normalize(
                GetMock<IAutomationElement>().Object,
                GetMock<IAutomationCacheRequest>().Object);

            GetMock<UIAutomationClient.IUIAutomationTreeWalker>()
                .Verify(
                    m => m.NormalizeElementBuildCache(
                        It.IsAny<UIAutomationClient.IUIAutomationElement>(),
                        It.IsAny<UIAutomationClient.IUIAutomationCacheRequest>()),
                    Times.Once);

            Assert.IsNotNull(result);
        }

        [Test]
        public void NormalizeWithCachingWhenNotExists()
        {
            var uiAutomationTreeWalkerMock = GetMock<UIAutomationClient.IUIAutomationTreeWalker>();
            uiAutomationTreeWalkerMock
                .Setup(
                    m => m.NormalizeElementBuildCache(
                        It.IsAny<UIAutomationClient.IUIAutomationElement>(),
                        It.IsAny<UIAutomationClient.IUIAutomationCacheRequest>()))
                .Returns((UIAutomationClient.IUIAutomationElement)null);

            var result = ClassUnderTest.Normalize(
                GetMock<IAutomationElement>().Object,
                GetMock<IAutomationCacheRequest>().Object);

            Assert.IsNull(result);
        }

    }
}

#endif