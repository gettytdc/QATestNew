using System;
using System.Reflection;
using BluePrism.BPCoreLib.DependencyInjection;
using BluePrism.Utilities.Testing;
using Moq;
using NUnit.Framework;
using BluePrism.ApplicationManager.WindowSpy;
using System.Drawing;

namespace BluePrism.UIAutomation.UnitTests
{

    [TestFixture]
    public class WindowSpyTests : UnitTestBase<clsWindowSpy>
    {
        public override void Setup()
        {
            base.Setup();
            DependencyResolver.SetContainer(Container);
        }

        [Test]
        public void HandleUIAMouseMovementsWorksWithElement()
        {
            var classUnderTest = SetupClassUnderTest();

            var passedPoint = new Point(); // can't use default as build doesn't support c# 7.1

            var automationFacadeMock = new Mock<IAutomationElementFacade>();
            automationFacadeMock.SetupGet(m => m.ControlType).Returns(ControlType.Pane);

            var automationElementMock = new Mock<IAutomationElement>();
            automationElementMock.SetupGet(m => m.CurrentNativeWindowHandle).Returns(IntPtr.Zero);
            automationElementMock.SetupGet(m => m.CurrentBoundingRectangle).Returns(Rectangle.Empty);
            automationElementMock.SetupGet(m => m.CurrentProcessId).Returns(0);
            automationElementMock.SetupGet(m => m.Current).Returns(automationFacadeMock.Object);

            var automationFactoryMock = new Mock<IAutomationFactory>();

            automationFactoryMock.
                Setup((m) => m.FromPoint(It.IsAny<Point>())).
                Callback((Point p) => passedPoint = p).
                Returns(automationElementMock.Object);

            SetupUiaMock(classUnderTest, automationFactoryMock.Object);

            var handleUIAMouseMovementsMethod = typeof(clsWindowSpy).GetMethod("HandleUIAMouseMovements", BindingFlags.Instance | BindingFlags.NonPublic);

            var point = new Point(12, 34);
            var parameters = new object[1] { point };
            handleUIAMouseMovementsMethod.Invoke(classUnderTest, parameters);

            Assert.AreEqual(point, passedPoint);
        }

        [Test]
        public void HandleUIAMouseMovementsWorksWithoutElement()
        {
            var classUnderTest = SetupClassUnderTest();

            var passedPoint = new Point(); // can't use default as build doesn't support c# 7.1

            var automationFactoryMock = new Mock<IAutomationFactory>();
            automationFactoryMock.
                Setup((m) => m.FromPoint(It.IsAny<Point>())).
                Callback((Point p) => passedPoint = p).
            Returns((Point p) => null);
    
        SetupUiaMock(classUnderTest, automationFactoryMock.Object);

            var handleUIAMouseMovementsMethod = typeof(clsWindowSpy).GetMethod("HandleUIAMouseMovements", BindingFlags.Instance | BindingFlags.NonPublic);

            var point = new Point(12, 34);
            var parameters = new object[1] { point };
            handleUIAMouseMovementsMethod.Invoke(classUnderTest, parameters);

            Assert.AreEqual(point, passedPoint);
        }

        [Test]
        public void HandleUIAMouseMovementsSwallowsUnauthorizedAccessException()
        {
            var classUnderTest = SetupClassUnderTest();

            var automationFactoryMock = new Mock<IAutomationFactory>();
            automationFactoryMock.Setup(m => m.FromPoint(It.IsAny<Point>())).Throws<UnauthorizedAccessException>();

            SetupUiaMock(classUnderTest, automationFactoryMock.Object);

            var handleUIAMouseMovementsMethod = typeof(clsWindowSpy).GetMethod("HandleUIAMouseMovements", BindingFlags.Instance | BindingFlags.NonPublic);

            var parameters = new object[1] { new Point(12, 34) };
            void test() => handleUIAMouseMovementsMethod.Invoke(classUnderTest, parameters);
            Assert.DoesNotThrow(test);
        }

        [Test]
        public void HandleUIAMouseMovementsThrowsOtherExceptions()
        {
            var classUnderTest = SetupClassUnderTest();

            var automationFactoryMock = new Mock<IAutomationFactory>();
            automationFactoryMock.Setup(m => m.FromPoint(It.IsAny<Point>())).Throws<ApplicationException>();

            SetupUiaMock(classUnderTest, automationFactoryMock.Object);

            var handleUIAMouseMovementsMethod = typeof(clsWindowSpy).GetMethod("HandleUIAMouseMovements", BindingFlags.Instance | BindingFlags.NonPublic);

            var parameters = new object[1] { new Point(12, 34) };
            void test() => handleUIAMouseMovementsMethod.Invoke(classUnderTest, parameters);
            Assert.Throws<TargetInvocationException>(test);
        }

        private clsWindowSpy SetupClassUnderTest()
        {
            var result = new clsWindowSpy(clsWindowSpy.SpyMode.UIAutomation, clsWindowSpy.SpyMode.UIAutomation, false);
            var type = typeof(clsWindowSpy);
            type.GetField("mHighlighter", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(result, new clsWindowHighlighter());
            return result;
        }

        private void SetupUiaMock(clsWindowSpy windowSpy, IAutomationFactory mock)
        {
            var type = typeof(clsWindowSpy);
            type.GetField("mAutomation", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(windowSpy, mock);
        }
    }
}
