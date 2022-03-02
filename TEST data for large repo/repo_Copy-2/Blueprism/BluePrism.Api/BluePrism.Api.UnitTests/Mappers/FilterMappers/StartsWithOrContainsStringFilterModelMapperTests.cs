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
    public class StartsWithOrContainsStringFilterModelMapperTests : UnitTestBase<StartsWithOrContainsStringFilterModelMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenContainsParameterIsPopulated() =>
            ClassUnderTest.CanMap(new StartsWithOrContainsStringFilterModel {Ctn = "a"}).Should().BeTrue();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenContainsParameterIsNotPopulated() =>
            ClassUnderTest.CanMap(new StartsWithOrContainsStringFilterModel()).Should().BeFalse();

        [Test]
        public void Map_ShouldReturnStringContainsFilter_WhenContainsIsPopulated() =>
            ClassUnderTest.Map(new StartsWithOrContainsStringFilterModel {Ctn = "a"}).Should().BeOfType<StringContainsFilter>();

        [TestCaseSource(nameof(InvalidFilters))]
        public void Map_ShouldThrowArgumentException_WhenInvalidFilterSupplied(BasicFilterModel<int> filter)
        {
            Action act = () => ClassUnderTest.Map(filter);
            act.ShouldThrow<ArgumentException>();
        }

        private static IEnumerable<TestCaseData> InvalidFilters() =>
            new[] {new BasicFilterModel<int>(), new RangeFilterModel<int>()}
                .ToTestCaseData();
    }
}
