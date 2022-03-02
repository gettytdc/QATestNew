namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using BpLibAdapters.Mappers.FilterMappers;
    using CommonTestClasses;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Server.Domain.Models.Pagination;

    [TestFixture(Category = "Unit Test")]
    public class WorkQueueParametersMapperTests
    {
        [Test]
        public void ToBluePrismObject_WithTestDomainWorkQueueParameters_ReturnsCorrectlyMappedResult()
        {
            var filterMappers = new IFilterMapper[]
            {
                new EqualsFilterMapper(),
                new StringContainsFilterMapper(),
                new StringStartsWithFilterMapper(),
                new GreaterThanOrEqualToFilterMapper(),
                new LessThanOrEqualToFilterMapper(),
                new RangeFilterMapper(),
                new NullFilterMapper(),
            };

            FilterMapper.SetFilterMappers(filterMappers);

            var domainWorkQueueParameters = WorkQueuesHelper.GetTestDomainWorkQueueParameters();
            var bluePrismWorkQueueParams = domainWorkQueueParameters.ToBluePrismObject();

            WorkQueuesHelper.ValidateParametersModelsAreEqual(bluePrismWorkQueueParams, domainWorkQueueParameters);
        }

        [Test]
        public void ToBluePrismObject_WithTestDomainWorkQueueParametersWithPagingToken_ReturnsCorrectlyMappedResult()
        {
            var filterMappers = new IFilterMapper[]
            {
                new NullFilterMapper(),
            };

            FilterMapper.SetFilterMappers(filterMappers);

            var domainWorkQueueParameters = WorkQueuesHelper.GetTestDomainWorkQueueParameters();
            var pagingToken = new PagingToken<int>()
            {
                DataType = "int",
                PreviousIdValue = 1,
                PreviousSortColumnValue = "name"
            };
            domainWorkQueueParameters.PagingToken = OptionHelper.Some(pagingToken);

            var bluePrismWorkQueueParams = domainWorkQueueParameters.ToBluePrismObject();

            var mappedPagingTokenResult = ((Some<WorkQueuePagingToken>) bluePrismWorkQueueParams.PagingToken).Value;
            mappedPagingTokenResult.DataType.Should().Be(pagingToken.DataType);
            mappedPagingTokenResult.PreviousIdValue.Should().Be(pagingToken.PreviousIdValue);
            mappedPagingTokenResult.PreviousSortColumnValue.Should().Be(pagingToken.PreviousSortColumnValue);
        }
    }
}
