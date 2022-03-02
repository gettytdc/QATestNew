namespace BluePrism.Api.UnitTests.Mappers.FilterMappers
{
    using Api.Mappers.FilterMappers;
    using Domain.Filters;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class EqualFilterModelMapperTests : UnitTestBase<EqualFilterModelMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenEqualsParameterIsPopulated() =>
          ClassUnderTest.CanMap(new BasicFilterModel<int?>{Eq = 4}).Should().BeTrue();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenSuppliedParameterIsNull() =>
            ClassUnderTest.CanMap((BasicFilterModel<string>)null).Should().BeFalse();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenEqualsParameterIsNUll() =>
            ClassUnderTest.CanMap(new BasicFilterModel<int?> { Eq = null }).Should().BeFalse();

        [Test]
        public void Map_WithValueConverter_ShouldReturnCorrectEqualsToFilter_WhenRangeFilterModelSupplied() =>
            ClassUnderTest.Map(new BasicFilterModel<string> { Eq = "2" }, int.Parse).Should().BeOfType<EqualsFilter<int>>();
    }
}
