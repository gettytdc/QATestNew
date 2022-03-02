namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers.FilterMappers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using BpLibAdapters.Mappers.FilterMappers;
    using Domain.Filters;
    using FluentAssertions;
    using FluentAssertions.Common;
    using NUnit.Framework;
    using Server.Domain.Models.DataFilters;

    [TestFixture]
    public class FilterMapperTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
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
        public void ToBluePrismObject_ShouldReturnEqualsDataFilter_FromEqualsFilter() =>
            new EqualsFilter<string>("Test").ToBluePrismObject().EqualTo.Should().Be("Test");

        [Test]
        public void ToBluePrismObject_WithValueConverter_ShouldReturnEqualsDataFilter_FromEqualsFilter()
        {
            DateTime TestValueConverter(string x) => DateTime.Parse(x, CultureInfo.InvariantCulture);
            var expectedResult = TestValueConverter("0001-01-01T12:00:00");
            var result = new EqualsFilter<string>("0001-01-01T12:00:00").ToBluePrismObject(TestValueConverter);

            ((EqualsDataFilter<DateTime>)result).EqualTo.Should().Be(expectedResult);
        }

        [Test]
        public void ToBluePrismObject_ShouldReturnContainsDataFilter_FromStringContainsFilter() =>
            new StringContainsFilter("Test").ToBluePrismObject().ContainsValue.Should().Be("Test");

        [Test]
        public void ToBluePrismObject_WithValueConverter_ShouldThrowArgumentException_FromStringContainsFilter()
        {
            Action act = () => new StringContainsFilter("Test").ToBluePrismObject(int.Parse);
            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void ToBluePrismObject_ShouldReturnStartsWithDataFilter_FromStringStartsWithFilter() =>
            new StringStartsWithFilter("Test").ToBluePrismObject().StartsWith.Should().Be("Test");

        [Test]
        public void ToBluePrismObject_WithValueConverter_ShouldThrowArgumentException_FromStringStartsWithFilter()
        {
            Action act = () => new StringStartsWithFilter("Test").ToBluePrismObject(int.Parse);
            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void ToBluePrismObject_ShouldReturnGreaterThanOrEqualToDataFilter_FromGreaterThanOrEqualToFilter() =>
            new GreaterThanOrEqualToFilter<string>("Test").ToBluePrismObject().GreaterThanOrEqualTo.Should().Be("Test");

        [Test]
        public void ToBluePrismObject_WithValueConverter_ShouldReturnGreaterThanOrEqualToDataFilter_FromGreaterThanOrEqualToFilter()
        {
            DateTimeOffset TestValueConverter(DateTime x) => x.ToDateTimeOffset();
            var expectedResult = TestValueConverter(DateTime.MinValue);
            var result = new GreaterThanOrEqualToFilter<DateTime>(DateTime.MinValue).ToBluePrismObject(TestValueConverter);

            ((GreaterThanOrEqualToDataFilter<DateTimeOffset>)result).GreaterThanOrEqualTo.Should().Be(expectedResult);
        }

        [Test]
        public void ToBluePrismObject_ShouldReturnLessThanOrEqualToDataFilter_FromLessThanOrEqualToFilter() =>
            new LessThanOrEqualToFilter<string>("Test").ToBluePrismObject().LessThanOrEqualTo.Should().Be("Test");

        [Test]
        public void ToBluePrismObject_WithValueConverter_ShouldReturnLessThanOrEqualToDataFilter_FromLessThanOrEqualToFilter()
        {
            DateTimeOffset TestValueConverter(DateTime x) => x.ToDateTimeOffset();
            var expectedResult = TestValueConverter(DateTime.MinValue);
            var result = new LessThanOrEqualToFilter<DateTime>(DateTime.MinValue).ToBluePrismObject(TestValueConverter);

            ((LessThanOrEqualToDataFilter<DateTimeOffset>)result).LessThanOrEqualTo.Should().Be(expectedResult);
        }

        [Test]
        public void ToBluePrismObject_ShouldReturnRangeDataFilter_FromRangeFilter()
        {
            const string greaterThanOrEqualTo = "TestMin";
            const string lessThanOrEqualTo = "TestMax";
            var result = new RangeFilter<string>(greaterThanOrEqualTo, lessThanOrEqualTo).ToBluePrismObject();

            result.GreaterThanOrEqualTo.Should().Be(greaterThanOrEqualTo);
            result.LessThanOrEqualTo.Should().Be(lessThanOrEqualTo);
        }

        [Test]
        public void ToBluePrismObject_WithValueConverter_ShouldReturnRangeDataFilter_FromRangeFilter()
        {
            DateTime TestValueConverter(string x) => DateTime.Parse(x);
            var expectedMinResult = TestValueConverter("0001-01-01T12:00:00");
            var expectedMaxResult = TestValueConverter("9999-12-31T11:59:59");
            var result = new RangeFilter<string>("0001-01-01T12:00:00", "9999-12-31T11:59:59").ToBluePrismObject(TestValueConverter);

            ((RangeDataFilter<DateTime>)result).GreaterThanOrEqualTo.Should().Be(expectedMinResult);
            ((RangeDataFilter<DateTime>)result).LessThanOrEqualTo.Should().Be(expectedMaxResult);
        }

        [Test]
        public void ToBluePrismObject_ShouldReturnNullDataFilter_FromNullFilter() =>
            new NullFilter<string>().ToBluePrismObject().Should().BeOfType<NullDataFilter<string>>();

        [Test]
        public void ToBluePrismObject_WithValueConverter_ShouldReturnNullDataFilter_FromNullFilter() =>
            new NullFilter<string>().ToBluePrismObject(int.Parse).Should().BeOfType<NullDataFilter<int>>();

        [Test]
        public void ToBluePrismObject_ShouldReturnMultiValueDataFilter_FromMultiValueFilter() =>
            new MultiValueFilter<string>(Array.Empty<Filter<string>>()).ToBluePrismObject().Should().BeOfType<MultiValueDataFilter<string>>();

        [Test]
        public void ToBluePrismObject_WithValueConverter_ShouldReturnMultiValueDataFilter_FromMultiValueFilter()
        {
            DateTime TestValueConverter(string x) => DateTime.Parse(x, CultureInfo.InvariantCulture);
            var expectedResult = TestValueConverter("0001-01-01T12:00:00");

            var result = new MultiValueFilter<string>(new[] {new EqualsFilter<string>("0001-01-01T12:00:00")})
                .ToBluePrismObject(TestValueConverter);

            result.Should().BeOfType<MultiValueDataFilter<DateTime>>();

            var filter = ((MultiValueDataFilter<DateTime>)result).First();

            filter.Should().BeOfType<EqualsDataFilter<DateTime>>();
            ((EqualsDataFilter<DateTime>)filter).EqualTo.Should().Be(expectedResult);
        }
    }
}
