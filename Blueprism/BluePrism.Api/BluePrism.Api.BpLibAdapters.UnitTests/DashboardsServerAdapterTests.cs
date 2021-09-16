namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BpLibAdapters.Mappers.Dashboard;
    using CommonTestClasses.Extensions;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Server.Domain.Models.Dashboard;
    using Utilities.Testing;

    [TestFixture]
    public class DashboardsServerAdapterTests : UnitTestBase<DashboardsServerAdapter>
    {
        [Test]
        public async Task GetWorkQueueComposition_OnSuccess_ReturnsSuccess()
        {
            var workQueueIds = new List<Guid>()
            {
                new Guid(),
                new Guid(),
                new Guid(),
            };

            GetMock<IServer>()
                .Setup(m => m.GetWorkQueueCompositions(It.IsAny<List<Guid>>()))
                .Returns(new List<WorkQueueComposition>());

            var result = await ClassUnderTest.GetWorkQueueComposition(workQueueIds);

            result.Should().BeAssignableTo<Success>();
        }

        [Test]
        public async Task GetWorkQueueComposition_OnSuccess_ReturnsExpectedData()
        {
            var workQueueIds = new List<Guid>()
            {
                new Guid(),
                new Guid(),
                new Guid(),
            };

            var testData = GetListWorkQueueComposition();

            GetMock<IServer>()
                .Setup(m => m.GetWorkQueueCompositions(It.IsAny<List<Guid>>()))
                .Returns(testData);

            var expectedResult = testData.Select(x => x.ToDomainObject());
            var result = await ClassUnderTest.GetWorkQueueComposition(workQueueIds);

            result.ToSuccess().Value.ShouldBeEquivalentTo(expectedResult);
        }

        private List<WorkQueueComposition> GetListWorkQueueComposition() =>
            new List<WorkQueueComposition>()
            {
                new WorkQueueComposition()
                {
                    Completed = 0,
                    Deferred = 1,
                    Exceptioned = 2,
                    Locked = 3,
                    Pending = 4,
                    Name = "Queue1"
                },
                new WorkQueueComposition()
                {
                    Completed = 0,
                    Deferred = 1,
                    Exceptioned = 2,
                    Locked = 3,
                    Pending = 4,
                    Name = "Queue2"
                },
                new WorkQueueComposition()
                {
                    Completed = 0,
                    Deferred = 1,
                    Exceptioned = 2,
                    Locked = 3,
                    Pending = 4,
                    Name = "Queue3"
                }
            };
    }
}
