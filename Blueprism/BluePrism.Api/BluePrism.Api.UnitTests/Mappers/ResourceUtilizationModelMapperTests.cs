namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Mappers.Dashboard;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ResourceUtilizationModelMapperTests
    {
        [Test]
        public void ToModelObject_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var domainResourceUtilization = new Domain.Dashboard.ResourceUtilization()
            {
                ResourceId = Guid.NewGuid(),
                DigitalWorkerName = "worker 1",
                Usages = Enumerable.Range(1, 24).ToArray(),
                UtilizationDate = DateTime.Now
            };

            var modelUtilizationHeapMap = domainResourceUtilization.ToModel();

            modelUtilizationHeapMap.ShouldBeEquivalentTo(domainResourceUtilization);
        }

        [Test]
        public void ToModelObject_ShouldReturnCorrectlyMappedModel_WhenCalledWithList()
        {
            IEnumerable<Domain.Dashboard.ResourceUtilization> domainResourceUtilizations = new []
            {
                new Domain.Dashboard.ResourceUtilization()
                {
                    ResourceId = Guid.NewGuid(),
                    DigitalWorkerName = "worker 1",
                    Usages = Enumerable.Range(1, 24).ToArray(),
                    UtilizationDate = DateTime.Now
                },
                new Domain.Dashboard.ResourceUtilization()
                {
                    ResourceId = Guid.NewGuid(),
                    DigitalWorkerName = "worker 2",
                    Usages = Enumerable.Range(1, 24).ToArray(),
                    UtilizationDate = DateTime.Now
                }
            };

            var modelUtilizationHeapMap = domainResourceUtilizations.ToModel();

            modelUtilizationHeapMap.Count().Should().Be(2);
        }
    }
}
