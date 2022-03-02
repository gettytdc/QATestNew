#if UNITTESTS

namespace BluePrism.UIAutomation.UnitTests
{
    using System;

    using Conditions;
    using Utilities.Functional;

    using Moq;

    using NUnit.Framework;

    using Patterns;
    using BluePrism.Utilities.Testing;

    [TestFixture]
    public class AutomationFactoryTests : UnitTestBase<AutomationFactory>
    {
        private Mock<UIAutomationClient.IUIAutomation> _uiAutomationMock;

        // For some reason the DI framework isn't correctly injecting the COM
        // dependency so have to manually do it here
        protected override AutomationFactory TestClassConstructor() =>
            new AutomationFactory(
                _uiAutomationMock.Object,
                GetMock<IAutomationPatternFactory>().Object,
                e => GetMock<IAutomationElement>()
                    .Tee(x => x.SetupGet(m => m.CurrentName).Returns(e.CurrentName))
                    .Object);

        [Test]
        public void GetFocusedElementWhenExists()
        {
            var mockElement = GetMock<UIAutomationClient.IUIAutomationElement>();
            mockElement
                .SetupGet(m => m.CurrentName)
                .Returns("This is a test");

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.GetFocusedElement())
                .Returns(() => mockElement.Object);

            var result = ClassUnderTest.GetFocusedElement();

            Assert.AreEqual("This is a test", result.CurrentName);
        }

        [Test]
        public void GetFocusedElementWhenNotExists()
        {
            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.GetFocusedElement());

            var result = ClassUnderTest.GetFocusedElement();

