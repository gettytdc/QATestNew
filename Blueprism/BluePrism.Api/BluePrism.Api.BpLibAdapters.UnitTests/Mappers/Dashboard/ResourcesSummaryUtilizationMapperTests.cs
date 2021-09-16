namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers.Dashboard
{
    using System;
    using BpLibAdapters.Mappers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ResourcesSummaryUtilizationMapperTests
    {
        [Test]
        public void ToDomainObject_WhenCorrectBluePrismObject_ShouldMapCorrectly()
        {
            var bpDomainResourcesSummaryUtilization = new Server.Domain.Models.Dashboard.ResourcesSummaryUtilization()
            {
                Dates = DateTime.Now,
                Usage = 99
            };

            var result = bpDomainResourcesSummaryUtilization.ToDomainObject();

            result.Dates.Should().Be(bpDomainResourcesSummaryUtilization.Dates);
            result.Usage.Should().Be(bpDomainResourcesSummaryUtilization.Usage);

        }
    }
}
