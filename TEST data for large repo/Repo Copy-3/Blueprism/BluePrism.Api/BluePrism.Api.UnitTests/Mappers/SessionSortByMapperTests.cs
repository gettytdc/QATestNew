namespace BluePrism.Api.UnitTests.Mappers
{
    using System.Collections.Generic;
    using Api.Mappers;
    using CommonTestClasses.Extensions;
    using Domain;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;

    public class SessionSortByMapperTests
    {
        [Test]
        public void GetSessionsSortByModelName_ShouldReturnDefaultSortOrder_WhenCalledWithNullSortBy()
        {
            SessionSortBy? nullSortOrderBy = null;
            var sortOrderResult = SessionSortByMapper.GetProcessSessionSortByModelName(nullSortOrderBy);

            var result = ((Some<SessionSortByProperty>)sortOrderResult).Value;

            result.Should().Be(SessionSortByProperty.SessionNumberAsc);
        }

        [TestCaseSource(nameof(MappedSortBy))]
        public void GetSessionsSortByModelName_ShouldReturnExpectedMapping_WhenCalledWithSortBy(SessionSortBy modelSessionSortBy, SessionSortByProperty expectedResult)
        {
            var sortOrderResult = SessionSortByMapper.GetProcessSessionSortByModelName(modelSessionSortBy);
            var result = ((Some<SessionSortByProperty>)sortOrderResult).Value;
            result.Should().Be(expectedResult);
        }

        private static IEnumerable<TestCaseData> MappedSortBy() =>
            new[] {
                    (SessionSortBy.ResourceNameAsc, SessionSortByProperty.ResourceNameAsc),
                    (SessionSortBy.ResourceNameDesc, SessionSortByProperty.ResourceNameDesc),
                    (SessionSortBy.EndTimeAsc, SessionSortByProperty.EndTimeAsc),
                    (SessionSortBy.EndTimeDesc, SessionSortByProperty.EndTimeDesc),
                    (SessionSortBy.ExceptionTypeAsc, SessionSortByProperty.ExceptionTypeAsc),
                    (SessionSortBy.ExceptionTypeDesc, SessionSortByProperty.ExceptionTypeDesc),
                    (SessionSortBy.LatestStageAsc, SessionSortByProperty.LatestStageAsc),
                    (SessionSortBy.LatestStageDesc, SessionSortByProperty.LatestStageDesc),
                    (SessionSortBy.ProcessNameAsc, SessionSortByProperty.ProcessNameAsc),
                    (SessionSortBy.ProcessNameDesc, SessionSortByProperty.ProcessNameDesc),
                    (SessionSortBy.SessionNumberAsc, SessionSortByProperty.SessionNumberAsc),
                    (SessionSortBy.SessionNumberDesc, SessionSortByProperty.SessionNumberDesc),
                    (SessionSortBy.StageStartedAsc, SessionSortByProperty.StageStartedAsc),
                    (SessionSortBy.StageStartedDesc, SessionSortByProperty.StageStartedDesc),
                    (SessionSortBy.StartTimeAsc, SessionSortByProperty.StartTimeAsc),
                    (SessionSortBy.StartTimeDesc, SessionSortByProperty.StartTimeDesc),
                    (SessionSortBy.UserNameAsc, SessionSortByProperty.UserAsc),
                    (SessionSortBy.UserNameDesc, SessionSortByProperty.UserDesc),
                    (SessionSortBy.StatusAsc, SessionSortByProperty.StatusAsc),
                    (SessionSortBy.StatusDesc, SessionSortByProperty.StatusDesc)
                }
                .ToTestCaseData();
    }
}