            Assert.IsNull(result);
        }

        [Test]
        public void GetRootElement()
        {
            var uiAutomationElement = GetMock<UIAutomationClient.IUIAutomationElement>();
            uiAutomationElement
                .SetupGet(m => m.CurrentName).Returns("This is a test");

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.GetRootElement())
                .Returns(uiAutomationElement.Object);

            var result = ClassUnderTest.GetRootElement();

            Assert.AreEqual("This is a test", result.CurrentName);
        }

        [Test]
        public void FromHandleWhenExists()
        {
            var argument = new IntPtr(1234);

            var uiAutomationElement = GetMock<UIAutomationClient.IUIAutomationElement>();
            uiAutomationElement
                .SetupGet(m => m.CurrentName).Returns("This is a test");

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.ElementFromHandle(It.Is<IntPtr>(v => v == argument)))
                .Returns(() => uiAutomationElement.Object);

            var result = ClassUnderTest.FromHandle(argument);

            Assert.AreEqual("This is a test", result.CurrentName);
        }

        [Test]
        public void FromHandleWhenNotExists()
        {
            var argument = new IntPtr(1234);

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.ElementFromHandle(It.Is<IntPtr>(v => v == argument)));

            var result = ClassUnderTest.FromHandle(argument);

            Assert.IsNull(result);
        }

        [Test]
        public void FromPointWhenExists()
        {
            var argument = new System.Drawing.Point(12, 34);

            var uiAutomationElement = GetMock<UIAutomationClient.IUIAutomationElement>();
            uiAutomationElement
                .SetupGet(m => m.CurrentName).Returns("This is a test");

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.ElementFromPoint(It.Is<UIAutomationClient.tagPOINT>(v => v.x == (int)argument.X && v.y == (int)argument.Y)))
                .Returns(uiAutomationElement.Object);

            var result = ClassUnderTest.FromPoint(argument);

            Assert.AreEqual("This is a test", result.CurrentName);
        }

        [Test]
        public void FromPointWhenNotExists()
        {
            var argument = new System.Drawing.Point(12, 34);

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.ElementFromPoint(
                    It.Is<UIAutomationClient.tagPOINT>(v => v.x == (int)argument.X && v.y == (int)argument.Y)));

            var result = ClassUnderTest.FromPoint(argument);

            Assert.IsNull(result);
        }

        [Test]
        public void CreateCacheRequest()
        {
            var uiAutomationCacheRequest = GetMock<UIAutomationClient.IUIAutomationCacheRequest>();
            uiAutomationCacheRequest
                .SetupGet(m => m.AutomationElementMode)
                .Returns(UIAutomationClient.AutomationElementMode.AutomationElementMode_Full);

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreateCacheRequest())
                .Returns(uiAutomationCacheRequest.Object);

            var result = ClassUnderTest.CreateCacheRequest();

            Assert.AreEqual(AutomationElementMode.Full, result.AutomationElementMode);
        }

        [Test]
        public void CreateAndConditionTwoParameters()
        {
            var condition = GetMock<IAutomationCondition>().Object;

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreateAndCondition(It.IsAny<UIAutomationClient.IUIAutomationCondition>(), It.IsAny<UIAutomationClient.IUIAutomationCondition>()))
                .Returns(GetMock<UIAutomationClient.IUIAutomationAndCondition>().Object);

            var result = ClassUnderTest.CreateAndCondition(condition, condition);

            Assert.That(result is AndCondition);
        }

        [Test]
        public void CreateAndConditionMultipleParameters()
        {
            var condition = GetMock<IAutomationCondition>().Object;

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreateAndCondition(It.IsAny<UIAutomationClient.IUIAutomationCondition>(), It.IsAny<UIAutomationClient.IUIAutomationCondition>()))
                .Returns(GetMock<UIAutomationClient.IUIAutomationAndCondition>().Object);

            var result = ClassUnderTest.CreateAndCondition(condition, condition, condition, condition);

            Assert.That(result is AndCondition);
        }

        [Test]
        public void CreateAndConditionTooFewParameters()
        {
            var condition = GetMock<IAutomationCondition>().Object;

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreateAndCondition(It.IsAny<UIAutomationClient.IUIAutomationCondition>(), It.IsAny<UIAutomationClient.IUIAutomationCondition>()))
                .Returns(GetMock<UIAutomationClient.IUIAutomationAndCondition>().Object);

            void Test() => ClassUnderTest.CreateAndCondition(condition);

            Assert.Throws<ArgumentException>(Test);
        }

        [Test]
        public void CreateFalseCondition()
        {
            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreateFalseCondition())
                .Returns(GetMock<UIAutomationClient.IUIAutomationCondition>().Object);

            var result = ClassUnderTest.CreateFalseCondition();

            Assert.That(result is FalseCondition);
        }

        [Test]
        public void CreateTrueCondition()
        {
            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreateTrueCondition())
                .Returns(GetMock<UIAutomationClient.IUIAutomationCondition>().Object);

            var result = ClassUnderTest.CreateTrueCondition();

            Assert.That(result is TrueCondition);
        }

        [Test]
        public void CreateOrCondition()
        {
            var condition = GetMock<IAutomationCondition>().Object;

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreateOrCondition(It.IsAny<UIAutomationClient.IUIAutomationCondition>(), It.IsAny<UIAutomationClient.IUIAutomationCondition>()))
                .Returns(GetMock<UIAutomationClient.IUIAutomationOrCondition>().Object);

            var result = ClassUnderTest.CreateOrCondition(condition, condition);

            Assert.That(result is OrCondition);
        }

        [Test]
        public void CreatePropertyCondition()
        {
            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreatePropertyCondition(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(GetMock<UIAutomationClient.IUIAutomationPropertyCondition>().Object);

            var result = ClassUnderTest.CreatePropertyCondition(PropertyType.AcceleratorKey, 123);

            Assert.That(result is PropertyCondition);
        }

        [Test]
        public void CreateTreeWalker()
        {
            var condition = GetMock<IAutomationCondition>().Object;

            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreateTreeWalker(It.IsAny<UIAutomationClient.IUIAutomationCondition>()))
                .Returns(GetMock<UIAutomationClient.IUIAutomationTreeWalker>().Object);

            var result = ClassUnderTest.CreateTreeWalker(condition);

            Assert.That(result is AutomationTreeWalker);
        }

        [Test]
        public void CreateControlTreeWalker()
        {
            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();
            _uiAutomationMock
                .Setup(m => m.CreateTreeWalker(It.IsAny<UIAutomationClient.IUIAutomationCondition>()))
                .Returns(GetMock<UIAutomationClient.IUIAutomationTreeWalker>().Object);
            _uiAutomationMock
                .Setup(m => m.CreateNotCondition(It.IsAny<UIAutomationClient.IUIAutomationCondition>()))
                .Returns(GetMock<UIAutomationClient.IUIAutomationCondition>().Object);
            _uiAutomationMock
                .Setup(m => m.CreatePropertyCondition(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(GetMock<UIAutomationClient.IUIAutomationPropertyCondition>().Object);

            var result = ClassUnderTest.CreateControlTreeWalker();

            Assert.That(result is AutomationTreeWalker);
        }

        [Test]
        public void CreateGridTextProvider()
        {
            _uiAutomationMock = GetMock<UIAutomationClient.IUIAutomation>();

            var automationElementMock = GetMock<IAutomationElement>();
            automationElementMock
                .Setup(m => m.PatternIsSupported(It.IsAny<PatternType>()))
                .Returns(false);

            var grid = GetMock<IGridPattern>();
            grid
                .Setup(m => m.GetItem(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => automationElementMock.Object);

            var gridTextProvider = ClassUnderTest.GetGridTextProvider();

            Assert.That(gridTextProvider is IAutomationGridText);
        }
    }
}

#endif