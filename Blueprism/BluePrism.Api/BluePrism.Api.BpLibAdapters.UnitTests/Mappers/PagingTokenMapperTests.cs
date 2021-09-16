namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using Domain;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Server.Domain.Models.Pagination;

    [TestFixture(Category = "Unit Test")]
    public class PagingTokenMapperTests
    {
        [Test]
        public void ToBluePrismObject_WithValidPagingTokenModel_ReturnsCorrectlyMappedResult()
        {
            var pagingToken = new PagingToken<int>()
            {
                PreviousIdValue = 1,
                DataType = "data-type",
                PreviousSortColumnValue = "some value"
            };
            var pagingTokenOption = OptionHelper.Some(pagingToken);
            var result = PagingTokenMapper<WorkQueuePagingToken, int>.ToBluePrismPagingToken(pagingTokenOption);

            var mappedPagingToken = ((Some<WorkQueuePagingToken>)result).Value;
            mappedPagingToken.PreviousIdValue.Should().Be(pagingToken.PreviousIdValue);
            mappedPagingToken.DataType.Should().Be(pagingToken.DataType);
            mappedPagingToken.PreviousSortColumnValue.Should().Be(pagingToken.PreviousSortColumnValue);

        }

        [Test]
        public void ToBluePrismObject_WithNoPagingTokenModel_ReturnsNoPagingTokenModel()
        {
            var result = PagingTokenMapper<WorkQueuePagingToken, int>.ToBluePrismPagingToken(OptionHelper.None<PagingToken<int>>());

            result.Should().BeAssignableTo<None<WorkQueuePagingToken>>();
        }
    }
}
