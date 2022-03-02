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
    public class RangeFilterMapperTests : UnitTestBase<RangeFilterMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenCorrectFilterSupplied() =>
            ClassUnderTest.CanMap(new RangeFilter<string>("", "")).Should().BeTrue();

        [TestCaseSource(nameof(InvalidFilters))]
        public void CanMap_ShouldReturnFalse_WhenInvalidFilterSupplied(Filter<string> filter) =>
            ClassUnderTest.CanMap(filter).Should().BeFalse();

        [Test]
        public void Map_ShouldProduceCorrectDataFilter_WhenSuccessful()
        {
            const string lessThanOrEqualTo = "TestMax";
            const string greaterThanOrEqualTo = "TestMin";
            var result = ClassUnderTest.Map(new RangeFilter<string>(greaterThanOrEqualTo, lessThanOrEqualTo));

            result.Should().BeOfType<RangeDataFilter<string>>();
            ((RangeDataFilter<string>)result).GreaterThanOrEqualTo.Should().Be(greaterThanOrEqualTo);
            ((RangeDataFilter<string>)result).LessThanOrEqualTo.Should().Be(lessThanOrEqualTo);
        }

        [Test]
        public void Map_WithValueConverter_ShouldProduceCorrectDataFilter_WhenSuccessful()
        {
            DateTime TestValueConverter(string x) => DateTime.Parse(x);
            var expectedMinResult = TestValueConverter(DateTime.MinValue.ToString(CultureInfo.CurrentCulture));
            var expectedMaxResult = TestValueConverter(DateTime.MaxValue.ToString(CultureInfo.CurrentCulture));
            var result = ClassUnderTest.Map(
                new RangeFilter<string>(DateTime.MinValue.ToString(CultureInfo.CurrentCulture), DateTime.MaxValue.ToString(CultureInfo.CurrentCulture)),
                TestValueConverter);

            result.Should().BeOfType<RangeDataFilter<DateTime>>();
            ((RangeDataFilter<DateTime>)result).GreaterThanOrEqualTo.Should().Be(expectedMinResult);
            ((RangeDataFilter<DateTime>)result).LessThanOrEqualTo.Should().Be(expectedMaxResult);
        }

        [TestCaseSource(nameof(InvalidFilters))]
        public void Map_ShouldThrowArgumentException_WhenInvalidFilterSupplied(Filter<string> filter)
        {
            Action act = () => ClassUnderTest.Map(filter);

            act.ShouldThrow<ArgumentException>();
        }

        private static IEnumerable<TestCaseData> InvalidFilters() =>
            new Filter<string>[]
            {
                new NullFilter<string>(),
                new GreaterThanOrEqualToFilter<string>(""),
                new LessThanOrEqualToFilter<string>(""),
                new EqualsFilter<string>(""),
                new StringContainsFilter(""),
                new StringStartsWithFilter(""),
                new MultiValueFilter<string>(Array.Empty<Filter<string>>())
            }
            .ToTestCaseData();
    }
}
