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
    using Utilities.Testing;

    [TestFixture]
    public class GreaterThanOrEqualToFilterModelMapperTests : UnitTestBase<GreaterThanOrEqualToFilterModelMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenFilterHasGreaterThanEqualToPopulated() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> { Gte = "a" }).Should().BeTrue();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenSuppliedParameterIsNull() =>
            ClassUnderTest.CanMap((RangeFilterModel<string>)null).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenSuppliedParameterIsNotRangeFilterModel() =>
            ClassUnderTest.CanMap(new BasicFilterModel<string>()).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenLessThanEqualToIsNotNull() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> { Lte = "a" }).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenGreaterThanEqualToIsNull() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string>()).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenEqualToIsPopulated() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> { Eq = "a" }).Should().BeFalse();

        [Test]
        public void Map_ShouldReturnGreaterThanOrEqualToFilter_WhenRangeFilterModelSupplied() =>
            ClassUnderTest.Map(new RangeFilterModel<string> { Gte = "a" }).Should().BeOfType<GreaterThanOrEqualToFilter<string>>();

        [TestCaseSource(nameof(InvalidMapParameters))]
        public void Map_ShouldThrowArgumentException_WhenSuppliedParameterIsNotRangeFilterModel(BasicFilterModel<string> filter)
        {
            Action act = () => ClassUnderTest.Map(filter);
            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Map_WithValueConverter_ShouldReturnCorrectGreaterThanOrEqualToFilterFilter_WhenRangeFilterModelSupplied() =>
            ClassUnderTest.Map(new RangeFilterModel<string> { Gte = "2" }, int.Parse).Should().BeOfType<GreaterThanOrEqualToFilter<int>>();

        [TestCaseSource(nameof(InvalidMapParameters))]
        public void Map_WithValueConverter_ShouldThrowArgumentException_WhenSuppliedParameterIsNotRangeFilterModel(BasicFilterModel<string> filter)
        {
            Action act = () => ClassUnderTest.Map(filter, int.Parse);
            act.ShouldThrow<ArgumentException>();
        }

        private static IEnumerable<TestCaseData> InvalidMapParameters() =>
            new[]
            {
                new BasicFilterModel<string>(), null
            }
            .ToTestCaseData();
    }
}
