namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Mappers.Dashboard;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ResourcesSummaryUtilizationModelMapperTests
    {
        [Test]
        public void ToModelObject_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var domainResourcesSummaryUtilization = new Domain.Dashboard.ResourcesSummaryUtilization()
            {
                Usage = 2,
                Dates = DateTime.Now
            };

            var modelUtilizationHeapMap = domainResourcesSummaryUtilization.ToModel();

            modelUtilizationHeapMap.ShouldBeEquivalentTo(domainResourcesSummaryUtilization);
        }

        [Test]
        public void ToModelObject_ShouldReturnCorrectlyMappedModel_WhenCalledWithList()
        {
            IEnumerable<Domain.Dashboard.ResourcesSummaryUtilization> domainResourcesSummaryUtilizations = new[]
            {
                new Domain.Dashboard.ResourcesSummaryUtilization()
                {
                    Usage = 2,
                    Dates = DateTime.Now
                },
                new Domain.Dashboard.ResourcesSummaryUtilization()
                {
                    Usage = 2,
                    Dates = DateTime.Now
                }
            };

            var modelUtilizationHeapMap = domainResourcesSummaryUtilizations.ToModel();

            modelUtilizationHeapMap.Count().Should().Be(2);
        }
    }
}
