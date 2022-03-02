namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using Api.Mappers;
    using Api.Mappers.FilterMappers;
    using CommonTestClasses.Extensions;
    using Domain.Filters;
    using Domain.PagingTokens;
    using Models;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;

    using static Func.OptionHelper;

    [TestFixture]
    public class ScheduleLogParametersMapperTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var filterMappers = new IFilterModelMapper[]
            {
                new EqualFilterModelMapper(),                
                new RangeFilterModelMapper(),                
                new NullFilterModelMapper(),
            };
            FilterModelMapper.SetFilterModelMappers(filterMappers);
        }

        [TestCaseSource(nameof(OptionAndTokenForScheduleLogParameters))]
        public void ToDomainObject_ShouldReturnCorrectlyMappedModel_WhenCalled(Option<PagingToken<int>> domainToken, string modelToken)
        {
            var scheduleLogParametersDomain = new Domain.ScheduleLogParameters
            {
                ScheduleId = new NullFilter<int>(),
                ItemsPerPage = 7,
                StartTime = new EqualsFilter<DateTimeOffset>(new DateTimeOffset(2020, 12, 21, 14, 17, 05, 322, TimeSpan.FromHours(1))),
                EndTime = new EqualsFilter<DateTimeOffset>(new DateTimeOffset(2020, 12, 22, 15, 45, 05, 741, TimeSpan.FromHours(1))),
                ScheduleLogStatus = new MultiValueFilter<Domain.ScheduleLogStatus>(
                     new Filter<Domain.ScheduleLogStatus>[]
                     {
                         new EqualsFilter<Domain.ScheduleLogStatus>(Domain.ScheduleLogStatus.Completed),
                         new EqualsFilter<Domain.ScheduleLogStatus>(Domain.ScheduleLogStatus.PartExceptioned),
                     }),
                PagingToken = domainToken,
            };

            var scheduleLogParametersModel = new Models.ScheduleLogParameters
            {
                ItemsPerPage = 7,
                StartTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 21, 14, 17, 05, 322, TimeSpan.FromHours(1)) },
                EndTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 22, 15, 45, 05, 741, TimeSpan.FromHours(1)) },
                ScheduleLogStatus = new CommaDelimitedCollection<ScheduleLogStatus>(new ScheduleLogStatus[] { ScheduleLogStatus.Completed, ScheduleLogStatus.PartExceptioned }),
                PagingToken = modelToken,
            };

            var result = scheduleLogParametersModel.ToDomainObject();

            scheduleLogParametersDomain.ShouldRuntimeTypesBeEquivalentTo(result);
        }

        private static IEnumerable<TestCaseData> OptionAndTokenForScheduleLogParameters()
        {
            var domainToken = new PagingToken<int>
            {
                DataType = "int",
                ParametersHashCode = "1",
                PreviousIdValue = 2,
                PreviousSortColumnValue = "somecolumn",
            };

            var bluePrismToken = "eyJQcmV2aW91c0lkVmFsdWUiOjIsIlByZXZpb3VzU29ydENvbHVtblZhbHVlIjoic29tZWNvbHVtbiIsIkRhdGFUeXBlIjoiaW50IiwiUGFyYW1ldGVyc0hhc2hDb2RlIjoiMSJ9";

            yield return new TestCaseData(None<PagingToken<int>>(), null);
            yield return new TestCaseData(Some(domainToken), bluePrismToken);
        }

        [Test]
        public void ToDomainObject_ShouldReturnDifferentMappedModel_WhenCalled()
        {
            var scheduleLogParametersDomain = new Domain.ScheduleLogParameters
            {
                ItemsPerPage = 7,
                StartTime = new EqualsFilter<DateTimeOffset>(new DateTimeOffset(2020, 12, 22, 14, 17, 05, 322, TimeSpan.FromHours(1))),
                EndTime = new EqualsFilter<DateTimeOffset>(new DateTimeOffset(2020, 12, 22, 15, 38, 45, 411, TimeSpan.FromHours(1))),
                ScheduleLogStatus = new MultiValueFilter<Domain.ScheduleLogStatus>(
                     new Filter<Domain.ScheduleLogStatus>[]
                     {
                         new EqualsFilter<Domain.ScheduleLogStatus>(Domain.ScheduleLogStatus.Completed),
                         new EqualsFilter<Domain.ScheduleLogStatus>(Domain.ScheduleLogStatus.PartExceptioned),
                     })
            };

            var scheduleLogParametersModel = new Models.ScheduleLogParameters
            {
                ItemsPerPage = 7,
                StartTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 22, 14, 17, 05, 322, TimeSpan.FromHours(1)) },
                EndTime = new RangeFilterModel<DateTimeOffset?> { Eq = new DateTimeOffset(2020, 12, 22, 15, 38, 45, 411, TimeSpan.FromHours(1)) },
                ScheduleLogStatus = new CommaDelimitedCollection<ScheduleLogStatus>(new ScheduleLogStatus[] { ScheduleLogStatus.Completed, ScheduleLogStatus.PartExceptioned })
            };

            var result = scheduleLogParametersModel.ToDomainObject();

            scheduleLogParametersDomain.Should().NotBe(result);
        }

        [Test]
        public void ToDomainObject_ShouldCorrectlyMapScheduleIdToEqualsFilter_WhenCalledWithScheduleId()
        {
            var scheduleLogParametersModel = new Models.ScheduleLogParameters();
            const int scheduleId = 7;

            var result = scheduleLogParametersModel.ToDomainObject(scheduleId);

            result.ScheduleId.Should().BeAssignableTo<EqualsFilter<int>>();
            ((EqualsFilter<int>)result.ScheduleId).EqualTo.Should().Be(scheduleId);
        }

        [Test]
        public void ToDomainObject_ShouldCorrectlyMapScheduleIdToNullFilter_WhenCalledWithoutScheduleId()
        {
            var scheduleLogParametersModel = new Models.ScheduleLogParameters();

            var result = scheduleLogParametersModel.ToDomainObject();

            result.ScheduleId.Should().BeAssignableTo<NullFilter<int>>();
        }
    }
}
