namespace BluePrism.Api.BpLibAdapters.UnitTests.DashboardServiceAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using CommonTestClasses.Extensions;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Server.Domain.Models;
    using Server.Domain.Models.Dashboard;
    using Utilities.Testing;

    [TestFixture]
    public class GetResourcesSummaryUtilizationTests : UnitTestBase<DashboardsServerAdapter>
    {
        [Test]
        public async Task GetResourcesSummaryUtilization_OnSuccess_ReturnsSuccess()
        {
            GetMock<IServer>()
                .Setup(m => m.GetResourcesSummaryUtilization(It.IsAny<ResourcesSummaryUtilizationParameters>()))
                .Returns(new List<ResourcesSummaryUtilization>());

            var result = await ClassUnderTest.GetResourcesSummaryUtilization(new Domain.ResourcesSummaryUtilizationParameters());

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetResourcesSummaryUtilization_OnSuccess_ReturnsExpectedData()
        {
            var expectedResult = new List<ResourcesSummaryUtilization>()
            {
                new ResourcesSummaryUtilization()
                {
                    Usage = 1,
                    Dates = DateTime.Now
                },
                new ResourcesSummaryUtilization()
                {
                    Usage = 2,
                    Dates = DateTime.Now
                }
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourcesSummaryUtilization(It.IsAny<ResourcesSummaryUtilizationParameters>()))
                .Returns(expectedResult);

            var result = await ClassUnderTest.GetResourcesSummaryUtilization(new Domain.ResourcesSummaryUtilizationParameters());

            result.ToSuccess().Value.ShouldBeEquivalentTo(expectedResult);
        }

    }
}
