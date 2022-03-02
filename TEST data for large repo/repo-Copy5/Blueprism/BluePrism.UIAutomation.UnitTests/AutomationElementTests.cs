#if UNITTESTS

namespace BluePrism.UIAutomation.UnitTests
{
    using System.Linq;
    using BluePrism.UnitTesting;
    using Conditions;

    using Moq;

    using NUnit.Framework;

    using Patterns;
    using BluePrism.Utilities.Testing;

    public class AutomationElementTests : UnitTestBase<AutomationElement>
    {
        protected override AutomationElement TestClassConstructor() =>
            new AutomationElement(
                GetMock<UIAutomationClient.IUIAutomationElement>().Object,
                GetMock<IAutomationPatternFactory>().Object,
                GetMock<IAutomationFactory>().Object,
                GetMock<IAutomationTreeNavigationHelper>().Object);
            
        [Test]
        public void FindFirstWhenFound()
        {
            var uiAutomationElementMock = GetMock<UIAutomationClient.IUIAutomationElement>();
            uiAutomationElementMock
                .Setup(m => m.FindFirstBuildCache(
                    It.IsAny<UIAutomationClient.TreeScope>(),
                    It.IsAny<UIAutomationClient.IUIAutomationCondition>(),
                    It.IsAny<UIAutomationClient.IUIAutomationCacheRequest>()
                    ))
                .Returns(() => GetMock<UIAutomationClient.IUIAutomationElement>().Object);
            uiAutomationElementMock
                .SetupGet(m => m.CurrentName)
                .Returns("This is a test");

            var cacheMock = GetMock<IAutomationCacheRequest>();

            var automationFactoryMock = GetMock<IAutomationFactory>();
            automationFactoryMock
                .Setup(m => m.FromUIAutomationElement(It.IsAny<UIAutomationClient.IUIAutomationElement>()))
                .Returns(() => ClassUnderTest);

            automationFactoryMock
                .Setup(m => m.CreateCacheRequest())
                .Returns(() => cacheMock.Object);

            var result = ClassUnderTest.FindFirst(TreeScope.None, GetMock<IAutomationCondition>().Object);

            Assert.AreEqual("This is a test", result.CurrentName);
        }

        [Test]
        public void FindFirstWhenNotFound()
        {
            var uiAutomationElementMock = GetMock<UIAutomationClient.IUIAutomationElement>();
            uiAutomationElementMock
                .Setup(m => m.FindFirstBuildCache(
                    It.IsAny<UIAutomationClient.TreeScope>(),
                    It.IsAny<UIAutomationClient.IUIAutomationCondition>(),
                    It.IsAny<UIAutomationClient.IUIAutomationCacheRequest>()
                    ))
                .Returns((UIAutomationClient.IUIAutomationElement) null);

            var cacheMock = GetMock<IAutomationCacheRequest>();

            var automationFactoryMock = GetMock<IAutomationFactory>();
            automationFactoryMock
               .Setup(m => m.CreateCacheRequest())
               .Returns(() => cacheMock.Object);

            var result = ClassUnderTest.FindFirst(TreeScope.None, GetMock<IAutomationCondition>().Object);

            Assert.IsNull(result);
        }

        [Test]
        public void FindAll()
        {
            var uiAutomationElementMock = GetMock<UIAutomationClient.IUIAutomationElement>();
            uiAutomationElementMock
                .SetupSequence(m => m.CurrentName)
                .Returns("Test1")
                .Returns("Test2")
                .Returns("Test3");
            uiAutomationElementMock
                .Setup(m => m.FindAllBuildCache(
                    It.IsAny<UIAutomationClient.TreeScope>(),
                    It.IsAny<UIAutomationClient.IUIAutomationCondition>(),
                    It.IsAny<UIAutomationClient.IUIAutomationCacheRequest>()
                    ))
                .Returns(() => GetMock<UIAutomationClient.IUIAutomationElementArray>().Object);

            var cacheMock = GetMock<IAutomationCacheRequest>();

            var automationFactoryMock = GetMock<IAutomationFactory>();
            automationFactoryMock
                .Setup(m => m.FromUIAutomationElement(It.IsAny<UIAutomationClient.IUIAutomationElement>()))
                .Returns(() => ClassUnderTest);

            automationFactoryMock
                .Setup(m => m.CreateCacheRequest())
                .Returns(() => cacheMock.Object);

            var uiAutomationElementArrayMock = GetMock<UIAutomationClient.IUIAutomationElementArray>();
            uiAutomationElementArrayMock
                .SetupGet(m => m.Length).Returns(3);
            uiAutomationElementArrayMock
                .Setup(m => m.GetElement(It.IsAny<int>()))
                .Returns(uiAutomationElementMock.Object);

            var result =
                ClassUnderTest.FindAll(TreeScope.None, GetMock<IAutomationCondition>().Object).ToList();

            GetMock<UIAutomationClient.IUIAutomationElementArray>()
                .Verify(m => m.GetElement(It.IsAny<int>()), Times.Exactly(3));

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Test1", result.ElementAt(0).CurrentName);
            Assert.AreEqual("Test2", result.ElementAt(1).CurrentName);
            Assert.AreEqual("Test3", result.ElementAt(2).CurrentName);
        }

        [Test]
        public void SetFocus()
        {
            ClassUnderTest.SetFocus();

            GetMock<UIAutomationClient.IUIAutomationElement>()
                .Verify(m => m.SetFocus(), Times.Once);
        }

        [Test]
        public void GetCurrentPatternWhenSupported()
        {
            var automationPatternFactory = GetMock<IAutomationPatternFactory>();
            automationPatternFactory
                .Setup(m => m.GetCurrentPattern(It.IsAny<IAutomationElement>(), It.Is<PatternType>(v => v == PatternType.InvokePattern)))
                .Returns(() => GetMock<IAutomationPattern>().Object);

            var result = ClassUnderTest.GetCurrentPattern(PatternType.InvokePattern);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetCurrentPatternWhenNotSupported()
        {
            var automationPatternFactory = GetMock<IAutomationPatternFactory>();
            automationPatternFactory
                .Setup(m => m.GetCurrentPattern(It.IsAny<IAutomationElement>(), It.Is<PatternType>(v => v == PatternType.InvokePattern)))
                .Returns((IAutomationPattern)null);

            var result = ClassUnderTest.GetCurrentPattern(PatternType.InvokePattern);

            Assert.IsNull(result);
        }

        [Test]
        public void GetElementPath()
        {
            var automationFactoryMock = GetMock<IAutomationFactory>();
            automationFactoryMock
                .SetupSequence(m => m.GetParentElement(It.IsAny<IAutomationElement>()))
                .Returns(GetMock<IAutomationElement>().Object)
                .Returns(GetMock<IAutomationElement>().Object)
                .Returns(GetMock<IAutomationElement>().Object)
                .Returns(GetMock<IAutomationElement>().Object)
                .Returns((IAutomationElement)null);

            var automationElementMock = GetMock<IAutomationElement>();
            automationElementMock
                .SetupSequence(m => m.CurrentAutomationId)
                .Returns("1")
                .Returns("2")
                .Returns("3/3")
                .Returns("4\\4");

            var result =
                ClassUnderTest.GetElementPath(GetMock<IAutomationElement>().Object);

            Assert.AreEqual(@"1/2/3\/3/4\\4/", result);
        }
    }
}

#endif