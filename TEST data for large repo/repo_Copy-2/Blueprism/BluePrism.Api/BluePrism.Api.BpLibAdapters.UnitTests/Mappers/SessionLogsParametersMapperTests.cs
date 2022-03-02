namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using Domain;
    using Domain.PagingTokens;
    using FluentAssertions;
    using NUnit.Framework;

    using static Func.OptionHelper;

    [TestFixture(Category = "Unit Test")]
    public class SessionLogsParametersMapperTests
    {

        [Test]
        public void ToBluePrismObject_WithTestDomainSessionLogsParameters_ReturnsCorrectlyMappedResult()
        {
            var domainSessionLogsParameters = new SessionLogsParameters { ItemsPerPage = 10, PagingToken = None<PagingToken<long>>()};
            var bluePrismSessionLogsParameters = domainSessionLogsParameters.ToBluePrismObject();
            bluePrismSessionLogsParameters.ItemsPerPage.Should().Be(domainSessionLogsParameters.ItemsPerPage);
            bluePrismSessionLogsParameters.PagingToken.Should().Be(None<Server.Domain.Models.Pagination.SessionLogsPagingToken>());
        }
    }
}
