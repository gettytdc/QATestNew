using BluePrism.AutomateAppCore.Resources;
using BluePrism.DigitalWorker.EnvironmentFunctions;
using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using BluePrism.AutomateProcessCore;

namespace BluePrism.DigitalWorker.UnitTests.EnvironmentFunctions
{
    [TestFixture]
    public class GetResourceNameFunctionTests : EnvironmentFunctionsTestBase<GetResourceNameFunction>
    {
        private List<clsProcessValue> _processParameters;
        private DigitalWorkerContext _digitalWorkerContext => 
            new DigitalWorkerContext(new DigitalWorkerStartUpOptions { Name = TestDigitalWorkerName });

        protected override GetResourceNameFunction TestClassConstructor()
            => new GetResourceNameFunction(() => _digitalWorkerContext);

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _processParameters = new List<clsProcessValue>();
        }

        [Test]
        public void Evaluate_WithDigitalWorkerName_ShouldReturnDigitalWorkerName()
        {
            var evaluateResult = ClassUnderTest.Evaluate(_processParameters, null);

            evaluateResult.EncodedValue.ShouldBeEquivalentTo(TestDigitalWorkerName.FullName);
        }

        [Test]
        public void Evaluate_WithNoParameters_ShouldNotThrow()
        {
            Action action = () => ClassUnderTest.Evaluate(_processParameters, null);

            action.ShouldNotThrow();
        }

        [Test]
        public void Evaluate_WithParameters_ShouldThrow()
        {
            _processParameters.Add(new clsProcessValue());

            Action action = () => ClassUnderTest.Evaluate(_processParameters, null);

            action.ShouldThrow<clsFunctionException>();
        }
    }
}
