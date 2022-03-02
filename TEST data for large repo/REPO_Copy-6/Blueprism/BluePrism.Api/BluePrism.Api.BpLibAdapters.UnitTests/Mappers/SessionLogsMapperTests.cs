namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using Domain;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class SessionLogsMapperTests
    {
        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedData()
        {
            var clsSessionLogEntry = SessionLogsHelper.GetTestBluePrismLogEntries(1).First();
            var domainSessionLogItem = clsSessionLogEntry.ToDomainObject();
            SessionLogsHelper.ValidateModelsAreEqual(clsSessionLogEntry, domainSessionLogItem);
        }

         [Test]
        public void ToItemsPage_WithEmptySessionLogItems_ReturnsNonePaginationToken()
        {
            var sessionLogItems = new List<SessionLogItem>();
            var parameters = new SessionLogsParameters { ItemsPerPage = 10 };

            var sessionLogsPage = sessionLogItems.ToItemsPage(parameters);
            sessionLogsPage.PagingToken.Should().BeAssignableTo<None<string>>();
        }

        [Test]
        public void ToItemsPage_SessionLogItems_ReturnsCorrectPaginationToken()
        {
            var items = SessionLogsHelper.GetTestBluePrismLogEntries(3, DateTimeOffset.UtcNow)
                .Select(x => x.ToDomainObject())
                .ToList();

            var parameters = new SessionLogsParameters { ItemsPerPage = 2 };

            var lastItem = items.OrderBy(x => x.LogId).Last();

            var pagingToken = new PagingToken<long>
            {
                DataType = lastItem.ResourceStartTime.GetTypeName(),
                PreviousIdValue = lastItem.LogId,
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };

            var sessionLogsPage = items.ToItemsPage(parameters);

            ((Some<string>)sessionLogsPage.PagingToken).Value.Should().Be(pagingToken.ToString());
        }

        [Test]
        public void ToItemsPage_ReturnsDifferentPaginationToken_WhenParametersHaveChanged()
        {
            var items = SessionLogsHelper.GetTestDomainSessionLogItems(3, DateTimeOffset.UtcNow).ToList();
            var parameters = new SessionLogsParameters { ItemsPerPage = 2 };

            var lastItem = items.OrderBy(x => x.LogId).Last();

            var pagingToken = new PagingToken<long>
            {
                DataType = lastItem.ResourceStartTime.GetTypeName(),
                PreviousIdValue = lastItem.LogId,
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };

            parameters.ItemsPerPage = 1;

            var sessionLogsPage = items.ToItemsPage(parameters);

            ((Some<string>)sessionLogsPage.PagingToken).Value.Should().NotBe(pagingToken.ToString());
        }
    }
}
