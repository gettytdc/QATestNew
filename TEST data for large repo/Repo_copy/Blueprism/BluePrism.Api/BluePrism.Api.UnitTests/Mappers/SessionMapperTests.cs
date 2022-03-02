namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using Api.Mappers;
    using CommonTestClasses.Extensions;
    using Domain;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;
    using Func;

    using SessionStatus = Domain.SessionStatus;
    using SessionTerminationReason = Domain.SessionTerminationReason;

    [TestFixture]
    public class SessionMapperTests
    {
        [Test]
        public void ToModelObject_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var processSession = new Session
            {
                SessionNumber = 1,
                ProcessId = Guid.NewGuid(),
                ProcessName = "processName",
                UserName = "userName",
                ResourceId = Guid.NewGuid(),
                ResourceName = "resourceName",
                Status = SessionStatus.Running,
                StartTime = OptionHelper.Some(DateTimeOffset.Now),
                EndTime = OptionHelper.Some(DateTimeOffset.Now),
                StageStarted = OptionHelper.Some(DateTimeOffset.Now),
                LatestStage = "latestStage"
            };

            processSession.ToModelObject().ShouldBeEquivalentTo(new SessionModel
            {
                SessionNumber = processSession.SessionNumber,
                ProcessId = processSession.ProcessId,
                ProcessName = processSession.ProcessName,
                UserName = processSession.UserName,
                ResourceId = processSession.ResourceId,
                ResourceName = processSession.ResourceName,
                Status = Models.SessionStatus.Running,
                StartTime = processSession.StartTime is Some<DateTimeOffset> startTime ? startTime.Value : (DateTimeOffset?)null,
                EndTime = processSession.EndTime is Some<DateTimeOffset> endTime ? endTime.Value : (DateTimeOffset?)null,
                StageStarted = processSession.StageStarted is Some<DateTimeOffset> stageStarted ? stageStarted.Value : (DateTimeOffset?)null,
                LatestStage = processSession.LatestStage
            });
        }

        [TestCaseSource(nameof(MappedSessionTerminationReasons))]
        public void ToModelObject_ShouldReturnCorrectSessionTerminationReasons_WhenCalled((SessionTerminationReason, Models.SessionTerminationReason) statusMappings)
        {
            var (domainSessionTerminationReason, modelSessionTerminationReason) = statusMappings;

            new Session { TerminationReason = domainSessionTerminationReason }
                .ToModelObject()
                .TerminationReason
                .Should()
                .Be(modelSessionTerminationReason);
        }

        [Test]
        public void ToModelObject_ShouldThrowArgumentException_WhenInvalidTerminationReasonStatusSupplied()
        {
            Action action = () => new Session { TerminationReason = (SessionTerminationReason)99 }.ToModelObject();
            action.ShouldThrow<ArgumentException>();
        }

        [TestCaseSource(nameof(MappedStatuses))]
        public void ToModelObject_ShouldReturnCorrectSessionStatus_WhenCalled(SessionStatus domainSessionStatus, Models.SessionStatus modelSessionStatus)
        {
            new Session { Status = domainSessionStatus }
                .ToModelObject()
                .Status
                .Should()
                .Be(modelSessionStatus);
        }

        [Test]
        public void ToModelObject_ShouldThrowArgumentException_WhenInvalidSessionStatusSupplied()
        {
            Action action = () => new Session { Status = (SessionStatus)99 }.ToModelObject();
            action.ShouldThrow<ArgumentException>();
        }

        private static (SessionTerminationReason domainSessionTerminationReason, Models.SessionTerminationReason
            modelSessionTerminationReason)[] MappedSessionTerminationReasons() =>
            new[]
            {
                (SessionTerminationReason.None, Models.SessionTerminationReason.None),
                (SessionTerminationReason.InternalError, Models.SessionTerminationReason.InternalError),
                (SessionTerminationReason.ProcessError, Models.SessionTerminationReason.ProcessError)
            };

        private static IEnumerable<TestCaseData> MappedStatuses() =>
            new[]
            {
                (SessionStatus.Completed, Models.SessionStatus.Completed),
                (SessionStatus.Failed, Models.SessionStatus.Terminated),
                (SessionStatus.Pending, Models.SessionStatus.Pending),
                (SessionStatus.Running, Models.SessionStatus.Running),
                (SessionStatus.Warning, Models.SessionStatus.Warning),
                (SessionStatus.Stopping, Models.SessionStatus.Stopping),
                (SessionStatus.Stopped, Models.SessionStatus.Stopped),
                (SessionStatus.Terminated, Models.SessionStatus.Terminated)
            }
            .ToTestCaseData();
    }
}
