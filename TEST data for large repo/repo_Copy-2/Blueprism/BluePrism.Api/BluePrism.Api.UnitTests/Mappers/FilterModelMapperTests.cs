namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using Api.Mappers.FilterMappers;
    using Domain.Filters;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class FilterModelMapperTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var filterMappers = new IFilterModelMapper[]
            {
                new EqualFilterModelMapper(),
                new GreaterThanOrEqualToFilterModelMapper(),
                new LessThanOrEqualToFilterModelMapper(),
                new RangeFilterModelMapper(),
                new StringFilterModelMapper(),
                new NullFilterModelMapper(),
            };
            FilterMapper.SetFilterModelMappers(filterMappers);
        }

        [Test]
        public void FilterMapper_WithEqualsFilter_ProducesCorrectDataFilter()
        {
            var result = new RangeFilterModel<string>
                {
                    Eq = "Test",
                    Gte = null,
                    Lte = null,
                }
                .ToDomain();

            ((EqualsFilter<string>)result).EqualTo.Should().Be("Test");
        }

        [Test]
        public void FilterMapper_WithGreaterThanOrEqualToFilter_ProducesCorrectDataFilter()
        {
            var result = new RangeFilterModel<string>
                {
                    Eq = null,
                    Gte = "Test",
                    Lte = null,
                }
                .ToDomain();

            ((GreaterThanOrEqualToFilter<string>)result).GreaterThanOrEqualTo.Should().Be("Test");
        }

        [Test]
        public void FilterMapper_WithLessThanOrEqualToFilter_ProducesCorrectDataFilter()
        {
            var result = new RangeFilterModel<string>
                {
                    Eq = null,
                    Gte = null,
                    Lte = "Test",
                }
                .ToDomain();

            ((LessThanOrEqualToFilter<string>)result).LessThanOrEqualTo.Should().Be("Test");
        }

        [Test]
        public void FilterMapper_WithRangeFilter_ProducesCorrectDataFilter()
        {
            var result = new RangeFilterModel<string>
                {
                    Eq = null,
                    Gte = "TestMin",
                    Lte = "TestMax",
                }
                .ToDomain();

            ((RangeFilter<string>)result).GreaterThanOrEqualTo.Should().Be("TestMin");
            ((RangeFilter<string>)result).LessThanOrEqualTo.Should().Be("TestMax");
        }

        [Test]
        public void FilterMapper_WithNullFilter_ProducesCorrectDataFilter()
        {
            var result = new RangeFilterModel<string>
                {
                    Eq = null,
                    Gte = null,
                    Lte = null,
                }
                .ToDomain();

            result.Should().BeOfType<NullFilter<string>>();
        }

        [Test]
        public void FilterMapper_WithInvalidFilter_ThrowsException()
        {
            Action test = () =>
                new RangeFilterModel<string> {Eq = "Test", Gte = "Test"}
                    .ToDomain();

            test.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void FilterMapper_WithStringFilter_MapsCorrectly()
        {
            var result = new StringFilterModel {Ctn = "test"}.ToDomain();

            result.Should().BeOfType<StringContainsFilter>();
            (result as StringContainsFilter).Contains.Should().Be("test");
        }
    }
}
