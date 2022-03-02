using NUnit.Framework;
using FluentAssertions;
using BluePrism.AutomateAppCore.EnvironmentFunctions;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;
using Moq;
using System;
using System.Collections.Generic;
using BluePrism.AutomateProcessCore.Processes;
using BluePrism.Utilities.Testing;

namespace AutomateAppCore.UnitTests.EnvironmentFunctions
{
    [TestFixture]
    public class IsBPServerFunctionTests : UnitTestBase<IsBPServerFunction>
    {
        private clsSession _session;
        private clsProcess _process;
        private Mock<IServer> _serverMock;
        private Func<IServer> _serverFactory;
        private List<clsProcessValue> _processParameters;

        protected override IsBPServerFunction TestClassConstructor()
            => new IsBPServerFunction(_serverFactory);

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _processParameters = new List<clsProcessValue>();

            _session = new clsSession(Guid.NewGuid(), 0, new WebConnectionSettings(2, 5, 1, new List<UriWebConnectionSettings>()));
            _process = new clsProcess(new Mock<IGroupObjectDetails>().Object, DiagramType.Process, false) { Session = _session };

            _serverMock = GetMock<IServer>();
            _serverMock.Setup(s => s.IsServer()).Returns(true);
            _serverFactory = () => _serverMock.Object;
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
        public void Evaluate_WhenBPServerIsTrue_ReturnsTrue()
        {
            var isBPServerValue = ClassUnderTest.Evaluate(_processParameters, _process);

            isBPServerValue.Should().Be(new clsProcessValue(true));
        }

        [Test]
        public void Evaluate_WhenBPServerIsNotTrue_ReturnsFalse()
        {
            _serverMock.Setup(s => s.IsServer()).Returns(false);

            var isBPServerValue = ClassUnderTest.Evaluate(_processParameters, _process);

            isBPServerValue.Should().Be(new clsProcessValue(false));
        }
    }
}