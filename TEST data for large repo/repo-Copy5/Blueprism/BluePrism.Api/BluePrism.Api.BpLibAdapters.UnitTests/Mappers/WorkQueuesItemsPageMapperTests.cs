namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using BpLibAdapters.Mappers;
    using Domain;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Server.Domain.Models.Pagination;

    [TestFixture(Category = "Unit Test")]
    public class WorkQueuesItemsPageMapperTests
    {
        private const WorkQueueSortByProperty WorkQueueSortByProperty = Domain.WorkQueueSortByProperty.AverageWorkedTimeAsc;

        [Test]
        public void ToItemsPage_WithEmptyTestItems_ReturnsNonePaginationToken()
        {
            var parameters = new WorkQueueParameters { ItemsPerPage = 10 };

            var workQueueItemsPage = new List<WorkQueue>().ToItemsPage(parameters);
            workQueueItemsPage.PagingToken.Should().BeAssignableTo<None<string>>();
        }

        [Test]
        public void ToItemsPage_TestItems_ReturnsCorrectPaginationToken()
        {
            var parameters = new WorkQueueParameters { ItemsPerPage = 10, SortBy = WorkQueueSortByProperty };

            var workQueues = GetWorkQueues();

            var lastItem = workQueues.Last();
            var lastItemProperty = lastItem.AverageWorkTime;

            var pagingToken = new PagingToken<int>
            {
                DataType = lastItemProperty.GetTypeName(),
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItemProperty),
                PreviousIdValue = lastItem.Ident,
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };

            var workQueueItemsPage = workQueues.ToItemsPage(parameters);

            ((Some<string>)workQueueItemsPage.PagingToken).Value.Should().Be(pagingToken.ToString());
        }

        [Test]
        public void ToItemsPage_ReturnsEmptyPagingToken_WhenPageSizeIsGreaterThanWorkQueues()
        {
            var workQueues = GetWorkQueues();
            var parameters = new WorkQueueParameters { ItemsPerPage = workQueues.Count + 1 , SortBy = WorkQueueSortByProperty };
            var workQueueItemsPage = workQueues.ToItemsPage(parameters);

            workQueueItemsPage.PagingToken.Should().BeAssignableTo<None<string>>();
        }
        
        private static IReadOnlyList<WorkQueue> GetWorkQueues() =>
            new Fixture()
                .Build<WorkQueue>()
                .With(x => x.Name, "Some name")
                .With(x => x.KeyField, "Some field")
                .CreateMany(10)
                .ToList();
    }
}
