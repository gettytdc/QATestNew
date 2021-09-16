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
    public class GetStartTimeFunctionTests : UnitTestBase<GetStartTimeFunction>
    {
        private clsSession _session;
        private clsProcess _process;
        private Mock<IServer> _serverMock;
        private Func<IServer> _serverFactory;
        private List<clsProcessValue> _processParameters;
        private readonly DateTime _sessionStarted = DateTime.Now.ToUniversalTime();

        protected override GetStartTimeFunction TestClassConstructor()
            => new GetStartTimeFunction(_serverFactory);

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _processParameters = new List<clsProcessValue>();

            _session = new clsSession(Guid.NewGuid(), 0, new WebConnectionSettings(2, 5, 1, new List<UriWebConnectionSettings>()));
            _process = new clsProcess(new Mock<IGroupObjectDetails>().Object, DiagramType.Process, false) { Session = _session };

            _serverMock = GetMock<IServer>();
            _serverFactory = () => _serverMock.Object;

            _serverMock.Setup(s => s.GetActualSessions(_session.ID)).Returns(new List<clsProcessSession> { new clsProcessSession() { SessionStart = _sessionStarted } });
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
        public void Evaluate_StartedSession_ShouldReturnStartTime()
        {            
            var startTimeValue = ClassUnderTest.Evaluate(_processParameters, _process);

            startTimeValue.Should().Be(new clsProcessValue(DataType.datetime, _sessionStarted, false));
        }

        [Test]
        public void Evaluate_SessionIsNull_ShouldThrow()
        {
            _process.Session = null;

            Action action = () => ClassUnderTest.Evaluate(_processParameters, _process);

            action.ShouldThrow<clsFunctionException>();
        }

        [Test]
        public void Evaluate_WithMultipleSessions_ShouldThrow()
        {
            _serverMock.Setup(s => s.GetActualSessions(_session.ID)).Returns(
                new List<clsProcessSession> {
                    new clsProcessSession() { SessionStart = _sessionStarted },
                    new clsProcessSession() { SessionStart = _sessionStarted }
                });

            Action action = () => ClassUnderTest.Evaluate(_processParameters, _process);

            action.ShouldThrow<clsFunctionException>();
        }
    }
}