using BluePrism.DigitalWorker.EnvironmentFunctions;
using NUnit.Framework;
using FluentAssertions;
using BluePrism.DigitalWorker.Sessions;
using Moq;
using BluePrism.AutomateProcessCore;
using System;
using System.Collections.Generic;

namespace BluePrism.DigitalWorker.UnitTests.EnvironmentFunctions
{
    [TestFixture]
    public class GetStartTimeFunctionTests : EnvironmentFunctionsTestBase<GetStartTimeFunction>
    {
        private List<clsProcessValue> _processParameters;

        protected override GetStartTimeFunction TestClassConstructor()
            => new GetStartTimeFunction(TestSessionRegistry);

        public override void Setup()
        {
            base.Setup();

            _processParameters = new List<clsProcessValue>();
        }
        
        [Test]
        public void Evaluate_WithRegisteredSession_ShouldReturnStartTime()
        {
            var evaluateResult = ClassUnderTest.Evaluate(_processParameters, Process);

            evaluateResult.ShouldBeEquivalentTo(new clsProcessValue(DataType.datetime, Now.DateTime, false));
        }
        
        [Test]
        public void Evaluate_WithNoRegisteredSession_ShouldThrow()
        {
            TestSessionRegistry.Unregister(SessionId);

            Action action = () => ClassUnderTest.Evaluate(_processParameters, Process);

            action.ShouldThrow<clsFunctionException>();
        }

        [Test]
        public void Evaluate_WithNoParameters_ShouldNotThrow()
        {
            Action action = () => ClassUnderTest.Evaluate(_processParameters, Process);

            action.ShouldNotThrow();
        }

        [Test]
        public void Evaluate_WithParameters_ShouldThrow()
        {
            _processParameters.Add(new clsProcessValue());

            Action action = () => ClassUnderTest.Evaluate(_processParameters, Process);

            action.ShouldThrow<clsFunctionException>();
        }

        [Test]
        public void Evaluate_WithNoProcessSession_ShouldThrow()
        {
            Action action = () => ClassUnderTest.Evaluate(_processParameters, new clsProcess(GetMock<IGroupObjectDetails>().Object, AutomateProcessCore.Processes.DiagramType.Process, false));

            action.ShouldThrow<clsFunctionException>();
        }
    }
}