namespace BluePrism.Api.Services.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Autofac;
    using BpLibAdapters;
    using Domain.Dashboard;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    public class DashboardsServiceTests : UnitTestBase<DashboardsService>
    { public override void Setup()
        {
            base.Setup(builder =>
            {
                builder.RegisterGeneric(typeof(MockAdapterAuthenticatedMethodRunner<>)).As(typeof(IAdapterAuthenticatedMethodRunner<>));
            });
        }

        [Test]
        public async Task GetWorkQueueComposition_ShouldReturnSuccess_WhenSuccessful()
        {
            var workQueueCompositions = new List<WorkQueueComposition>();

            GetMock<IDashboardsServerAdapter>()
                .Setup(x => x.GetWorkQueueComposition(It.IsAny<List<Guid>>()))
                .ReturnsAsync(ResultHelper.Succeed<IEnumerable<WorkQueueComposition>>(workQueueCompositions));

            var result = await ClassUnderTest.GetWorkQueueCompositions(new List<Guid>());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetWorkQueueComposition_ShouldReturnPermissionsError_WhenUserDoesNotHavePermissions()
        {
            GetMock<IDashboardsServerAdapter>()
                .Setup(x => x.GetWorkQueueComposition(It.IsAny<List<Guid>>()))
                .ReturnsAsync(ResultHelper<IEnumerable<WorkQueueComposition>>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetWorkQueueCompositions(new List<Guid>());

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetWorkQueueComposition_ShouldReturnExpectedResult_WhenSuccess()
        {
            var testData = GetListWorkQueueComposition();
            GetMock<IDashboardsServerAdapter>()
                .Setup(x => x.GetWorkQueueComposition(It.IsAny<List<Guid>>()))
                .ReturnsAsync(ResultHelper.Succeed<IEnumerable<WorkQueueComposition>>(testData));

            var result = await ClassUnderTest.GetWorkQueueCompositions(new List<Guid>());

            result.OnSuccess((x) => x.ShouldBeEquivalentTo(testData));
        }

        private List<WorkQueueComposition> GetListWorkQueueComposition() =>
            new List<WorkQueueComposition>()
            {
                new WorkQueueComposition()
                {
                    Id = Guid.NewGuid(),
                    Name = "Queue1",
                    Completed = 0,
                    Deferred = 1,
                    Exceptioned = 2,
                    Locked = 3,
                    Pending = 4,
                },
                new WorkQueueComposition()
                {
                    Id = Guid.NewGuid(),
                    Name = "Queue2",
                    Completed = 0,
                    Deferred = 1,
                    Exceptioned = 2,
                    Locked = 3,
                    Pending = 4,
                },
                new WorkQueueComposition()
                {
                    Id = Guid.NewGuid(),
                    Name = "Queue3",
                    Completed = 0,
                    Deferred = 1,
                    Exceptioned = 2,
                    Locked = 3,
                    Pending = 4,
                }
            };
    }
}
