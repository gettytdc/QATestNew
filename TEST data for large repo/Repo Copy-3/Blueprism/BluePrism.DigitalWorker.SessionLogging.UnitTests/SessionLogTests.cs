using System;
using BluePrism.DigitalWorker.SessionLogging.LogData;
using BluePrism.UnitTesting;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SessionLogData = BluePrism.DigitalWorker.Messages.Events.ProcessStages.LogData.SessionLogData;

namespace BluePrism.DigitalWorker.SessionLogging.UnitTests
{
    [TestFixture]
    public class SessionLogTests : UnitTestBase<SessionLog>
    {
        [Test]
        public void CreateMessage_HasCorrectData()
        {
            var sessionId = Guid.NewGuid();
            var entryNumber = 5;
            var at = DateTimeOffset.Now;
            var stageId = Guid.NewGuid();
            var stageName = "stage";
            var data = new ActionStageStartedData("obj", "act");

            var log = new SessionLog(sessionId, entryNumber, at, stageId, stageName, data);

            var message = log.CreateMessage();

            message.SessionId.Should().Be(sessionId);
            message.EntryNumber.Should().Be(entryNumber);
            message.At.Should().Be(at);
            message.StageId.Should().Be(stageId);
            message.StageName.Should().Be(stageName);
            message.Data.Should().Be(JsonConvert.SerializeObject(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
        }

        [Test]
        public void Deserialize_PopulatesFieldsCorrectly()
        {
            var sessionId = Guid.NewGuid();
            var entryNumber = 5;
            var at = DateTimeOffset.Now;
            var stageId = Guid.NewGuid();
            var stageName = "stage";
            var data = new ActionStageStartedData("obj", "act");
            var serializedData = JsonConvert.SerializeObject(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            var log = SessionLog.Deserialize(sessionId, entryNumber, at, stageId, stageName, serializedData);

            var message = log.CreateMessage();

            log.SessionId.Should().Be(sessionId);
            log.EntryNumber.Should().Be(entryNumber);
            log.At.Should().Be(at);
            log.StageId.Should().Be(stageId);
            log.StageName.Should().Be(stageName);
            log.Data.Should().ShouldBeEquivalentTo(data);
        }

        [TestCaseSource(nameof(SessionLogDataCases))]
        public void RoundTripTests(SessionLogData data)
        {
            var log = new SessionLog(Guid.NewGuid(), 5, DateTimeOffset.Now, Guid.NewGuid(), "stage", data);

            var message = log.CreateMessage();
            var roundTripped = SessionLog.Deserialize(message.SessionId, message.EntryNumber, message.At, message.StageId, message.StageName, message.Data);

            roundTripped.ShouldBeEquivalentTo(log);
        }

        static SessionLogData[] SessionLogDataCases =
        {
            new ActionStageStartedData("obj", "act"),
            new ActionStageEndedData("obj", "act"),
            new AlertStageEndedData("alert"),
            new CalculationStageEndedData("result", 3),
            new ChoiceStageEndedData("choice", 2),
            new CodeStageEndedData(),
            new DebugDataEmittedData("alert"),
            new DecisionStageEndedData("alert"),
            new LoopEndStageEndedData("alert"),
            new LoopStartStageEndedData("alert"),
            new MultipleCalculationStageEndedData(),
            new NavigateStageEndedData(),
            new NoteStageStartedData("alert"),
            new ObjectStartedData(),
            new ObjectEndedData("obj", "act"),
            new ProcessEndedData(),
            new ProcessStartedData(),
            new ProcessReferenceEndedData(),
            new ProcessReferenceStartedData(),
            new ProcessStageErroredData("alert"),
            new ReadStageEndedData(),
            new RecoverStageEndedData(),
            new ResumeStageEndedData(),
            new SkillStageEndedData(),
            new SkillStageStartedData("obj", "act"),
            new SubsheetEndedData(),
            new SubsheetStartedData(),
            new SubsheetReferenceStageEndedData(),
            new SubsheetReferenceStageStartedData(),
            new WaitStageEndedData("choice", 2),
            new WriteStageEndedData()
        };
    }
}
