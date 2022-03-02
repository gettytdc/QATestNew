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

    public class WorkQueueItemsSortByMapperTests
    {
        [Test]
        public void GetWorkQueueItemsSortByModelName_ShouldReturnDefaultSortOrder_WhenCalledWithNullSortBy()
        {
            WorkQueueItemSortBy? nullSortOrderBy = null;
            var sortOrderResult = WorkQueueItemsSortByMapper.GetWorkQueueItemsSortByModelName(nullSortOrderBy);

            var result = ((Some<WorkQueueItemSortByProperty>)sortOrderResult).Value;

            result.Should().Be(WorkQueueItemSortByProperty.LastUpdatedDesc);
        }
        
        [TestCaseSource(nameof(MappedSortBy))]
        public void GetWorkQueueItemsSortByModelName_ShouldReturnExpectedMapping_WhenCalledWithSortBy(WorkQueueItemSortBy modelWorkQueueItemSortBy, WorkQueueItemSortByProperty expectedResult)
        {
            var sortOrderResult = WorkQueueItemsSortByMapper.GetWorkQueueItemsSortByModelName(modelWorkQueueItemSortBy);
            var result = ((Some<WorkQueueItemSortByProperty>)sortOrderResult).Value;
            result.Should().Be(expectedResult);
        }
        
        private static IEnumerable<TestCaseData> MappedSortBy() =>
            new[] {
                    (WorkQueueItemSortBy.AttemptAsc, WorkQueueItemSortByProperty.AttemptAsc),
                    (WorkQueueItemSortBy.AttemptDesc, WorkQueueItemSortByProperty.AttemptDesc),
                    (WorkQueueItemSortBy.AttemptWorkTimeAsc, WorkQueueItemSortByProperty.AttemptWorkTimeAsc),
                    (WorkQueueItemSortBy.AttemptWorkTimeDesc, WorkQueueItemSortByProperty.AttemptWorkTimeDesc),
                    (WorkQueueItemSortBy.CompletedAsc, WorkQueueItemSortByProperty.CompletedAsc),
                    (WorkQueueItemSortBy.CompletedDesc, WorkQueueItemSortByProperty.CompletedDesc),
                    (WorkQueueItemSortBy.DeferredAsc, WorkQueueItemSortByProperty.DeferredAsc),
                    (WorkQueueItemSortBy.DeferredDesc, WorkQueueItemSortByProperty.DeferredDesc),
                    (WorkQueueItemSortBy.ExceptionAsc, WorkQueueItemSortByProperty.ExceptionAsc),
                    (WorkQueueItemSortBy.ExceptionDesc, WorkQueueItemSortByProperty.ExceptionDesc),
                    (WorkQueueItemSortBy.ExceptionReasonAsc, WorkQueueItemSortByProperty.ExceptionReasonAsc),
                    (WorkQueueItemSortBy.ExceptionReasonDesc, WorkQueueItemSortByProperty.ExceptionReasonDesc),
                    (WorkQueueItemSortBy.KeyValueAsc, WorkQueueItemSortByProperty.KeyValueAsc),
                    (WorkQueueItemSortBy.KeyValueDesc, WorkQueueItemSortByProperty.KeyValueDesc),
                    (WorkQueueItemSortBy.LastUpdatedAsc, WorkQueueItemSortByProperty.LastUpdatedAsc),
                    (WorkQueueItemSortBy.LastUpdatedDesc, WorkQueueItemSortByProperty.LastUpdatedDesc),
                    (WorkQueueItemSortBy.LoadedAsc, WorkQueueItemSortByProperty.LoadedAsc),
                    (WorkQueueItemSortBy.LoadedDesc, WorkQueueItemSortByProperty.LoadedDesc),
                    (WorkQueueItemSortBy.LockedAsc, WorkQueueItemSortByProperty.LockedAsc),
                    (WorkQueueItemSortBy.LockedDesc, WorkQueueItemSortByProperty.LockedDesc),
                    (WorkQueueItemSortBy.PriorityAsc, WorkQueueItemSortByProperty.PriorityAsc),
                    (WorkQueueItemSortBy.PriorityDesc, WorkQueueItemSortByProperty.PriorityDesc),
                    (WorkQueueItemSortBy.StatusAsc, WorkQueueItemSortByProperty.StatusAsc),
                    (WorkQueueItemSortBy.StatusDesc, WorkQueueItemSortByProperty.StatusDesc),
                    (WorkQueueItemSortBy.WorkTimeAsc, WorkQueueItemSortByProperty.WorkTimeAsc),
                    (WorkQueueItemSortBy.WorkTimeDesc, WorkQueueItemSortByProperty.WorkTimeDesc)
                }
                .ToTestCaseData();
    }
}
