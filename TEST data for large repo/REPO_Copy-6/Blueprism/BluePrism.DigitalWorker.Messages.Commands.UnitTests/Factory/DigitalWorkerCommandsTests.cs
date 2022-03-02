using BluePrism.AutomateProcessCore;
using BluePrism.DigitalWorker.Messages.Commands.Factory;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace BluePrism.DigitalWorker.Messages.Commands.UnitTests.Factory
{
    [TestFixture]
    public class DigitalWorkerCommandsTests
    {
        private const string VariableName = "BoolVariable";
        private const string Username = "Username";
        private readonly Guid _sessionId = Guid.NewGuid();
        private readonly Guid _processId = Guid.NewGuid();
        private readonly clsProcessValue _boolProcessValue = new clsProcessValue(true);
        private SessionVariable SessionVariable => GetSessionVariableData(VariableName, _boolProcessValue);

        [Test]
        public void RunProcess_ShouldInitialiseMessage()
        {
            var runProcess = DigitalWorkerCommands.RunProcess(_sessionId, _processId);

            runProcess.SessionId.Should().Be(_sessionId);
            runProcess.ProcessId.Should().Be(_processId);
        }

        [Test]
        public void StopProcess_ShouldInitialiseMessage()
        {
            var stopProcess = DigitalWorkerCommands.StopProcess(_sessionId, Username);

            stopProcess.SessionId.Should().Be(_sessionId);
            stopProcess.Username.Should().Be(Username);
        }

        [Test]
        public void RequestStopProcess_ShouldInitialiseMessage()
        {
            var requestStopProcess = DigitalWorkerCommands.RequestStopProcess(_sessionId);

            requestStopProcess.SessionId.Should().Be(_sessionId);
        }

        [Test]
        public void GetSessionVariables_ShouldInitialiseMessage()
        {
            var getSessionVariables = DigitalWorkerCommands.GetSessionVariables(_sessionId);

            getSessionVariables.SessionId.Should().Be(_sessionId);
        }

        [Test]
        public void GetSessionVariablesResponse_ShouldInitialiseMessage()
        {
            var sessionVariables = new[] { SessionVariable };

            var sessionVariablesResponse = DigitalWorkerCommands.GetSessionVariablesResponse(true, sessionVariables);
            
            sessionVariablesResponse.Variables.ShouldBeEquivalentTo(sessionVariables);
        }

        private static SessionVariable GetSessionVariableData(string variableName, clsProcessValue boolProcessValue) =>
            new SessionVariable(variableName, boolProcessValue.Description, new ProcessValue(boolProcessValue.FormattedValue, TryParseDataType(boolProcessValue.DataTypeName)));

        private static ProcessValueType TryParseDataType(string type)
        {
            Enum.TryParse<ProcessValueType>(type, true, out var dataType);

            return dataType;
        }
    }
}
