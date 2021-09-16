namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using System.Collections.Generic;
    using Domain;
    using Domain.Filters;
    using FluentAssertions;
    using BpLibAdapters.Mappers.FilterMappers;
    using AutomateAppCore;
    using Func;
    using SessionStatus = Domain.SessionStatus;

    public static class SessionsHelper
    {
        public static SessionParameters GetTestDomainProcessSessionParameters() =>
            new SessionParameters
            {
                SessionNumber = new StringStartsWithFilter("1"),
                ItemsPerPage = 10,
                ProcessName = new StringStartsWithFilter("process"),
                ResourceName = new StringStartsWithFilter("resource"),
                User = new StringStartsWithFilter("bob"),
                Status = new MultiValueFilter<SessionStatus>(new Filter<SessionStatus>[] { new EqualsFilter<SessionStatus>(SessionStatus.Completed) }),
                StartTime = new GreaterThanOrEqualToFilter<DateTimeOffset>(DateTimeOffset.UtcNow),
                EndTime = new EqualsFilter<DateTimeOffset>(DateTimeOffset.UtcNow),
                LatestStage = new StringStartsWithFilter("latest"),
                StageStarted = new LessThanOrEqualToFilter<DateTimeOffset>(DateTimeOffset.UtcNow),
            };

        public static void ValidateParameterModelsAreEqual(Server.Domain.Models.ProcessSessionParameters bluePrismSessionParameters, SessionParameters domainSessionParameters)
        {
            bluePrismSessionParameters.ItemsPerPage.Should().Be(domainSessionParameters.ItemsPerPage);
            bluePrismSessionParameters.StartTime.Should().Be(domainSessionParameters.StartTime.ToBluePrismObject());
            bluePrismSessionParameters.EndTime.Should().Be(domainSessionParameters.EndTime.ToBluePrismObject());
            bluePrismSessionParameters.StageStarted.Should()
                .Be(domainSessionParameters.StageStarted.ToBluePrismObject());
            bluePrismSessionParameters.SessionNumber.Should()
                .Be(domainSessionParameters.SessionNumber.ToBluePrismObject());
            bluePrismSessionParameters.LatestStage.Should().Be(domainSessionParameters.LatestStage.ToBluePrismObject());
            bluePrismSessionParameters.ProcessName.Should().Be(domainSessionParameters.ProcessName.ToBluePrismObject());
            bluePrismSessionParameters.ResourceName.Should()
                .Be(domainSessionParameters.ResourceName.ToBluePrismObject());
            bluePrismSessionParameters.Status.ShouldBeEquivalentTo(domainSessionParameters.Status.ToBluePrismObject(), options => options.RespectingRuntimeTypes());
            bluePrismSessionParameters.User.Should().Be(domainSessionParameters.User.ToBluePrismObject());
        }

        public static void ValidateModelsAreEqual(clsProcessSession bluePrism, Session domain)
        {
            domain.SessionId.Should().Be(bluePrism.SessionID);
            domain.SessionNumber.Should().Be(bluePrism.SessionNum);
            ((Some<DateTimeOffset>)domain.StartTime).Value.Should().Be(bluePrism.SessionStart);
            ((Some<DateTimeOffset>)domain.EndTime).Value.Should().Be(bluePrism.SessionEnd);
            ((Some<DateTimeOffset>)domain.StageStarted).Value.Should().Be(bluePrism.LastUpdated);
            domain.ProcessId.Should().Be(bluePrism.ProcessID);
            domain.ProcessName.Should().Be(bluePrism.ProcessName);
            domain.ResourceId.Should().Be(bluePrism.ResourceID);
            domain.ResourceName.Should().Be(bluePrism.ResourceName);
            StatusesAreEqual(bluePrism.Status, domain.Status).Should().BeTrue();
            domain.UserName.Should().Be(bluePrism.UserName);
            domain.ExceptionMessage.Should().Be(bluePrism.ExceptionMessage);
            ((Some<string>)domain.ExceptionType).Value.Should().Be(bluePrism.ExceptionType);
            TerminationReasonAreEqual(bluePrism.SessionTerminationReason, domain.TerminationReason).Should().BeTrue();
        }

        public static bool StatusesAreEqual(Server.Domain.Models.SessionStatus bluePrismState, SessionStatus domainState) =>
            (bluePrismState == Server.Domain.Models.SessionStatus.Terminated && domainState == SessionStatus.Terminated)
            || (bluePrismState == Server.Domain.Models.SessionStatus.Failed && domainState == SessionStatus.Failed)
            || (bluePrismState == Server.Domain.Models.SessionStatus.Running && domainState == SessionStatus.Running)
            || (bluePrismState == Server.Domain.Models.SessionStatus.Archived && domainState == SessionStatus.Archived)
            || (bluePrismState == Server.Domain.Models.SessionStatus.Completed && domainState == SessionStatus.Completed)
            || (bluePrismState == Server.Domain.Models.SessionStatus.Debugging && domainState == SessionStatus.Debugging)
            || (bluePrismState == Server.Domain.Models.SessionStatus.Pending && domainState == SessionStatus.Pending)
            || (bluePrismState == Server.Domain.Models.SessionStatus.Stalled && domainState == SessionStatus.Warning)
            || (bluePrismState == Server.Domain.Models.SessionStatus.Stopped && domainState == SessionStatus.Stopped)
            || (bluePrismState == Server.Domain.Models.SessionStatus.StopRequested && domainState == SessionStatus.Stopping);

        public static bool TerminationReasonAreEqual(AutomateAppCore.SessionTerminationReason bluePrismState, Domain.SessionTerminationReason domainState) =>
            (bluePrismState == AutomateAppCore.SessionTerminationReason.InternalError && domainState == Domain.SessionTerminationReason.InternalError)
            || (bluePrismState == AutomateAppCore.SessionTerminationReason.None && domainState == Domain.SessionTerminationReason.None)
            || (bluePrismState == AutomateAppCore.SessionTerminationReason.ProcessError && domainState == Domain.SessionTerminationReason.ProcessError);

        public static IEnumerable<clsProcessSession> GetTestBluePrismClsProcessSession(int count = 1)
        {
            var sessionsList = new List<clsProcessSession>(count);
            for (var i = 0; i < count; i++)
            {
                sessionsList.Add(new clsProcessSession
                {
                    SessionID = Guid.NewGuid(),
                    SessionNum = 1,
                    SessionStart = new DateTimeOffset(DateTime.UtcNow),
                    SessionEnd = new DateTimeOffset(DateTime.UtcNow),
                    LastUpdated = new DateTimeOffset(DateTime.UtcNow),
                    LastStage = "someStage",
                    ProcessID = Guid.NewGuid(),
                    ProcessName = "some name",
                    ResourceID = Guid.NewGuid(),
                    ResourceName = "some resource name",
                    Status = Server.Domain.Models.SessionStatus.Running,
                    UserName = "user name",
                    ExceptionMessage = "message",
                    ExceptionType = "type",
                    SessionTerminationReason = AutomateAppCore.SessionTerminationReason.InternalError
                });
            }

            return sessionsList;
        }
    }
}
