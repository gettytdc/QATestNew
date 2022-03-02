namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using BpLibAdapters.Mappers;
    using Domain;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ResourcesSummaryUtilizationParametersMapperTests
    {
        [Test]
        public void ToBluePrismObject_WithTestResourcesSummaryUtilizationParameters_ReturnsCorrectlyMappedResult()
        {

            var domainResourceUtilizationParameters = new ResourcesSummaryUtilizationParameters()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                ResourceIds = new[] { Guid.NewGuid(), Guid.NewGuid() },
            
            };
            var result = domainResourceUtilizationParameters.ToBluePrismObject();

            result.StartDate.Should().Be(domainResourceUtilizationParameters.StartDate);
            result.EndDate.Should().Be(domainResourceUtilizationParameters.EndDate);
            result.ResourceIds.Should().BeEquivalentTo(domainResourceUtilizationParameters.ResourceIds);
        }

        [Test]
        public void ToBluePrismObject_WithTestResourcesSummaryUtilizationHeatWithEmptyCollections_ReturnsCorrectlyMappedResult()
        {

            var domainResourceUtilizationParameters = new ResourcesSummaryUtilizationParameters()
            {
                ResourceIds = null
            };
            var result = domainResourceUtilizationParameters.ToBluePrismObject();

            result.ResourceIds.Should().BeEmpty();
        }
    }
}
