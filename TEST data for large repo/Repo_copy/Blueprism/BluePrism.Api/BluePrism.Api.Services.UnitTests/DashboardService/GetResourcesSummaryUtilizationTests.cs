namespace BluePrism.Api.Services.UnitTests.DashboardService
{
    using System;
    using System.Collections.Generic;
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

    public class GetResourcesSummaryUtilizationTests : UnitTestBase<DashboardsService>
    {
        public override void Setup()
        {
            base.Setup(builder =>
            {
                builder.RegisterGeneric(typeof(MockAdapterAuthenticatedMethodRunner<>)).As(typeof(IAdapterAuthenticatedMethodRunner<>));
            });
        }

        [Test]
        public async Task GetResourcesSummaryUtilization_ShouldReturnSuccess_WhenSuccessful()
        {
            GetMock<IDashboardsServerAdapter>()
                .Setup(x => x.GetResourcesSummaryUtilization(It.IsAny<ResourcesSummaryUtilizationParameters>()))
                .ReturnsAsync(ResultHelper.Succeed<IEnumerable<ResourcesSummaryUtilization>>(new List<ResourcesSummaryUtilization>()));

            var result = await ClassUnderTest.GetResourcesSummaryUtilization(new ResourcesSummaryUtilizationParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetResourcesSummaryUtilization_ShouldReturnPermissionsError_WhenUserDoesNotHavePermissions()
        {
            GetMock<IDashboardsServerAdapter>()
                .Setup(x => x.GetResourcesSummaryUtilization(It.IsAny<ResourcesSummaryUtilizationParameters>()))
                .ReturnsAsync(ResultHelper<IEnumerable<ResourcesSummaryUtilization>>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetResourcesSummaryUtilization(new ResourcesSummaryUtilizationParameters());

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetResourcesSummaryUtilization_ShouldReturnExpectedResult_WhenSuccess()
        {
            var testData = new[]
            {
                new ResourcesSummaryUtilization()
                {
                    Dates = DateTime.Now,
                    Usage = 1,
                },
                new ResourcesSummaryUtilization()
                {
                    Dates = DateTime.Now,
                    Usage = 2,
                }
            };
            GetMock<IDashboardsServerAdapter>()
                .Setup(x => x.GetResourcesSummaryUtilization(It.IsAny<ResourcesSummaryUtilizationParameters>()))
                .ReturnsAsync(ResultHelper.Succeed<IEnumerable<ResourcesSummaryUtilization>>(testData));

            var result = await ClassUnderTest.GetResourcesSummaryUtilization(new ResourcesSummaryUtilizationParameters());

            result.OnSuccess((x) => x.ShouldBeEquivalentTo(testData));
        }
    }
}
