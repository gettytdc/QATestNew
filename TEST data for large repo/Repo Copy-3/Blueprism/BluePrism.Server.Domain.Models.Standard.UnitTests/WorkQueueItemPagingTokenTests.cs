using BluePrism.Server.Domain.Models.Pagination;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.Server.Domain.Models.Standard.UnitTests
{
    [TestFixture]
    public class WorkQueueItemPagingTokenTests
    {
        [Test]
        public void WorkQueueItemPagingToken_ShouldReturnCorrectIdColumnName() => WorkQueueItemPagingToken.IdColumnName.Should().Be("ident");
    }
}
