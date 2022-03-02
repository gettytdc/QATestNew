using BluePrism.Server.Domain.Models.Pagination;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.Server.Domain.Models.Standard.UnitTests
{
    [TestFixture]
    public class WorkQueueItemParametersOrderByTests
    {
        [TestCase("asc","loaded", "ident", "loaded asc, ident asc")]
        [TestCase("desc", "loaded", "ident", "loaded desc, ident desc")]
        public void GetOrderByClause_ShouldReturnExpectedClause_WhenProvidedSortDirectionAndColumnNames(string sortDirection, string column1, string column2, string expectedResult)
        {
            var result = OrderBySqlGenerator.GetOrderByClause(sortDirection, column1, column2);
            result.Should().Be(expectedResult);
        }
    }
}
