namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers.FilterMappers
{
    using System;
    using System.Collections.Generic;
    using BpLibAdapters.Mappers.FilterMappers;
    using CommonTestClasses.Extensions;
    using Domain.Filters;
    using FluentAssertions;
    using NUnit.Framework;
    using Server.Domain.Models.DataFilters;
    using Utilities.Testing;

    [TestFixture]
    public class StringStartsWithFilterMapperTests : UnitTestBase<StringStartsWithFilterMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenCorrectFilterSupplied() =>
            ClassUnderTest.CanMap(new StringStartsWithFilter("")).Should().BeTrue();

        [TestCaseSource(nameof(InvalidFilters))]
        public void CanMap_ShouldReturnFalse_WhenInvalidFilterSupplied(Filter<string> filter) =>
            ClassUnderTest.CanMap(filter).Should().BeFalse();

        [Test]
        public void Map_ShouldProduceCorrectDataFilter_WhenSuccessful()
        {
            const string startsWithValue = "test";
            var result = ClassUnderTest.Map(new StringStartsWithFilter(startsWithValue));

            result.Should().BeOfType<StartsWithDataFilter>();
            ((StartsWithDataFilter)result).StartsWith.Should().Be(startsWithValue);
        }

        [Test]
        public void Map_WithValueConverter_ShouldProduceCorrectDataFilter_WhenSuccessful()
        {
            const string startsWithValue = "test";
            var result = ClassUnderTest.Map(new StringStartsWithFilter(startsWithValue), x => x + "1");

            result.Should().BeOfType<StartsWithDataFilter>();
            ((StartsWithDataFilter)result).StartsWith.Should().Be(startsWithValue);
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
                new RangeFilter<string>("", ""),
                new EqualsFilter<string>(""),
                new StringContainsFilter(""),
                new MultiValueFilter<string>(Array.Empty<Filter<string>>())
            }.ToTestCaseData();
    }
}
