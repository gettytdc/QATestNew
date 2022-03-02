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
    public class LessThanOrEqualToFilterModelMapperTests : UnitTestBase<LessThanOrEqualToFilterModelMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenFilterHasLessThanEqualToPopulated() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> {Lte = "a"}).Should().BeTrue();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenSuppliedParameterIsNull() =>
            ClassUnderTest.CanMap((RangeFilterModel<string>)null).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenSuppliedParameterIsNotRangeFilterModel() =>
            ClassUnderTest.CanMap(new BasicFilterModel<string>()).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenGreaterThanEqualToIsNotNull() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> {Gte = "a"}).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenLessThanEqualToIsNull() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string>()).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenEqualToIsPopulated() =>
            ClassUnderTest.CanMap(new RangeFilterModel<string> {Eq = "a"}).Should().BeFalse();

        [Test]
        public void Map_ShouldReturnLessThanOrEqualToFilter_WhenRangeFilterModelSupplied() =>
            ClassUnderTest.Map(new RangeFilterModel<string> {Lte = "a"}).Should().BeOfType<LessThanOrEqualToFilter<string>>();

        [TestCaseSource(nameof(InvalidMapParameters))]
        public void Map_ShouldThrowArgumentException_WhenSuppliedParameterIsNotRangeFilterModel(BasicFilterModel<string> filter)
        {
            Action act = () => ClassUnderTest.Map(filter);
            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Map_WithValueConverter_ShouldReturnCorrectLessThanOrEqualToFilterFilter_WhenRangeFilterModelSupplied() =>
            ClassUnderTest.Map(new RangeFilterModel<string> { Lte = "2" }, int.Parse).Should().BeOfType<LessThanOrEqualToFilter<int>>();

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
