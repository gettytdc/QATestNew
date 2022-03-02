using NUnit.Framework;
using FluentAssertions;
using BluePrism.AutomateAppCore.EnvironmentFunctions;
using BluePrism.AutomateProcessCore;
using Moq;
using System;
using System.Collections.Generic;
using BluePrism.AutomateProcessCore.Processes;
using BluePrism.Utilities.Testing;

namespace AutomateAppCore.UnitTests.EnvironmentFunctions
{
    [TestFixture]
    public class GetBPVersionMinorFunctionTests : UnitTestBase<GetBPVersionMinorFunction>
    {
        private Guid _sessionId;
        private clsSession _session;
        private clsProcess _process;
        private List<clsProcessValue> _processParameters;

        protected int MinorAssemblyVersion => GetType().Assembly.GetName().Version.Minor;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _processParameters = new List<clsProcessValue>();

            _sessionId = Guid.NewGuid();
            _session = new clsSession(_sessionId, 0, new WebConnectionSettings(2, 5, 1, new List<UriWebConnectionSettings>()));
            _process = new clsProcess(new Mock<IGroupObjectDetails>().Object, DiagramType.Process, false) { Session = _session };
        }

        [Test]
        public void Evaluate_WithNoParameters_ShouldNotThrow()
        {            
            Action action = () => ClassUnderTest.Evaluate(_processParameters, _process);

            action.ShouldNotThrow();
        }

        [Test]
        public void Evaluate_WithParameters_ShouldThrow()
        {
            _processParameters.Add(new clsProcessValue());

            Action action = () => ClassUnderTest.Evaluate(_processParameters, _process);

            action.ShouldThrow<clsFunctionException>();
        }

        [Test]
        public void Evaluate_WithSession_ShouldReturnAssemblyMajorVersion()
        {            
            var bpVersionMinorValue = ClassUnderTest.Evaluate(_processParameters, _process);

            bpVersionMinorValue.Should().Be(new clsProcessValue(MinorAssemblyVersion));
        }
    }
}