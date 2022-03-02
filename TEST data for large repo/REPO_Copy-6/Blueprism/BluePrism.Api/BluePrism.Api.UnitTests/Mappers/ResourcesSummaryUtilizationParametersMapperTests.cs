namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using Api.Mappers;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;
    
    [TestFixture]
    public class ResourcesSummaryUtilizationParametersMapperTests
    {
        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedObject_WhenCalledWithValidModel()
        {
            var modelResourceUtilizationParameters = new ResourcesSummaryUtilizationParameters()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                ResourceIds = new[] {Guid.NewGuid(), Guid.NewGuid()},
            };
            var domainResourceUtilizationParameters = modelResourceUtilizationParameters.ToDomainObject();

            domainResourceUtilizationParameters.StartDate.Should().Be(modelResourceUtilizationParameters.StartDate);
            domainResourceUtilizationParameters.EndDate.Should().Be(modelResourceUtilizationParameters.EndDate);
            domainResourceUtilizationParameters.ResourceIds.Should().BeEquivalentTo(modelResourceUtilizationParameters.ResourceIds);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedObject_WhenCalledWithValidModelWithEmptyResourceIdsAndAttributeIds()
        {
            var modelResourceUtilizationParameters = new ResourcesSummaryUtilizationParameters()
            {
                ResourceIds = new Guid[] { }
            };
            var domainResourceUtilizationParameters = modelResourceUtilizationParameters.ToDomainObject();

            domainResourceUtilizationParameters.ResourceIds.Should().BeEmpty();
        }
    }
}
