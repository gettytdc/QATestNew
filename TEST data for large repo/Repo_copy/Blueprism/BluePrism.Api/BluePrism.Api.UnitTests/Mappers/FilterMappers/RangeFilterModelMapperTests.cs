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
    public class RangeFilterModelMapperTests : UnitTestBase<RangeFilterModelMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenFilterHasGreaterThanEqualToAndLessThanEqualToParametersPopulated() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> {Gte = "a", Lte = "a"}).Should().BeTrue();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenSuppliedParameterIsNull() =>
            ClassUnderTest.CanMap((RangeFilterModel<string>)null).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenSuppliedParameterIsNotRangeFilterModel() =>
            ClassUnderTest.CanMap(new BasicFilterModel<string>()).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenGreaterThanEqualToIsNull() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> {Lte = "a"}).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenLessThanEqualToIsNull() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> {Gte = "a"}).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenEqualToIsPopulated() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> {Eq = "a"}).Should().BeFalse();

        [Test]
        public void Map_ShouldReturnRangeFilter_WhenRangeFilterModelSupplied() =>
            ClassUnderTest.Map(new RangeFilterModel<string> {Gte = "a", Lte = "a"}).Should().BeOfType<RangeFilter<string>>();

        [TestCaseSource(nameof(InvalidMapParameters))]
        public void Map_ShouldThrowArgumentException_WhenSuppliedParameterIsNotRangeFilterModel(BasicFilterModel<string> filter)
        {
            Action act = () => ClassUnderTest.Map(filter);
            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Map_WithValueConverter_ShouldReturnCorrectRangeFilter_WhenRangeFilterModelSupplied() =>
            ClassUnderTest.Map(new RangeFilterModel<string> { Gte = "1", Lte = "2" }, int.Parse).Should().BeOfType<RangeFilter<int>>();

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
