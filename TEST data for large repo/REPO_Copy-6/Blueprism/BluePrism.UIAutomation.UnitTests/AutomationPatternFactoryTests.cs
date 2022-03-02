#if UNITTESTS

namespace BluePrism.UIAutomation.UnitTests
{
    using System;
    using System.Linq;
    using Moq;

    using NUnit.Framework;

    using Patterns;
    using BluePrism.Utilities.Testing;
    using Autofac;

    public class AutomationPatternFactoryTests : UnitTestBase<AutomationPatternFactory>
    {
        private Mock<IAutomationPattern> _automationPatternMock;

        public override void Setup()
        {
            base.Setup(builder =>
            {
                builder.RegisterInstance<Func<(PatternType, IAutomationElement), IAutomationPattern>>(_ => _automationPatternMock?.Object);
            });
            _automationPatternMock = GetMock<IAutomationPattern>();
        }


        [Test]
        public void GetCurrentPatternWhenSupported()
        {

            var result = ClassUnderTest.GetCurrentPattern(GetMock<IAutomationElement>().Object, PatternType.ValuePattern);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetCurrentPatternWhenNotSupported()
        {
            _automationPatternMock = null;

            GetMock<UIAutomationClient.IUIAutomationElement>()
                .Setup(m => m.GetCurrentPattern(It.Is<int>(v => v == (int) PatternType.ValuePattern)))
                .Returns(null);

            var result = ClassUnderTest.GetCurrentPattern(GetMock<IAutomationElement>().Object, PatternType.ValuePattern);

            Assert.IsNull(result);
        }

        [Test]
        public void GetSupportedPatterns()
        {
            var uiAutomationElementMock = GetMock<UIAutomationClient.IUIAutomationElement>();
            uiAutomationElementMock
                .Setup(m => m.GetCurrentPattern(It.IsAny<int>()))
                .Returns(() => null);
            uiAutomationElementMock
                .Setup(m => m.GetCurrentPattern(It.Is<int>(v => v == (int) PatternType.InvokePattern)))
                .Returns(() => GetMock<UIAutomationClient.IUIAutomationInvokePattern>());
            uiAutomationElementMock
                .Setup(m => m.GetCurrentPattern(It.Is<int>(v => v == (int) PatternType.TextPattern)))
                .Returns(() => GetMock<UIAutomationClient.IUIAutomationTextPattern>());
            uiAutomationElementMock
                .Setup(m => m.GetCurrentPattern(It.Is<int>(v => v == (int) PatternType.ValuePattern)))
                .Returns(() => GetMock<UIAutomationClient.IUIAutomationValuePattern>());

            var automationElementMock = GetMock<IAutomationElement>();
            automationElementMock
                .SetupGet(m => m.Element)
                .Returns(() => uiAutomationElementMock.Object);

            var result = ClassUnderTest.GetSupportedPatterns(GetMock<IAutomationElement>().Object);

            Assert.AreEqual(3, result.Count());
        }
    }
}

#endif