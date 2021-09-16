namespace BluePrism.Api.Services.UnitTests.DashboardService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using BpLibAdapters;
    using Domain;
    using Domain.Dashboard;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    public class GetResourceUtilizationTests : UnitTestBase<DashboardsService>
    {
        public override void Setup()
        {
            base.Setup(builder =>
            {
                builder.RegisterGeneric(typeof(MockAdapterAuthenticatedMethodRunner<>)).As(typeof(IAdapterAuthenticatedMethodRunner<>));
            });
        }

        [Test]
        public async Task GetResourceUtilization_ShouldReturnSuccess_WhenSuccessful()
        {
            GetMock<IDashboardsServerAdapter>()
                .Setup(x => x.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                .ReturnsAsync(ResultHelper.Succeed<IEnumerable<ResourceUtilization>>(new List<ResourceUtilization>()));

            var result = await ClassUnderTest.GetResourceUtilization(new ResourceUtilizationParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetResourceUtilization_ShouldReturnPermissionsError_WhenUserDoesNotHavePermissions()
        {
            GetMock<IDashboardsServerAdapter>()
                .Setup(x => x.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                .ReturnsAsync(ResultHelper<IEnumerable<ResourceUtilization>>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetResourceUtilization(new ResourceUtilizationParameters());

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetResourceUtilization_ShouldReturnExpectedResult_WhenSuccess()
        {
            var testData = new[]
            {
                new ResourceUtilization()
                {
                    ResourceId = Guid.NewGuid(),
                    DigitalWorkerName = "worker 1",
                    UtilizationDate = DateTime.Now,
                    Usages = Enumerable.Range(1, 24).ToArray(),
                },
                new ResourceUtilization()
                {
                    ResourceId = Guid.NewGuid(),
                    DigitalWorkerName = "worker 2",
                    UtilizationDate = DateTime.Now,
                    Usages = Enumerable.Range(1, 24).ToArray(),
                }
            };
            GetMock<IDashboardsServerAdapter>()
                .Setup(x => x.GetResourceUtilization(It.IsAny<ResourceUtilizationParameters>()))
                .ReturnsAsync(ResultHelper.Succeed<IEnumerable<ResourceUtilization>>(testData));

            var result = await ClassUnderTest.GetWorkQueueCompositions(new List<Guid>());

            result.OnSuccess((x) => x.ShouldBeEquivalentTo(testData));
        }
    }
}
