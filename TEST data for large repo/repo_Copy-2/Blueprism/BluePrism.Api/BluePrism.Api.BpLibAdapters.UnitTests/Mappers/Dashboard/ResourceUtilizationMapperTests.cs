namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers.Dashboard
{
    using System;
    using System.Linq;
    using BpLibAdapters.Mappers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ResourceUtilizationMapperTests
    {
        [Test]
        public void ToDomainObject_WhenCorrectBluePrismObject_ShouldMapCorrectly()
        {
            var bpDomainResourcesSummaryUtilization = new Server.Domain.Models.Dashboard.ResourceUtilization()
            {
                DigitalWorkerName = "Worker1",
                ResourceId = Guid.NewGuid(),
                UtilizationDate = DateTime.Now,
                Usages = Enumerable.Range(1, 24).ToArray(),
            };

            var result = bpDomainResourcesSummaryUtilization.ToDomainObject();

            result.DigitalWorkerName.Should().Be(bpDomainResourcesSummaryUtilization.DigitalWorkerName);
            result.ResourceId.Should().Be(bpDomainResourcesSummaryUtilization.ResourceId);
            result.UtilizationDate.Should().Be(bpDomainResourcesSummaryUtilization.UtilizationDate);
            result.Usages.ShouldBeEquivalentTo(bpDomainResourcesSummaryUtilization.Usages);

        }
    }
}
