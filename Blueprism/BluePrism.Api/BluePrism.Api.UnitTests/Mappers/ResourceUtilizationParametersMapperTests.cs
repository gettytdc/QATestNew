namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using Api.Mappers;
    using FluentAssertions;
    using NUnit.Framework;
    using ResourceUtilizationParameters = Models.ResourceUtilizationParameters;

    [TestFixture]
    public class ResourceUtilizationParametersMapperTests
    {
        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedObject_WhenCalledWithValidModel()
        {
            var modelResourceUtilizationParameters = new ResourceUtilizationParameters()
            {
                StartDate = DateTime.Now,
                ResourceIds = new[] { Guid.NewGuid(), Guid.NewGuid() },
                PageNumber = 1,
                PageSize = 2
            };
            var domainResourceUtilizationParameters = modelResourceUtilizationParameters.ToDomainObject();

            domainResourceUtilizationParameters.StartDate.Should().Be(modelResourceUtilizationParameters.StartDate);
            domainResourceUtilizationParameters.ResourceIds.Should().BeEquivalentTo(modelResourceUtilizationParameters.ResourceIds);
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedObject_WhenCalledWithValidModelWithEmptyResourceIdsAndAttributeIds()
        {
            var modelResourceUtilizationParameters = new ResourceUtilizationParameters()
            {
                ResourceIds = new Guid[] { }
            };
            var domainResourceUtilizationParameters = modelResourceUtilizationParameters.ToDomainObject();

            domainResourceUtilizationParameters.ResourceIds.Should().BeEmpty();
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedObject_WhenCalledWithoutPageSize()
        {
            var modelResourceUtilizationParameters = new ResourceUtilizationParameters();
            var domainResourceUtilizationParameters = modelResourceUtilizationParameters.ToDomainObject();

            domainResourceUtilizationParameters.PageSize.Should().BeNull();
        }

        [Test]
        public void ToDomainObject_ShouldReturnCorrectlyMappedObject_WhenCalledWithoutPageNumber()
        {
            var modelResourceUtilizationParameters = new ResourceUtilizationParameters();
            var domainResourceUtilizationParameters = modelResourceUtilizationParameters.ToDomainObject();

            domainResourceUtilizationParameters.PageNumber.Should().BeNull();
        }
    }
}
