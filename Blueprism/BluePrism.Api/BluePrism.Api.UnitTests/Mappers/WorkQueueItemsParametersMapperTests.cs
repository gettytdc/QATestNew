namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
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
    using WorkQueueItemParameters = Models.WorkQueueItemParameters;
    using static Func.OptionHelper;

    public class WorkQueueItemsParametersMapperTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var filterMappers = new IFilterModelMapper[]
            {
                new EqualFilterModelMapper(),
                new GreaterThanOrEqualToFilterModelMapper(),
                new LessThanOrEqualToFilterModelMapper(),
                new NullFilterModelMapper(),
                new RangeFilterModelMapper(),
                new StartsWithStringFilterModelMapper(),
                new StartsWithOrContainsStringFilterModelMapper(),
            };

            FilterModelMapper.SetFilterModelMappers(filterMappers);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var testDateGte = new DateTime(2020, 12, 11, 14, 17, 05, 322);
            var testDateLte = new DateTime(2020, 12, 21, 14, 17, 05, 322);

            var workItemParametersDomain = new Domain.WorkQueueItemParameters
            {
                SortBy = WorkQueueItemSortByProperty.LastUpdatedDesc,
                KeyValue = new StringStartsWithFilter("test"),
                Status = new StringStartsWithFilter("t"),
                ExceptionReason = new StringStartsWithFilter("ex"),
                WorkTime = new RangeFilter<int>(0, 20),
                Attempt = new EqualsFilter<int>(7),
                Priority = new RangeFilter<int>(5, 10),
                LoadedDate = new EqualsFilter<DateTimeOffset>(new DateTime(2020, 12, 21, 14, 17, 05, 322)),
                DeferredDate = new NullFilter<DateTimeOffset>(),
                LockedDate = new RangeFilter<DateTimeOffset>(testDateGte, testDateLte),
                CompletedDate = new EqualsFilter<DateTimeOffset>(new DateTime(2020, 12, 21, 14, 17, 05, 322)),
                LastUpdated = new EqualsFilter<DateTimeOffset>(new DateTime(2020, 12, 21, 14, 17, 05, 322)),
                ExceptionedDate = new EqualsFilter<DateTimeOffset>(new DateTime(2020, 12, 21, 14, 17, 05, 322)),
                PagingToken = None<PagingToken<long>>(),
            };

            var workItemParametersModel = new WorkQueueItemParameters
            {
                SortBy = WorkQueueItemSortBy.LastUpdatedDesc,
                KeyValue = new StartsWithOrContainsStringFilterModel { Strtw = "test" },
                Status = new StartsWithOrContainsStringFilterModel { Strtw = "t" },
                ExceptionReason = new StartsWithOrContainsStringFilterModel { Strtw = "ex" },
                TotalWorkTime = new RangeFilterModel<int?> { Gte = 0, Lte = 20 },
                Attempt = new RangeFilterModel<int?> { Eq = 7 },
                Priority = new RangeFilterModel<int?> { Gte = 5, Lte = 10 },
                LoadedDate = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2020-12-21T14:17:05.322Z") },
                DeferredDate = new RangeFilterModel<DateTimeOffset?>(),
                LockedDate =
                    new RangeFilterModel<DateTimeOffset?>
                    {
                        Gte = DateTimeOffset.Parse("2020-12-11T14:17:05.322Z"),
                        Lte = DateTimeOffset.Parse("2020-12-21T14:17:05.322Z")
                    },
                CompletedDate = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2020-12-21T14:17:05.322Z") },
                LastUpdated = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2020-12-21T14:17:05.322Z") },
                ExceptionedDate = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2020-12-21T14:17:05.322Z") }
            };

            var result = workItemParametersModel.ToDomainObject();

            workItemParametersDomain.ShouldRuntimeTypesBeEquivalentTo(result);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedPagingToken_WhenCalledWithNullPagingToken()
        {
            var workQueueItemParameters = new WorkQueueItemParameters
            {
                ItemsPerPage = 5,
                PagingToken = null,
                SortBy = WorkQueueItemSortBy.AttemptAsc
            };

            var expected = new Domain.WorkQueueItemParameters
            {
                ItemsPerPage = 5,
                PagingToken = None<PagingToken<long>>(),
                SortBy = WorkQueueItemSortByProperty.AttemptAsc
            };

            var actual = workQueueItemParameters.ToDomainObject();

            actual.PagingToken.Should().Be(expected.PagingToken);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedPagingToken_WhenCalledWithPagingToken()
        {
            var workQueueItemParameters = GetTestWorkQueueItemParameters();

            var domainPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousSortColumnValue = "2021-02-19T11:12:58.1400000+00:00",
                PreviousIdValue = 75,
                ParametersHashCode = workQueueItemParameters.ToDomainObject().GetHashCodeForValidation()
            };
            
            var expected = new Domain.WorkQueueItemParameters
            {
                ItemsPerPage = 7,
                PagingToken = Some(domainPagingToken),
                SortBy = WorkQueueItemSortByProperty.ExceptionDesc,
            };

            workQueueItemParameters.PagingToken = domainPagingToken.ToString();

            var actual = workQueueItemParameters.ToDomainObject();

            ((Some<PagingToken<long>>)actual.PagingToken).Value
                .ShouldBeEquivalentTo(((Some<PagingToken<long>>)expected.PagingToken).Value);
        }
        
        [Test]
        public void ToDomainObject_ShouldReturnSamePagingToken_WhenParametersNotChanged()
        {
            var originalWorkQueueItemParameters = GetTestWorkQueueItemParameters();
            var updatedWorkQueueItemParameters = GetTestWorkQueueItemParameters();

            var originalPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousSortColumnValue = new DateTime(2021, 01, 14, 12, 45, 22).ToString(),
                PreviousIdValue = 75,
                ParametersHashCode = originalWorkQueueItemParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            var updatedPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousSortColumnValue = new DateTime(2021, 01, 14, 12, 45, 22).ToString(),
                PreviousIdValue = 75,
                ParametersHashCode = updatedWorkQueueItemParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            originalPagingTokenPagingToken.ShouldBeEquivalentTo(updatedPagingTokenPagingToken);
        }

        [Test]
        public void ToDomainObject_ShouldReturnDifferentPagingToken_WhenFilterParametersChanged()
        {
            var originalWorkQueueItemParameters = GetTestWorkQueueItemParameters();
            var updatedWorkQueueItemParameters = GetTestWorkQueueItemParameters();
            updatedWorkQueueItemParameters.LoadedDate = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2021-01-10T11:17:05.322Z") };

            var originalPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousSortColumnValue = new DateTime(2021, 01, 14, 12, 45, 22).ToString(),
                PreviousIdValue = 75,
                ParametersHashCode = originalWorkQueueItemParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            var updatedPagingTokenPagingToken = new PagingToken<long>
            {
                DataType = "DateTimeOffset",
                PreviousSortColumnValue = new DateTime(2021, 01, 14, 12, 45, 22).ToString(),
                PreviousIdValue = 75,
                ParametersHashCode = updatedWorkQueueItemParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            originalPagingTokenPagingToken.Should().NotBe(updatedPagingTokenPagingToken);
        }

        private WorkQueueItemParameters GetTestWorkQueueItemParameters() =>
            new WorkQueueItemParameters
            {
                ItemsPerPage = 7,
                SortBy = WorkQueueItemSortBy.ExceptionDesc,
                KeyValue = new StartsWithOrContainsStringFilterModel { Strtw = "test2" },
                Status = new StartsWithOrContainsStringFilterModel { Strtw = "t2" },
                ExceptionReason = new StartsWithOrContainsStringFilterModel { Strtw = "ex" },
                TotalWorkTime = new RangeFilterModel<int?> { Gte = null, Lte = 20 },
                Attempt = new RangeFilterModel<int?> { Eq = null },
                Priority = new RangeFilterModel<int?> { Gte = null, Lte = null },
                LoadedDate = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2020-12-21T11:17:05.322Z") },
                DeferredDate = new RangeFilterModel<DateTimeOffset?>(),
                LockedDate =
                    new RangeFilterModel<DateTimeOffset?> { Gte = DateTimeOffset.Parse("2020-12-11T11:17:05.322Z"), Lte = DateTimeOffset.Parse("2020-12-21T11:17:05.322Z") },
                CompletedDate = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2020-12-21T11:17:05.322Z") },
                LastUpdated = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2020-12-21T11:17:05.322Z") },
                ExceptionedDate = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("2020-12-21T11:17:05.322Z") }
            };
    }
}
