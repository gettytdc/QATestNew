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
    public class IsStopRequestedFunctionTests : UnitTestBase<IsStopRequestedFunction>
    {
        private clsSession _session;
        private clsProcess _process;
        private Mock<IServer> _serverMock;
        private Func<IServer> _serverFactory; 

        protected override IsStopRequestedFunction TestClassConstructor()
            => new IsStopRequestedFunction(_serverFactory);

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _session = new clsSession(Guid.NewGuid(), 0, new WebConnectionSettings(2, 5, 1, new List<UriWebConnectionSettings>()));
            _process = new clsProcess(new Mock<IGroupObjectDetails>().Object, DiagramType.Process, false) { Session = _session };

            _serverMock = GetMock<IServer>();
            _serverFactory = () => _serverMock.Object;
        }

        [Test]
        public void Evaluate_WhenNoStopRequested_ShouldReturnFalse()
        {
            _serverMock.Setup(s => s.IsStopRequested(It.IsAny<int>())).Returns(false);
            
            var isStopRequestedValue = ClassUnderTest.Evaluate(null, _process);

            isStopRequestedValue.Should().Be(new clsProcessValue(false));
        }

        [Test]
        public void Evaluate_WhenStopRequested_ShouldReturnTrue()
        {
            _serverMock.Setup(s => s.IsStopRequested(It.IsAny<int>())).Returns(true);

            var isStopRequestedValue = ClassUnderTest.Evaluate(null, _process);

            isStopRequestedValue.Should().Be(new clsProcessValue(true));
        }

        [Test]
        public void Evaluate_SessionIsNull_ShouldReturnFalse()
        {
            _serverMock.Setup(s => s.IsStopRequested(It.IsAny<int>())).Returns(false);

            _process.Session = null;

            var isStopRequestedValue = ClassUnderTest.Evaluate(null, _process);

            isStopRequestedValue.Should().Be(new clsProcessValue(false));
        }

        [Test]
        public void Evaluate_SessionIdentifierIsDigitalWorker_ShouldReturnFalse()
        {
            _session = new clsSession(new DigitalWorkerSessionIdentifier(Guid.NewGuid()), new WebConnectionSettings(2, 5, 1, new List<UriWebConnectionSettings>()));

            var isStopRequestedValue = ClassUnderTest.Evaluate(null, _process);

            isStopRequestedValue.Should().Be(new clsProcessValue(false));
        }
    }
}