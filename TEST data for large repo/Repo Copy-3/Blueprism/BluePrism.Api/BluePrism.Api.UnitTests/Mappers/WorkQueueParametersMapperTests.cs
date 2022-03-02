namespace BluePrism.Api.UnitTests.Mappers
{
    using System.Collections.Generic;
    using Api.Mappers;
    using Api.Mappers.FilterMappers;
    using CommonTestClasses.Extensions;
    using Domain;
    using Domain.Filters;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;
    using QueueStatus = Domain.QueueStatus;
    using static Func.OptionHelper;

    public class WorkQueueParametersMapperTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var filterMappers = new IFilterModelMapper[]
            {
                new EqualFilterModelMapper(), new GreaterThanOrEqualToFilterModelMapper(),
                new LessThanOrEqualToFilterModelMapper(), new NullFilterModelMapper(), new RangeFilterModelMapper(),
                new StartsWithOrContainsStringFilterModelMapper(),
            };

            FilterModelMapper.SetFilterModelMappers(filterMappers);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedWorkQueueModel_WhenCalled()
        {
            var pagingTokenDetails = new
            {
                DataType = "int", PreviousIdValue = 1, ParametersHashCode = "12345", PreviousSortColumnValue = "name"
            };

            var workQueueParametersDomain = new Domain.WorkQueueParameters
            {
                SortBy = WorkQueueSortByProperty.NameAsc,
                ItemsPerPage = 10,
                NameFilter = new StringContainsFilter("test"),
                KeyFieldFilter = new EqualsFilter<string>("test"),
                MaxAttemptsFilter = new GreaterThanOrEqualToFilter<int>(2),
                QueueStatusFilter = new EqualsFilter<QueueStatus>(QueueStatus.Paused),
                PendingItemCountFilter = new RangeFilter<int>(2,10),
                CompletedItemCountFilter = new EqualsFilter<int>(1),
                LockedItemCountFilter = new NullFilter<int>(),
                ExceptionedItemCountFilter = new EqualsFilter<int>(0),
                TotalItemCountFilter = new RangeFilter<int>(5,20),
                AverageWorkTimeFilter = new EqualsFilter<int>(1),
                TotalCaseDurationFilter = new EqualsFilter<int>(1),
                PagingToken = Some(new PagingToken<int>()
                {
                    DataType = pagingTokenDetails.DataType,
                    PreviousIdValue = pagingTokenDetails.PreviousIdValue,
                    ParametersHashCode = pagingTokenDetails.ParametersHashCode,
                    PreviousSortColumnValue = pagingTokenDetails.PreviousSortColumnValue
                }),
            };

            var workQueueParametersModel = new Models.WorkQueueParameters
            {
                SortBy = WorkQueueSortBy.NameAsc,
                ItemsPerPage = 10,
                Name = new StartsWithOrContainsStringFilterModel{ Ctn = "test"},
                KeyField = new StartsWithOrContainsStringFilterModel{Eq = "test"},
                MaxAttempts = new RangeFilterModel<int?>{ Gte = 2},
                Status = new BasicFilterModel<Models.QueueStatus> { Eq = Models.QueueStatus.Paused },
                PendingItemCount = new RangeFilterModel<int?>{Gte=2, Lte = 10},
                CompletedItemCount = new RangeFilterModel<int?> {Eq = 1},
                LockedItemCount = new RangeFilterModel<int?>(),
                ExceptionedItemCount = new RangeFilterModel<int?>{Eq = 0},
                TotalItemCount = new RangeFilterModel<int?>{Gte = 5,Lte = 20},
                AverageWorkTime = new RangeFilterModel<int?>{Eq = 1},
                TotalCaseDuration = new RangeFilterModel<int?>{Eq = 1},
                PagingToken = new PagingTokenModel<int>()
                {
                    DataType = pagingTokenDetails.DataType,
                    PreviousIdValue = pagingTokenDetails.PreviousIdValue,
                    ParametersHashCode = pagingTokenDetails.ParametersHashCode,
                    PreviousSortColumnValue = pagingTokenDetails.PreviousSortColumnValue
                }
            };

            var result = workQueueParametersModel.ToDomainObject();
            workQueueParametersDomain.ShouldRuntimeTypesBeEquivalentTo(result);
        }

        [Test]
        public void ToDomainObject_ShouldReturnNonePagingToken_WhenCalledWithNonePagingToken()
        {
            var workQueueParametersModel = new Models.WorkQueueParameters
            {
                PagingToken = null
            };

            var result = workQueueParametersModel.ToDomainObject();
            result.PagingToken.Should().BeAssignableTo<None>();
        }
        [TestCaseSource(nameof(MappedStatuses))]
        public void ToDomainObject_ShouldReturnCorrectQueueStatus_WhenCalled(Models.QueueStatus modelQueueStatus, QueueStatus domainQueueStatus)
        {
            var workQueueParametersModel = new Models.WorkQueueParameters
            {
                Status = new BasicFilterModel<Models.QueueStatus> { Eq = modelQueueStatus },
            };

            var status = workQueueParametersModel.ToDomainObject().QueueStatusFilter;
            status.Should().BeOfType<EqualsFilter<QueueStatus>>();
            ((EqualsFilter<QueueStatus>)status).EqualTo.Should().Be(domainQueueStatus);
        }

        private static IEnumerable<TestCaseData> MappedStatuses() =>
            new[]
            {
                (Models.QueueStatus.Paused, QueueStatus.Paused),
                (Models.QueueStatus.Running, QueueStatus.Running)
            }.ToTestCaseData();


    }
}
