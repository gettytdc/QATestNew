namespace BluePrism.Api.UnitTests.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.Mappers;
    using Domain;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using static Func.OptionHelper;

    [TestFixture]
    public class ItemsPageMapperTests
    {
        [Test]
        public void ToModelItemsPage_ShouldReturnCorrectlyMappedData_WhenCorrectItemsPassed()
        {
            var domainItemsPage = new ItemsPage<DomainTestItem>
            {
                Items = new[] { new DomainTestItem { Name = "Test" } },
                PagingToken = None<string>()
            };
            var modelItemsPage = domainItemsPage.ToModelItemsPage(x => new ModelTestItem { Name = x.Name });
            modelItemsPage.Items.First().Name.ShouldBeEquivalentTo(domainItemsPage.Items.First().Name);
            modelItemsPage.PagingToken.ShouldBeEquivalentTo(domainItemsPage.PagingToken is Some<string> t ? t.Value : null);
        }

        [Test]
        public void ToModelItemsPage_ShouldReturnEmptyItems_WhenEmptyItemsPassed()
        {
            var domainItemsPage = new ItemsPage<DomainTestItem> { Items = new List<DomainTestItem>() };
            var modelItemsPage = domainItemsPage.ToModelItemsPage(x => new ModelTestItem { Name = x.Name });
            modelItemsPage.Items.Should().BeEmpty();
        }


        internal class DomainTestItem
        {
            internal string Name { get; set; }
        }

        internal class ModelTestItem
        {
            internal string Name { get; set; }
        }
    }
}
