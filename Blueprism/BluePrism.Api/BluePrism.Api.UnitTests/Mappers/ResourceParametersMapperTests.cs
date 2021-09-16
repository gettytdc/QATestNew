namespace BluePrism.Api.UnitTests.Mappers
{
    using Api.Mappers;
    using Api.Mappers.FilterMappers;
    using Domain.Filters;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;
    using static Func.OptionHelper;
    using ResourceDisplayStatus = Domain.ResourceDisplayStatus;
    using ResourceParameters = Domain.ResourceParameters;

    [TestFixture]
    public class ResourceParametersMapperTests
    {
        [SetUp]
        public void SetUp() =>
            FilterModelMapper.SetFilterModelMappers(new IFilterModelMapper[]
            {
                new EqualFilterModelMapper(),
                new StartsWithStringFilterModelMapper(),
                new GreaterThanOrEqualToFilterModelMapper(),
                new NullFilterModelMapper()
            });

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedData_WhenCalled()
        {
            var modelResourceParameters = new Models.ResourceParameters
            {
                Name = new StartsWithStringFilterModel { Strtw = "resource" },
                GroupName = new StartsWithStringFilterModel { Strtw = "group" },
                PoolName = new StartsWithStringFilterModel { Strtw = "pool" },
                ActiveSessionCount = new RangeFilterModel<int?> { Gte = 0 },
                PendingSessionCount = new RangeFilterModel<int?> { Gte = 0 },
                DisplayStatus = new CommaDelimitedCollection<Models.ResourceDisplayStatus>(new[]
                {
                    Models.ResourceDisplayStatus.Working,
                    Models.ResourceDisplayStatus.Offline
                }),
                ItemsPerPage = 10
            };
            var domainResourceParameters = new ResourceParameters
            {
                Name = new StringStartsWithFilter("test"),
                GroupName = new StringStartsWithFilter("group"),
                PoolName = new StringStartsWithFilter("pool"),
                ActiveSessionCount = new GreaterThanOrEqualToFilter<int>(0),
                PendingSessionCount = new GreaterThanOrEqualToFilter<int>(0),
                DisplayStatus = new MultiValueFilter<ResourceDisplayStatus>(new Filter<ResourceDisplayStatus>[]
                {
                    new EqualsFilter<ResourceDisplayStatus>(ResourceDisplayStatus.Working),
                    new EqualsFilter<ResourceDisplayStatus>(ResourceDisplayStatus.Offline)
                }),
                ItemsPerPage = 10,
                PagingToken = None<PagingToken<string>>()
            };
            var result = modelResourceParameters.ToDomainObject();
            domainResourceParameters.ShouldBeEquivalentTo(result);
        }
    }
}
