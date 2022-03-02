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
    public class GetUserNameFunctionTests : UnitTestBase<GetUserNameFunction>
    {
        private clsSession _session;
        private clsProcess _process;
        private Mock<IServer> _serverMock;
        private Func<IServer> _serverFactory;
        private List<clsProcessValue> _processParameters;
        private readonly string _sessionStartedByuser = "TestUser";

        protected override GetUserNameFunction TestClassConstructor()
            => new GetUserNameFunction(_serverFactory);

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _processParameters = new List<clsProcessValue>();

            _session = new clsSession(Guid.NewGuid(), 0, new WebConnectionSettings(2, 5, 1, new List<UriWebConnectionSettings>()));
            _process = new clsProcess(new Mock<IGroupObjectDetails>().Object, DiagramType.Process, false) { Session = _session };

            _serverMock = GetMock<IServer>();
            _serverFactory = () => _serverMock.Object;

            _serverMock.Setup(s => s.GetSessionDetails(_session.ID)).Returns(new clsServer.SessionData(0, _sessionStartedByuser, string.Empty, string.Empty));
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
        public void Evaluate_WithUserName_ShouldReturnUserName()
        {            
            var usernameValue = ClassUnderTest.Evaluate(_processParameters, _process);

            usernameValue.Should().Be(new clsProcessValue(_sessionStartedByuser));
        }

        [Test]
        public void Evaluate_SessionIsNull_ShouldReturnEmptyString()
        {
            _process.Session = null;

            var usernameValue = ClassUnderTest.Evaluate(_processParameters, _process);

            usernameValue.Should().Be(new clsProcessValue(string.Empty));
        }
    }
}