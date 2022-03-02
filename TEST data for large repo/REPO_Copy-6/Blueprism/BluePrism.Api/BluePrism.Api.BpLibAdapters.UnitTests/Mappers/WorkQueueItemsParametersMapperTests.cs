namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using BpLibAdapters.Mappers.FilterMappers;
    using CommonTestClasses;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class WorkQueueItemsParametersMapperTests
    {
        [Test]
        public void ToBluePrismObject_WithTestDomainWorkQueueItemsParameters_ReturnsCorrectlyMappedResult()
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
            };

            FilterMapper.SetFilterMappers(filterMappers);

            var domainWorkQueueItemsParameters = WorkQueueItemHelper.GetTestDomainWorkQueueParameters();
            var bluePrismWorkQueueItemsParams = domainWorkQueueItemsParameters.ToBluePrismObject();

            WorkQueueItemHelper.ValidateParametersModelsAreEqual(bluePrismWorkQueueItemsParams, domainWorkQueueItemsParameters);
        }
    }
}
