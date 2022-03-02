namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Apps72.Dev.Data.DbMocker;
    using Autofac;
    using Domain;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Services;

    [TestFixture]
    public class SessionLogBluePrismIntegrationTest : BluePrismIntegrationTestBase<SessionLogsService>
    {
        public override void Setup() =>
            base.Setup(builder =>
                builder.RegisterInstance(new SessionLogConfiguration { MaxResultStringLength = 100 }));

        [Test]
        public async Task GetSessionLog_OnSuccess_ReturnsExpectedValue()
        {
            var sessionId = Guid.NewGuid();

            SetupMockData(sessionId);
            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetSessionLogs(sessionId, new SessionLogsParameters());

            result.Should().BeAssignableTo<Success<ItemsPage<SessionLogItem>>>();

            var resultValue = ((Success<ItemsPage<SessionLogItem>>)result).Value.Items.ToList();

            resultValue.Count.Should().Be(1);
            resultValue[0].Result.Should().Be(TestLogItem.Result);
        }

        [Test]
        public async Task GetSessionLog_OnSessionNotFound_ReturnsSessionNotFoundError()
        {
            SetupMockData(Guid.NewGuid());
            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetSessionLogs(Guid.NewGuid(), new SessionLogsParameters());

            result.Should().BeAssignableTo<Failure<SessionNotFoundError>>();
        }

        [Test]
        public async Task GetSessionLogParameters_OnSuccess_ReturnsExpectedValue()
        {
            var sessionId = Guid.NewGuid();
            var logId = 1234;

            SetupMockData(sessionId, logId);
            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetLogParameters(sessionId, logId);

            var expectedValue = new SessionLogItemParameters
            {
                Inputs = new Dictionary<string, DataValue>
                {
                    ["test"] = new DataValue { ValueType = DataValueType.Text, Value = "abc" },
                },
                Outputs = new Dictionary<string, DataValue>()
            };

            result.Should().BeAssignableTo<Success>();
            ((Success<SessionLogItemParameters>)result).Value.ShouldBeEquivalentTo(expectedValue);
        }

        [Test]
        public async Task GetSessionLogParameters_OnSessionNotFound_ReturnsSessionNotFoundError()
        {
            SetupMockData(Guid.NewGuid());
            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetLogParameters(Guid.NewGuid(), 1);

            result.Should().BeAssignableTo<Failure<SessionNotFoundError>>();
        }

        [Test]
        public async Task GetSessionLogParameters_OnLogNotFound_ReturnsLogNotFoundError()
        {
            var sessionId = Guid.NewGuid();
            SetupMockData(sessionId, 1);
            ConfigureFallbackForUpdateAndInsert();

            var result = await Subject.GetLogParameters(sessionId, 2);

            result.Should().BeAssignableTo<Failure<LogNotFoundError>>();
        }

        private static readonly SessionLogItem TestLogItem = new SessionLogItem
        {
            LogId = 1234,
            StageName = "TestObject",
            StageType = StageTypes.Block,
            Result = "1234",
            ResourceStartTime = OptionHelper.Some(DateTimeOffset.UtcNow.AddHours(4)),
            HasParameters = true
        };

        private void SetupMockData(Guid sessionId, long? logId = null)
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("from BPASessionLog_Unicode"))
                .ReturnsTable(MockTable
                    .WithColumns("logid", "startdatetime", "starttimezoneoffset", "stagetype", "objectname", "actionname", "processname", "pagename", "stagename", "result", "resulttype", "attributexml", "LogNumber")
                    .AddRow(
                        1234,
                        ((Some<DateTimeOffset>)TestLogItem.ResourceStartTime).Value.DateTime,
                        0,
                        (int)TestLogItem.StageType,
                        TestLogItem.StageName,
                        "TestAction",
                        "TestProcess",
                        "TestPage",
                        "TestStage",
                        TestLogItem.Result,
                        (int)AutomateProcessCore.DataType.number,
                        @"<parameters><inputs><input name=""test"" type=""text"" value=""abc"" /></inputs><outputs /></parameters>",
                        4321));

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("from BPASessionLog_NonUnicode"))
                .ReturnsTable(MockTable.Empty());

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select s.sessionnumber from BPASession")
                    && (Guid)cmd.Parameters.ByName("@id1").Value == sessionId)
                .ReturnsTable(MockTable
                    .WithColumns("sessionnumber")
                    .AddRow(1234));

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Contains("select s.sessionnumber from BPASession")
                    && (Guid)cmd.Parameters.ByName("@id1").Value != sessionId)
                .ReturnsTable(MockTable.Empty());

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Equals("select name from sysobjects where id = object_id(@name)"))
                .ReturnsScalar(cmd => cmd.Parameters.ByName("@name").Value.ToString());

            if (logId.HasValue)
            {
                DatabaseConnection.Mocks
                    .When(cmd =>
                        cmd.HasValidSqlServerCommandText()
                        && cmd.CommandText.Contains("[attributexml]")
                        && (long)cmd.Parameters.ByName("@logid").Value == logId.Value)
                    .ReturnsScalar(@"<parameters><inputs><input name=""test"" type=""text"" value=""abc"" /></inputs><outputs /></parameters>");

                DatabaseConnection.Mocks
                    .When(cmd =>
                        cmd.HasValidSqlServerCommandText()
                        && cmd.CommandText.Contains("[attributexml]")
                        && (long)cmd.Parameters.ByName("@logid").Value != logId.Value)
                    .ReturnsTable(MockTable.Empty());
            }
        }
    }
}
