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
    public class StartsWithStringFilterModelMapperTests : UnitTestBase<StartsWithStringFilterModelMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenStartsWithParameterIsPopulated() =>
            ClassUnderTest.CanMap(new StartsWithStringFilterModel { Strtw = "a" }).Should().BeTrue();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenStartsWithParameterIsNotPopulated() =>
            ClassUnderTest.CanMap(new StartsWithStringFilterModel()).Should().BeFalse();

        [Test]
        public void Map_ShouldReturnStringStartsWithFilter_WhenStartsWithIsPopulated() =>
            ClassUnderTest.Map(new StartsWithStringFilterModel { Strtw = "a" }).Should().BeOfType<StringStartsWithFilter>();

        [TestCaseSource(nameof(InvalidFilters))]
        public void Map_ShouldThrowArgumentException_WhenInvalidFilterSupplied(BasicFilterModel<int> filter)
        {
            Action act = () => ClassUnderTest.Map(filter);
            act.ShouldThrow<ArgumentException>();
        }

        private static IEnumerable<TestCaseData> InvalidFilters() =>
            new[] { new BasicFilterModel<int>(), new RangeFilterModel<int>() }
                .ToTestCaseData();
    }
}
