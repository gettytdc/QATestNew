namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers.FilterMappers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using BpLibAdapters.Mappers.FilterMappers;
    using CommonTestClasses.Extensions;
    using Domain.Filters;
    using FluentAssertions;
    using NUnit.Framework;
    using Server.Domain.Models.DataFilters;
    using Utilities.Testing;

    [TestFixture]
    public class MultiValueFilterMapperTests : UnitTestBase<MultiValueFilterMapper>
    {
        public override void OneTimeSetup()
        {
            var filterMappers = new IFilterMapper[]
            {
                new EqualsFilterMapper(),
                new StringContainsFilterMapper(),
                new StringStartsWithFilterMapper(),
                new GreaterThanOrEqualToFilterMapper(),
                new LessThanOrEqualToFilterMapper(),
                new RangeFilterMapper(),
                new NullFilterMapper(),
                new MultiValueFilterMapper()
            };
            FilterMapper.SetFilterMappers(filterMappers);
        }

        [Test]
        public void CanMap_ShouldReturnTrue_WhenCorrectFilterSupplied() =>
            ClassUnderTest.CanMap(new MultiValueFilter<string>(Array.Empty<Filter<string>>())).Should().BeTrue();

        [TestCaseSource(nameof(InvalidFilters))]
        public void CanMap_ShouldReturnFalse_WhenInvalidFilterSupplied(Filter<string> filter) =>
            ClassUnderTest.CanMap(filter).Should().BeFalse();

        [Test]
        public void Map_ShouldReturnMultiValueDataFilter_WhenCalledWithMultiValueFilter() =>
            ClassUnderTest.Map(new MultiValueFilter<string>(new[] {new EqualsFilter<string>("")}))
                .Should()
                .BeOfType<MultiValueDataFilter<string>>();

        [Test]
        public void Map_ShouldReturnMultiValueDataFilter_WhenCalledWithEmptyMultiValueFilter() =>
            ClassUnderTest.Map(new MultiValueFilter<string>(Array.Empty<Filter<string>>()))
                .Should()
                .BeOfType<MultiValueDataFilter<string>>();

        [TestCaseSource(nameof(InvalidFilters))]
        public void Map_ShouldThrowException_WhenInvalidFilterSupplied(Filter<string> filter)
        {
            Action act = () => ClassUnderTest.Map(filter);

            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Map_WithValueConverter_ShouldReturnMultiValueDataFilter_WhenCalledWithMultiValueFilter()
        {
            DateTime TestValueConverter(string x) => DateTime.Parse(x, CultureInfo.InvariantCulture);
            var filter = new MultiValueFilter<string>(new []{ new EqualsFilter<string>("0001-01-01T12:00:00") });
            var result = ClassUnderTest.Map(filter, TestValueConverter);

            result.Should().BeOfType<MultiValueDataFilter<DateTime>>();
        }

        [Test]
        public void Map_WithValueConverter_ShouldReturnMultiValueDataFilter_WhenCalledWithEmptyMultiValueFilter()
        {
            DateTime TestValueConverter(string x) => DateTime.Parse(x, CultureInfo.InvariantCulture);
            var filter = new MultiValueFilter<string>(Array.Empty<Filter<string>>());
            var result = ClassUnderTest.Map(filter, TestValueConverter);

            result.Should().BeOfType<MultiValueDataFilter<DateTime>>();
        }

        [TestCaseSource(nameof(InvalidFilters))]
        public void Map_WithValueConverter_ShouldThrowException_WhenInvalidFilterSupplied(Filter<string> filter)
        {
            Action act = () => ClassUnderTest.Map(filter, int.Parse);

            act.ShouldThrow<ArgumentException>();
        }

        private static IEnumerable<TestCaseData> InvalidFilters() =>
            new Filter<string>[]
                {
                    new NullFilter<string>(),
                    new GreaterThanOrEqualToFilter<string>(""),
                    new LessThanOrEqualToFilter<string>(""),
                    new RangeFilter<string>("", ""),
                    new StringContainsFilter(""),
                    new StringStartsWithFilter(""),
                    new EqualsFilter<string>("")
                }
                .ToTestCaseData();
    }
}
