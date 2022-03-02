namespace BluePrism.Api.UnitTests.Mappers.FilterMappers
{
    using Api.Mappers.FilterMappers;
    using Domain.Filters;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class NullFilterModelMapperTests : UnitTestBase<NullFilterModelMapper>
    {
        [Test]
        public void CanMap_ShouldReturnTrue_WhenSuppliedFilterIsNull() =>
            ClassUnderTest.CanMap<BasicFilterModel<string>>(null).Should().BeTrue();

        [Test]
        public void CanMap_ShouldReturnTrue_WhenEqualsParameterIsNullAndOfTypeBasicFilterModel() =>
            ClassUnderTest.CanMap(new BasicFilterModel<string>()).Should().BeTrue();

        [Test]
        public void CanMap_ShouldReturnFalse_WhenFilterSuppliedIsNotOfTypeBasicFilterModel() =>
            ClassUnderTest.CanMap(new StartsWithOrContainsStringFilterModel()).Should().BeFalse();

        [Test]
        public void Map_ShouldReturnNullFilter_WhenParameterIsNull() =>
            ClassUnderTest.Map((BasicFilterModel<string>)null).Should().BeOfType<NullFilter<string>>();

        [Test]
        public void Map_ShouldReturnNullFilter_WhenParameterIsBasicFilterModel() =>
            ClassUnderTest.Map(new BasicFilterModel<string>()).Should().BeOfType<NullFilter<string>>();

        [Test]
        public void Map_WithValueConverter_ShouldReturnCorrectNullFilter_WhenParameterIsNull() =>
            ClassUnderTest.Map((BasicFilterModel<string>)null, int.Parse).Should().BeOfType<NullFilter<int>>();

        [Test]
        public void Map_WithValueConverter_ShouldReturnCorrectNullFilter_WhenParameterIsBasicFilterModel() =>
            ClassUnderTest.Map(new BasicFilterModel<string>(), int.Parse).Should().BeOfType<NullFilter<int>>();
    }
}
