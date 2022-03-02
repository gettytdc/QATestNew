namespace BluePrism.Api.UnitTests.Mappers
{
    using Api.Mappers;
    using CommonTestClasses.Extensions;
    using Domain;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;
    using System.Collections.Generic;

    public class WorkQueueSortByMapperTests
    {
        [Test]
        public void GetWorkQueueSortByModelName_ShouldReturnDefaultSortOrder_WhenCalledWithNullSortBy()
        {
            WorkQueueSortBy? nullSortOrderBy = null;
            var sortOrderResult = WorkQueueSortByMapper.GetWorkQueueSortByModelName(nullSortOrderBy);

            var result = ((Some<WorkQueueSortByProperty>)sortOrderResult).Value;

            result.Should().Be(WorkQueueSortByProperty.NameAsc);
        }

        [Test, TestCaseSource(nameof(WorkQueueSortByCases))]
        public void GetWorkQueueSortByModelName_ShouldReturnExpectedMapping_WhenCalledWithSortBy(
            WorkQueueSortBy sortBy, WorkQueueSortByProperty domainSortByProperty)
        {
            var sortOrderResult = WorkQueueSortByMapper.GetWorkQueueSortByModelName(sortBy);
            var result = ((Some<WorkQueueSortByProperty>)sortOrderResult).Value;

            result.Should().Be(domainSortByProperty);
        }

        private static IEnumerable<TestCaseData> WorkQueueSortByCases() =>
            new[]
            {
                (Models.WorkQueueSortBy.NameAsc, Domain.WorkQueueSortByProperty.NameAsc),
                (Models.WorkQueueSortBy.NameDesc, Domain.WorkQueueSortByProperty.NameDesc),
                (Models.WorkQueueSortBy.StatusAsc, Domain.WorkQueueSortByProperty.RunningAsc),
                (Models.WorkQueueSortBy.StatusDesc, Domain.WorkQueueSortByProperty.RunningDesc),
                (Models.WorkQueueSortBy.KeyFieldAsc, Domain.WorkQueueSortByProperty.KeyFieldAsc),
                (Models.WorkQueueSortBy.KeyFieldDesc, Domain.WorkQueueSortByProperty.KeyFieldDesc),
                (Models.WorkQueueSortBy.MaxAttemptsAsc, Domain.WorkQueueSortByProperty.MaxAttemptsAsc),
                (Models.WorkQueueSortBy.MaxAttemptsDesc, Domain.WorkQueueSortByProperty.MaxAttemptsDesc),
                (Models.WorkQueueSortBy.IsEncryptedAsc, Domain.WorkQueueSortByProperty.EncryptIdAsc),
                (Models.WorkQueueSortBy.IsEncryptedDesc, Domain.WorkQueueSortByProperty.EncryptIdDesc),
                (Models.WorkQueueSortBy.TotalItemCountAsc, Domain.WorkQueueSortByProperty.TotalAsc),
                (Models.WorkQueueSortBy.TotalItemCountDesc, Domain.WorkQueueSortByProperty.TotalDesc),
                (Models.WorkQueueSortBy.CompletedItemCountAsc, Domain.WorkQueueSortByProperty.CompletedAsc),
                (Models.WorkQueueSortBy.CompletedItemCountDesc, Domain.WorkQueueSortByProperty.CompletedDesc),
                (Models.WorkQueueSortBy.LockedItemCountAsc, Domain.WorkQueueSortByProperty.LockedAsc),
                (Models.WorkQueueSortBy.LockedItemCountDesc, Domain.WorkQueueSortByProperty.LockedDesc),
                (Models.WorkQueueSortBy.PendingItemCountAsc, Domain.WorkQueueSortByProperty.PendingAsc),
                (Models.WorkQueueSortBy.PendingItemCountDesc, Domain.WorkQueueSortByProperty.PendingDesc),
                (Models.WorkQueueSortBy.ExceptionedItemCountAsc, Domain.WorkQueueSortByProperty.ExceptionedAsc),
                (Models.WorkQueueSortBy.ExceptionedItemCountDesc, Domain.WorkQueueSortByProperty.ExceptionedDesc),
                (Models.WorkQueueSortBy.TotalCaseDurationAsc, Domain.WorkQueueSortByProperty.TotalWorkTimeAsc),
                (Models.WorkQueueSortBy.TotalCaseDurationDesc, Domain.WorkQueueSortByProperty.TotalWorkTimeDesc),
                (Models.WorkQueueSortBy.AverageWorkTimeAsc, Domain.WorkQueueSortByProperty.AverageWorkedTimeAsc),
                (Models.WorkQueueSortBy.AverageWorkTimeDesc, Domain.WorkQueueSortByProperty.AverageWorkedTimeDesc)
            }.ToTestCaseData();
    }
}
