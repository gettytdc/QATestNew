namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Filters;
    using BluePrism.Server.Domain.Models.DataFilters;
    using BpLibAdapters.Mappers;
    using BpLibAdapters.Mappers.FilterMappers;
    using NUnit.Framework;
    using BluePrism.Server.Domain.Models;
    using CommonTestClasses.Extensions;
    using Domain;
    using Domain.PagingTokens;
    using FluentAssertions.Execution;
    using Func;
    using Server.Domain.Models.Pagination;
    using static Func.OptionHelper;
    using ScheduleLogParameters = Server.Domain.Models.ScheduleLogParameters;

    [TestFixture(Category = "Unit Test")]
    public class ScheduleLogParametersMapperTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var filterMappers = new IFilterMapper[]
            {
                new EqualsFilterMapper(),
                new MultiValueFilterMapper(),
                new NullFilterMapper(),
            };
            FilterMapper.SetFilterMappers(filterMappers);
        }

        [TestCaseSource(nameof(OptionsForScheduleLogParameters))]
        public void ToBluePrismObject_WithTestDomainScheduleLogParameters_ReturnsCorrectlyMappedResult(Option<PagingToken<int>> domainToken, Option<ScheduleLogsPagingToken> bluePrismToken)
        {
            var domainScheduleLogParameters = new Domain.ScheduleLogParameters
            {
                ScheduleId = new EqualsFilter<int>(21),
                ItemsPerPage = 24,
                StartTime = new EqualsFilter<DateTimeOffset>(new DateTime(2020, 12, 21, 14, 17, 05, 322)),
                EndTime = new EqualsFilter<DateTimeOffset>(new DateTime(2020, 12, 22, 14, 17, 05, 322)),
                ScheduleLogStatus = new MultiValueFilter<Domain.ScheduleLogStatus>(
                     new Filter<Domain.ScheduleLogStatus>[]
                     {
                         new EqualsFilter<Domain.ScheduleLogStatus>(Domain.ScheduleLogStatus.Completed),
                         new EqualsFilter<Domain.ScheduleLogStatus>(Domain.ScheduleLogStatus.PartExceptioned)
                     }),
                PagingToken = domainToken,
            };

            var expected = new ScheduleLogParameters
            {
                ScheduleId = new EqualsDataFilter<int> { EqualTo = 21 },
                ItemsPerPage = 24,
                StartTime = new EqualsDataFilter<DateTimeOffset> { EqualTo = new DateTime(2020, 12, 21, 14, 17, 05, 322) },
                EndTime = new EqualsDataFilter<DateTimeOffset> { EqualTo = new DateTime(2020, 12, 22, 14, 17, 05, 322) },
                ScheduleLogStatus = new MultiValueDataFilter<ItemStatus>(
                     new DataFilter<ItemStatus>[]
                     {
                         new EqualsDataFilter<ItemStatus> { EqualTo = ItemStatus.Completed },
                         new EqualsDataFilter<ItemStatus> { EqualTo = ItemStatus.PartExceptioned }
                     }),
                PagingToken = bluePrismToken,
            };

            var mappedDomainScheduleLog = domainScheduleLogParameters.ToBluePrismObject();

            mappedDomainScheduleLog.ShouldRuntimeTypesBeEquivalentTo(expected);
        }

        [TestCaseSource(nameof(OptionsForScheduleLogParameters))]
        public void ToBluePrismObject_WithTestNullFilterDomainScheduleLogParameters_ReturnsCorrectlyMappedResult(Option<PagingToken<int>> domainToken, Option<ScheduleLogsPagingToken> bluePrismToken)
        {
            var domainScheduleLogParameters = new Domain.ScheduleLogParameters
            {
                ScheduleId = new NullFilter<int>(),
                ItemsPerPage = 24,
                StartTime = new NullFilter<DateTimeOffset>(),
                EndTime = new NullFilter<DateTimeOffset>(),
                ScheduleLogStatus = new NullFilter<ScheduleLogStatus>(),
                PagingToken = domainToken,
            };

            var expected = new Server.Domain.Models.ScheduleLogParameters
            {
                ScheduleId = new NullDataFilter<int>(),
                ItemsPerPage = 24,
                StartTime = new NullDataFilter<DateTimeOffset>(),
                EndTime = new NullDataFilter<DateTimeOffset>(),
                ScheduleLogStatus = new MultiValueDataFilter<ItemStatus>(new DataFilter<ItemStatus>[]{}),
                PagingToken = bluePrismToken,
            };

            var mappedDomainScheduleLog = domainScheduleLogParameters.ToBluePrismObject();

            mappedDomainScheduleLog.ShouldRuntimeTypesBeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> OptionsForScheduleLogParameters()
        {
            var domainToken = new PagingToken<int>
            {
                DataType = "int",
                ParametersHashCode = "123456789",
                PreviousIdValue = 2,
                PreviousSortColumnValue = "somecolumn",
            };

            var bluePrismToken = new ScheduleLogsPagingToken
            {
                DataType = "int",
                PreviousIdValue = 2,
                PreviousSortColumnValue = "somecolumn",
            };

            yield return new TestCaseData(None<PagingToken<int>>(), None<ScheduleLogsPagingToken>());
            yield return new TestCaseData(Some(domainToken), Some(bluePrismToken));
        }

        [Test]
        public void ToBluePrismObject_WithTestDomainScheduleLogParameters_ReturnsDifferentMappedResult()
        {
            var domainScheduleLogParameters = new Domain.ScheduleLogParameters
            {
                ScheduleId = new EqualsFilter<int>(21),
                ItemsPerPage = 24,
                StartTime = new EqualsFilter<DateTimeOffset>(new DateTime(2020, 12, 21, 14, 17, 05, 322)),
                EndTime = new EqualsFilter<DateTimeOffset>(new DateTime(2020, 12, 22, 14, 17, 05, 322)),
                ScheduleLogStatus = new MultiValueFilter<Domain.ScheduleLogStatus>(
                     new Filter<Domain.ScheduleLogStatus>[]
                     {
                         new EqualsFilter<Domain.ScheduleLogStatus>(Domain.ScheduleLogStatus.Completed),
                         new EqualsFilter<Domain.ScheduleLogStatus>(Domain.ScheduleLogStatus.PartExceptioned)
                     })
            };

            var expected = new ScheduleLogParameters
            {
                ScheduleId = new EqualsDataFilter<int> { EqualTo = 99 },
                ItemsPerPage = 5,
                StartTime = new EqualsDataFilter<DateTimeOffset> { EqualTo = new DateTime(2015, 12, 21, 14, 17, 05, 322) },
                EndTime = new EqualsDataFilter<DateTimeOffset> { EqualTo = new DateTime(2010, 12, 22, 14, 17, 05, 322) },
                ScheduleLogStatus = new MultiValueDataFilter<ItemStatus>(
                     new DataFilter<ItemStatus>[]
                     {
                         new EqualsDataFilter<ItemStatus> { EqualTo = ItemStatus.Pending },
                         new EqualsDataFilter<ItemStatus> { EqualTo = ItemStatus.PartExceptioned }
                     }),
                PagingToken = None<ScheduleLogsPagingToken>(),
            };

            var mappedDomainScheduleLog = domainScheduleLogParameters.ToBluePrismObject();

            using (var scope = new AssertionScope())
            {
                mappedDomainScheduleLog.ShouldRuntimeTypesBeEquivalentTo(expected);
                var isNotEquivalent = scope.Discard().Any();

                Execute.Assertion.ForCondition(isNotEquivalent).FailWith("Expected {0} not to be equivalent to {1}, but they are", mappedDomainScheduleLog, expected);
            }
        }
    }
}
