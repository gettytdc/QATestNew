namespace BluePrism.Api.BpLibAdapters.UnitTests.DashboardServiceAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    public class GetResourceUtilizationTests : UnitTestBase<DashboardsServerAdapter>
    {
        [Test]
        public async Task GetResourceUtilization_OnSuccess_ReturnsSuccess()
        {
            GetMock<IServer>()
                .Setup(m => m.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                .Returns(new List<ResourceUtilization>());

            var result = await ClassUnderTest.GetResourceUtilization(new Domain.ResourceUtilizationParameters());

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetResourceUtilization_OnSuccess_ReturnsExpectedData()
        {
            var expectedResult = new List<ResourceUtilization>()
            {
                new ResourceUtilization()
                {
                    ResourceId = Guid.NewGuid(),
                    DigitalWorkerName = "worker 1",
                    Usages = Enumerable.Range(1, 24).ToArray(),
                    UtilizationDate = DateTime.Now
                },
                new ResourceUtilization()
                {
                    ResourceId = Guid.NewGuid(),
                    DigitalWorkerName = "worker 2",
                    Usages = Enumerable.Range(1, 24).ToArray(),
                    UtilizationDate = DateTime.Now
                }
            };

            GetMock<IServer>()
                .Setup(m => m.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                .Returns(expectedResult);

            var result = await ClassUnderTest.GetResourceUtilization(new Domain.ResourceUtilizationParameters());

            result.ToSuccess().Value.ShouldBeEquivalentTo(expectedResult);
        }

    }
}
