namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using BpLibAdapters.Mappers.FilterMappers;
    using CommonTestClasses;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class ProcessSessionParametersMapperTests
    {
        [Test]
        public void ToBluePrismObject_WithTestDomainProcessSessionParameters_ReturnsCorrectlyMappedResult()
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

            var domainProcessSessionParameters = SessionsHelper.GetTestDomainProcessSessionParameters();
            var bluePrismProcessSessionParams = domainProcessSessionParameters.ToBluePrismObject();

            SessionsHelper.ValidateParameterModelsAreEqual(bluePrismProcessSessionParams, domainProcessSessionParameters);
        }
    }
}
