namespace BluePrism.Api.UnitTests.Mappers
{
    using Api.Mappers;
    using Api.Mappers.FilterMappers;
    using CommonTestClasses;
    using CommonTestClasses.Extensions;
    using Domain.Filters;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;
    using static Func.OptionHelper;
    using RetirementStatus = Domain.RetirementStatus;
    using ScheduleParameters = Domain.ScheduleParameters;

    [TestFixture]
    public class ScheduleParametersMapperTests
    {
        [SetUp]
        public void SetUp() =>
            FilterModelMapper.SetFilterModelMappers(new IFilterModelMapper[]
            {
                new EqualFilterModelMapper(),
                new StartsWithStringFilterModelMapper(),
                new NullFilterModelMapper()
            });

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedData_WhenCalled()
        {
            var modelScheduleParameters = new Models.ScheduleParameters
            {
                Name = new StartsWithStringFilterModel { Strtw = "test" },
                RetirementStatus = new[] { Models.RetirementStatus.Active },
                ItemsPerPage = 10
            };
            var domainScheduleParameters = new ScheduleParameters
            {
                Name = new StringStartsWithFilter("test"),
                RetirementStatus = new MultiValueFilter<RetirementStatus>(new Filter<RetirementStatus>[]
                {
                    new EqualsFilter<RetirementStatus>(RetirementStatus.Active)
                }),
                ItemsPerPage = 10,
                PagingToken = None<PagingToken<string>>()
            };
            var result = modelScheduleParameters.ToDomainObject();
            domainScheduleParameters.ShouldRuntimeTypesBeEquivalentTo(result);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedPagingToken_WhenCalledWithNullPagingToken()
        {
            var scheduleParameters = new Models.ScheduleParameters { ItemsPerPage = 5, PagingToken = null };

            var expected = new ScheduleParameters() { ItemsPerPage = 5, PagingToken = None<PagingToken<string>>(), };

            var actual = scheduleParameters.ToDomainObject();

            actual.PagingToken.Should().Be(expected.PagingToken);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedPagingToken_WhenCalledWithPagingToken()
        {
            var scheduleParameters = new Models.ScheduleParameters { ItemsPerPage = 7 };

            var domainPagingToken = new PagingToken<string>
            {
                DataType = "DateTimeOffset",
                PreviousSortColumnValue = "2021-02-19T11:12:58.1400000+00:00",
                PreviousIdValue = "test",
                ParametersHashCode = scheduleParameters.ToDomainObject().GetHashCodeForValidation()
            };

            var expected = new ScheduleParameters { ItemsPerPage = 7, PagingToken = Some(domainPagingToken), };

            scheduleParameters.PagingToken = domainPagingToken.ToString();

            var actual = scheduleParameters.ToDomainObject();

            ((Some<PagingToken<string>>)actual.PagingToken).Value
                .ShouldBeEquivalentTo(((Some<PagingToken<string>>)expected.PagingToken).Value);
        }

        [Test]
        public void ToDomainObject_ShouldReturnSamePagingToken_WhenParametersNotChanged()
        {
            var originalScheduleParameters = new Models.ScheduleParameters
            {
                ItemsPerPage = 3,
                Name = new StartsWithStringFilterModel(),
                RetirementStatus = new[] { Models.RetirementStatus.Active, Models.RetirementStatus.Retired }
            };
            var updateScheduleParameters = new Models.ScheduleParameters
            {
                ItemsPerPage = 3,
                Name = new StartsWithStringFilterModel(),
                RetirementStatus = new[] { Models.RetirementStatus.Active, Models.RetirementStatus.Retired }
            };

            var originalPagingTokenPagingToken = new PagingToken<string>
            {
                DataType = "DateTimeOffset",
                PreviousIdValue = "test",
                ParametersHashCode = originalScheduleParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            var updatedPagingTokenPagingToken = new PagingToken<string>
            {
                DataType = "DateTimeOffset",
                PreviousIdValue = "test",
                ParametersHashCode = updateScheduleParameters.ToDomainObject().GetHashCodeForValidation(),
            };

            originalPagingTokenPagingToken.ShouldBeEquivalentTo(updatedPagingTokenPagingToken);
        }

        [Test]
        public void ToDomainObject_ShouldReturnDifferentPagingToken_WhenParametersChanged()
        {
            var originalScheduleParameters = SchedulesHelper.GetTestDomainScheduleParameters();
            var updateScheduleParameters = SchedulesHelper.GetTestDomainScheduleParameters();
            updateScheduleParameters.RetirementStatus = null;

            var originalPagingTokenPagingToken = new PagingToken<string>
            {
                DataType = "DateTimeOffset",
                PreviousIdValue = "test",
                ParametersHashCode = originalScheduleParameters.GetHashCodeForValidation(),
            };

            var updatedPagingTokenPagingToken = new PagingToken<string>
            {
                DataType = "DateTimeOffset",
                PreviousIdValue = "test",
                ParametersHashCode = updateScheduleParameters.GetHashCodeForValidation(),
            };

            originalPagingTokenPagingToken.Should().NotBe(updatedPagingTokenPagingToken);
        }
    }
}
