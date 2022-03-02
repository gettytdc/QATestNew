namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using BpLibAdapters.Mappers.FilterMappers;
    using CommonTestClasses;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class ScheduleParametersMapperTests
    {
        [OneTimeSetUp]
        public void SetUp() =>
            FilterMapper.SetFilterMappers(new IFilterMapper[]
            {
                new EqualsFilterMapper(),
                new MultiValueFilterMapper(),
                new StringStartsWithFilterMapper()
            });

        [Test]
        public void ToBluePrismObject_WithScheduleParameters_ReturnsCorrectlyMappedData()
        {
            var domainParameters = SchedulesHelper.GetTestDomainScheduleParameters();
            var bluePrismParameters = domainParameters.ToBluePrismObject();
            SchedulesHelper.ValidateParametersModelsAreEqual(bluePrismParameters, domainParameters);
        }
    }
}
