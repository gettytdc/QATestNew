namespace BluePrism.Api.CommonTestClasses
{
    using BluePrism.Api.Domain.PagingTokens;
    using BpLibAdapters.Mappers.FilterMappers;
    using Domain;
    using Domain.Filters;
    using FluentAssertions;
    using Func;

    public static class ResourceHelper
    {
        public static ResourceParameters GetTestDomainResourceParameters(int? itemsPerPage = null) =>
            new ResourceParameters
            {
                Name = new StringStartsWithFilter("test"),
                GroupName = new StringStartsWithFilter("group"),
                PoolName = new StringStartsWithFilter("pool"),
                ActiveSessionCount = new GreaterThanOrEqualToFilter<int>(0),
                PendingSessionCount = new GreaterThanOrEqualToFilter<int>(0),
                DisplayStatus = new MultiValueFilter<ResourceDisplayStatus>(new[]
                {
                    new EqualsFilter<ResourceDisplayStatus>(ResourceDisplayStatus.Working),
                    new EqualsFilter<ResourceDisplayStatus>(ResourceDisplayStatus.Idle),
                    new EqualsFilter<ResourceDisplayStatus>(ResourceDisplayStatus.Warning),
                    new EqualsFilter<ResourceDisplayStatus>(ResourceDisplayStatus.Offline),
                    new EqualsFilter<ResourceDisplayStatus>(ResourceDisplayStatus.Missing),
                    new EqualsFilter<ResourceDisplayStatus>(ResourceDisplayStatus.LoggedOut),
                    new EqualsFilter<ResourceDisplayStatus>(ResourceDisplayStatus.Private),
                }),
                ItemsPerPage = itemsPerPage ?? 10,
            };

        public static ResourceParameters GetTestDomainNullFiltersResourceParameters(ResourceSortBy? sortBy = null, int? itemsPerPage = null) =>
            new ResourceParameters
            {
                SortBy = sortBy ?? ResourceSortBy.NameAscending,
                Name = new NullFilter<string>(),
                GroupName = new NullFilter<string>(),
                PoolName = new NullFilter<string>(),
                ActiveSessionCount = new NullFilter<int>(),
                PendingSessionCount = new NullFilter<int>(),
                DisplayStatus = new NullFilter<ResourceDisplayStatus>(),
                ItemsPerPage = itemsPerPage ?? 10,
                PagingToken = OptionHelper.Some(new PagingToken<string>()),
            };

        public static void ValidateParametersModelsAreEqual(Server.Domain.Models.ResourceParameters bluePrismParameters,
                                                            ResourceParameters domainParameters)
        {
            bluePrismParameters.Name.Should().Be(domainParameters.Name.ToBluePrismObject());
            bluePrismParameters.GroupName.Should().Be(domainParameters.GroupName.ToBluePrismObject());
            bluePrismParameters.PoolName.Should().Be(domainParameters.PoolName.ToBluePrismObject());
            bluePrismParameters.ActiveSessionCount.Should().Be(domainParameters.ActiveSessionCount.ToBluePrismObject());
            bluePrismParameters.PendingSessionCount.Should().Be(domainParameters.PendingSessionCount.ToBluePrismObject());
            bluePrismParameters.DisplayStatus.Should()
                .Be(domainParameters.DisplayStatus.ToBluePrismObject(x => (Server.Domain.Models.ResourceDisplayStatus)x));
            bluePrismParameters.ItemsPerPage.Should().Be(domainParameters.ItemsPerPage);
        }
    }
}
