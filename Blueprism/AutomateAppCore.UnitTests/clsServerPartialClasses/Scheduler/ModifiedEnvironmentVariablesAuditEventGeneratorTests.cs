using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.clsServerPartialClasses.EnvironmentVariables;
using BluePrism.AutomateProcessCore;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.clsServerPartialClasses.Scheduler
{
    public class ModifiedEnvironmentVariablesAuditEventGeneratorTests
    {
        private clsEnvironmentVariable _oldVariable;
        private clsEnvironmentVariable _newVariable;
        private IUser _user;

        [SetUp]
        public void SetUp()
        {
            _oldVariable = CreateNewVariable();
            _newVariable = CreateNewVariable();
            var mockUser = new Mock<IUser>();
            _user = mockUser.Object;
        }

        [Test]
        public void Generate_ScheduleAndTasksNotModified_ShouldReturnNoEvents()
        {
            var generator = new ModifiedEnvironmentVariablesAuditEventGenerator(_oldVariable, _newVariable, _user);
            var auditEvent = generator.Generate(EnvironmentVariableEventCode.Modified, _user);
            Assert.That(auditEvent, Is.Null);
        }

        [Test]
        public void Generate_ScheduleNameChanged_ShouldReturnEventWithCorrectComment()
        {
            _newVariable.Name = "Some other name";
            var generator = new ModifiedEnvironmentVariablesAuditEventGenerator(_oldVariable, _newVariable, _user);
            var auditEvents = generator.Generate(EnvironmentVariableEventCode.Modified, _user);
            auditEvents.Comment.Should().StartWith("Name");
        }

        [Test]
        public void Generate_ScheduleDescriptionChanged_ShouldReturnEventWithCorrectComment()
        {
            _newVariable.Description = "Some other description";
            var generator = new ModifiedEnvironmentVariablesAuditEventGenerator(_oldVariable, _newVariable, _user);
            var auditEvents = generator.Generate(EnvironmentVariableEventCode.Modified, _user);
            auditEvents.Comment.Should().StartWith("Description");
        }

        [Test]
        public void Generate_ScheduleValueChanged_ShouldReturnEventWithCorrectComment()
        {
            _newVariable.Value = new clsProcessValue("Newer String");
            var generator = new ModifiedEnvironmentVariablesAuditEventGenerator(_oldVariable, _newVariable, _user);
            var auditEvents = generator.Generate(EnvironmentVariableEventCode.Modified, _user);
            auditEvents.Comment.Should().StartWith("Value");
        }

        [Test]
        public void Generate_ScheduleDataTypeChanged_ShouldReturnEventWithCorrectComment()
        {
            _newVariable.Value = new clsProcessValue(1);
            var generator = new ModifiedEnvironmentVariablesAuditEventGenerator(_oldVariable, _newVariable, _user);
            var auditEvents = generator.Generate(EnvironmentVariableEventCode.Modified, _user);
            auditEvents.Comment.Should().StartWith("Data Type");
        }

        private static clsEnvironmentVariable CreateNewVariable()
        {
            var Name = "New Name";
            var Value = new clsProcessValue("New String");
            var Description = "New Description";
            return new clsEnvironmentVariable(Name, Value, Description);
        }
    }
}
