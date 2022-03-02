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
    public class EqualsFilterMapperTests : UnitTestBase<EqualsFilterMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenCorrectFilterSupplied() =>
            ClassUnderTest.CanMap(new EqualsFilter<string>("")).Should().BeTrue();

        [TestCaseSource(nameof(InvalidFilters))]
        public void CanMap_ShouldReturnFalse_WhenInvalidFilterSupplied(Filter<string> filter) =>
            ClassUnderTest.CanMap(filter).Should().BeFalse();

        [Test]
        public void Map_ShouldProduceCorrectDataFilter_WhenSuccessful()
        {
            const string equalValue = "test";
            var result = ClassUnderTest.Map(new EqualsFilter<string>(equalValue));

            result.Should().BeOfType<EqualsDataFilter<string>>();
            ((EqualsDataFilter<string>)result).EqualTo.Should().Be(equalValue);
        }

        [Test]
        public void Map_WithValueConverter_ShouldProduceCorrectDataFilter_WhenSuccessful()
        {
            DateTime TestValueConverter(string x) => DateTime.Parse(x);
            var expectedResult = TestValueConverter("0002-01-01T00:00:00");
            var result = ClassUnderTest.Map(new EqualsFilter<string>("0002-01-01T00:00:00"), TestValueConverter);

            result.Should().BeOfType<EqualsDataFilter<DateTime>>();
            ((EqualsDataFilter<DateTime>)result).EqualTo.Should().Be(expectedResult);
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
                new StringContainsFilter(""),
                new StringStartsWithFilter(""),
                new MultiValueFilter<string>(Array.Empty<Filter<string>>())
            }
            .ToTestCaseData();
    }
}
