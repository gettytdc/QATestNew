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
    public class NullFilterMapperTests : UnitTestBase<NullFilterMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenCorrectFilterSupplied() =>
            ClassUnderTest.CanMap(new NullFilter<string>()).Should().BeTrue();

        [TestCaseSource(nameof(InvalidFilters))]
        public void CanMap_ShouldReturnFalse_WhenInvalidFilterSupplied(Filter<string> filter) =>
            ClassUnderTest.CanMap(filter).Should().BeFalse();

        [Test]
        public void Map_ShouldProduceCorrectDataFilter_WhenSuccessful() =>
            ClassUnderTest.Map(new NullFilter<string>()).Should().BeOfType<NullDataFilter<string>>();

        [Test]
        public void Map_WithValueConverter_ProduceCorrectDataFilter_WhenSuccessful() =>
            ClassUnderTest.Map(new NullFilter<string>(), int.Parse).Should().BeOfType<NullDataFilter<int>>();

        [TestCaseSource(nameof(InvalidFilters))]
        public void Map_ShouldThrowArgumentException_WhenInvalidFilterSupplied(Filter<string> filter)
        {
            Action act = () => ClassUnderTest.Map(filter);
            act.ShouldThrow<ArgumentException>();
        }

        private static IEnumerable<TestCaseData> InvalidFilters() =>
            new Filter<string>[]
            {
                new EqualsFilter<string>(""),
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
