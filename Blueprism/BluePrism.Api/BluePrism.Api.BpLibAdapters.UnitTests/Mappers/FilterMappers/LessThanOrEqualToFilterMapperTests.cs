namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers.FilterMappers
{
    using System;
    using System.Collections.Generic;
    using BpLibAdapters.Mappers.FilterMappers;
    using CommonTestClasses.Extensions;
    using Domain.Filters;
    using FluentAssertions;
    using FluentAssertions.Common;
    using NUnit.Framework;
    using Server.Domain.Models.DataFilters;
    using Utilities.Testing;

    [TestFixture]
    public class LessThanOrEqualToFilterMapperTests : UnitTestBase<LessThanOrEqualToFilterMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenCorrectFilterSupplied() =>
            ClassUnderTest.CanMap(new LessThanOrEqualToFilter<string>("")).Should().BeTrue();

        [TestCaseSource(nameof(InvalidFilters))]
        public void CanMap_ShouldReturnFalse_WhenInvalidFilterSupplied(Filter<string> filter) =>
            ClassUnderTest.CanMap(filter).Should().BeFalse();

        [Test]
        public void Map_ShouldProduceCorrectDataFilter_WhenSuccessful()
        {
            const string lessThanOrEqualTo = "test";
            var result = ClassUnderTest.Map(new LessThanOrEqualToFilter<string>(lessThanOrEqualTo));

            result.Should().BeOfType<LessThanOrEqualToDataFilter<string>>();
            ((LessThanOrEqualToDataFilter<string>)result).LessThanOrEqualTo.Should().Be(lessThanOrEqualTo);
        }

        [Test]
        public void Map_WithValueConverter_ShouldProduceCorrectDataFilter_WhenSuccessful()
        {
            DateTimeOffset TestValueConverter(DateTime x) => x.ToDateTimeOffset();
            var expectedResult = TestValueConverter(DateTime.MinValue);
            var result = ClassUnderTest.Map(new LessThanOrEqualToFilter<DateTime>(DateTime.MinValue), TestValueConverter);

            result.Should().BeOfType<LessThanOrEqualToDataFilter<DateTimeOffset>>();
            ((LessThanOrEqualToDataFilter<DateTimeOffset>)result).LessThanOrEqualTo.Should().Be(expectedResult);
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
                new RangeFilter<string>("", ""),
                new GreaterThanOrEqualToFilter<string>(""),
                new EqualsFilter<string>(""),
                new StringContainsFilter(""),
                new StringStartsWithFilter(""),
                new MultiValueFilter<string>(Array.Empty<Filter<string>>())
            }.ToTestCaseData();
    }
}
