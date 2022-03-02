namespace BluePrism.Api.UnitTests.Mappers.FilterMappers
{
    using System;
    using System.Collections.Generic;
    using Api.Mappers.FilterMappers;
    using CommonTestClasses.Extensions;
    using Domain.Filters;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class FilterModelMapperTests
    {
        private const string TestValue = "Test";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var filterMappers = new IFilterModelMapper[]
            {
                new EqualFilterModelMapper(),
                new GreaterThanOrEqualToFilterModelMapper(),
                new LessThanOrEqualToFilterModelMapper(),
                new RangeFilterModelMapper(),
                new StartsWithStringFilterModelMapper(),
                new StartsWithOrContainsStringFilterModelMapper(),
                new NullFilterModelMapper(),
            };
            FilterModelMapper.SetFilterModelMappers(filterMappers);
        }
        
        [TestCaseSource(nameof(ValidFilters))]
        public void CanMap_ShouldReturnTrue_WhenValidFilterSupplied(BasicFilterModel<string> filter) =>
            filter.CanMap().Should().BeTrue();

        [Test]
        public void ToDomain_ShouldReturnEqualsFilter_WhenOnlyEqualPropertyIsPopulated()
        {
            var result = new StartsWithOrContainsStringFilterModel {Eq = TestValue}.ToDomain();

            result.Should().BeOfType<EqualsFilter<string>>();
            ((EqualsFilter<string>)result).EqualTo.Should().Be(TestValue);
        }

        [Test]
        public void ToDomain_ShouldReturnGreaterThanOrEqualToFilter_WhenOnlyGreaterThanEqualToPropertyIsPopulated()
        {
            var result = new RangeFilterModel<string> {Gte = TestValue}.ToDomain();

            result.Should().BeOfType<GreaterThanOrEqualToFilter<string>>();
            ((GreaterThanOrEqualToFilter<string>)result).GreaterThanOrEqualTo.Should().Be(TestValue);
        }

        [Test]
        public void ToDomain_ShouldReturnLessThanOrEqualToFilter_WhenOnlyLessThanEqualToPropertyIsPopulated()
        {
            var result = new RangeFilterModel<string> {Lte = TestValue}.ToDomain();

            result.Should().BeOfType<LessThanOrEqualToFilter<string>>();
            ((LessThanOrEqualToFilter<string>)result).LessThanOrEqualTo.Should().Be(TestValue);
        }

        [Test]
        public void ToDomain_ShouldReturnRangeFilter_WhenOnlyGreaterThanEqualToAndLessThanEqualToPropertyIsPopulated()
        {
            const string gte = "TestMin";
            const string lte = "TestMax";

            var result = new RangeFilterModel<string>
                {
                    Gte = gte,
                    Lte = lte,
                }
                .ToDomain();

            result.Should().BeOfType<RangeFilter<string>>();
            ((RangeFilter<string>)result).GreaterThanOrEqualTo.Should().Be(gte);
            ((RangeFilter<string>)result).LessThanOrEqualTo.Should().Be(lte);
        }

        [Test]
        public void ToDomain_ShouldReturnNullFilter_WhenNoPropertiesIsPopulated() =>
            new StartsWithOrContainsStringFilterModel().ToDomain().Should().BeOfType<NullFilter<string>>();

        [Test]
        public void ToDomain_ShouldReturnStringContainsFilter_WhenOnlyContainsPropertyIsPopulated()
        {
            var result = new StartsWithOrContainsStringFilterModel {Ctn = TestValue}.ToDomain();

            result.Should().BeOfType<StringContainsFilter>();
            ((StringContainsFilter)result).Contains.Should().Be(TestValue);
        }

        [Test]
        public void ToDomain_ShouldReturnStringStartsWithFilter_WhenOnlyStartsWithPropertyIsPopulated()
        {
            var result = new StartsWithOrContainsStringFilterModel {Strtw = TestValue}.ToDomain();

            result.Should().BeOfType<StringStartsWithFilter>();
            ((StringStartsWithFilter)result).StartsWith.Should().Be(TestValue);
        }

        [TestCaseSource(nameof(InvalidFilters))]
        public void ToDomain_ShouldThrowArgumentException_WhenInvalidFilterSupplied(BasicFilterModel<string> filter)
        {
            Action test = () => filter.ToDomain();

            test.ShouldThrow<ArgumentException>();
        }

        private static IEnumerable<TestCaseData> ValidFilters() =>
            new []
            {
                new BasicFilterModel<string> {Eq = TestValue},
                new RangeFilterModel<string> {Gte = TestValue},
                new RangeFilterModel<string> {Lte = TestValue},
                new RangeFilterModel<string> {Gte = TestValue, Lte = TestValue},
                new StartsWithStringFilterModel {Strtw = TestValue},
                new StartsWithOrContainsStringFilterModel {Ctn = TestValue}
            }
            .ToTestCaseData();

        private static IEnumerable<TestCaseData> InvalidFilters() =>
            new BasicFilterModel<string>[]
            {
                new StartsWithOrContainsStringFilterModel {Eq = TestValue, Gte = TestValue},
                new StartsWithOrContainsStringFilterModel {Eq = TestValue, Lte = TestValue},
                new StartsWithOrContainsStringFilterModel {Eq = TestValue, Strtw = TestValue},
                new StartsWithOrContainsStringFilterModel {Eq = TestValue, Ctn = TestValue},
                new StartsWithOrContainsStringFilterModel {Ctn = TestValue, Gte = TestValue},
                new StartsWithOrContainsStringFilterModel {Ctn = TestValue, Lte = TestValue},
                new StartsWithOrContainsStringFilterModel {Strtw = TestValue, Gte = TestValue},
                new StartsWithOrContainsStringFilterModel {Strtw = TestValue, Lte = TestValue},
                new StartsWithOrContainsStringFilterModel {Strtw = TestValue, Ctn = TestValue}
            }
            .ToTestCaseData();
    }
}
