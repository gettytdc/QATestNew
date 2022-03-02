namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using BpLibAdapters.Mappers;
    using Domain;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ResourceUtilizationParametersMapperTests
    {
        [Test]
        public void ToBluePrismObject_WithTestResourceUtilizationParameters_ReturnsCorrectlyMappedResult()
        {
           
            var domainResourceUtilizationParameters = new ResourceUtilizationParameters()
            {
                StartDate = DateTime.Now,
                ResourceIds = new[]{Guid.NewGuid(), Guid.NewGuid()},
                PageNumber = 1,
                PageSize = 2
            };
            var result = domainResourceUtilizationParameters.ToBluePrismObject();

            result.StartDate.Should().Be(domainResourceUtilizationParameters.StartDate);
            result.ResourceIds.Should().BeEquivalentTo(domainResourceUtilizationParameters.ResourceIds);
        }

        [Test]
        public void ToBluePrismObject_WithTestResourceUtilizationWithEmptyCollections_ReturnsCorrectlyMappedResult()
        {

            var domainResourceUtilizationParameters = new ResourceUtilizationParameters()
            {
                ResourceIds = null
            };
            var result = domainResourceUtilizationParameters.ToBluePrismObject();

            result.ResourceIds.Should().BeEmpty();
        }
    }
}
