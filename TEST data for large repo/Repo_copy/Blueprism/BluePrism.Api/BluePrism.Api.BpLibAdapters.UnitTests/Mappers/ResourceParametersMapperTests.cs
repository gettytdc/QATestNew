namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using BpLibAdapters.Mappers.FilterMappers;
    using CommonTestClasses;
    using NUnit.Framework;

    class ResourceParametersMapperTests
    {
        [OneTimeSetUp]
        public void SetUp() =>
            FilterMapper.SetFilterMappers(new IFilterMapper[]
            {
                new NullFilterMapper(),
                new EqualsFilterMapper(),
                new MultiValueFilterMapper(),
                new GreaterThanOrEqualToFilterMapper(),
                new StringStartsWithFilterMapper()
            });

        [Test]
        public void ToBluePrismObject_WithResourceParameters_ReturnsCorrectlyMappedData()
        {
            var domainParameters = ResourceHelper.GetTestDomainResourceParameters();
            var bluePrismParameters = domainParameters.ToBluePrismObject();
            ResourceHelper.ValidateParametersModelsAreEqual(bluePrismParameters, domainParameters);
        }
    }
}
