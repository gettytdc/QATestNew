namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using AutomateAppCore.Groups;
    using Utilities.Testing;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Domain.Errors;
    using BluePrism.Server.Domain.Models;

    [TestFixture]
    public class GroupsServerAdapterTests : UnitTestBase<GroupsServerAdapter>
    {
        [Test]
        public async Task AddToGroup_ShouldReturnSuccess_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(x => x.AddToGroup(It.IsAny<GroupTreeType>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<GroupMember>>()))
                .Verifiable();

            var result = await ClassUnderTest.AddToGroup(Domain.Groups.GroupTreeType.None, Guid.NewGuid(),
                Array.Empty<Domain.Groups.GroupMember>());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task AddToGroup_ShouldReturnFailure_WhenPermissionsExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(x => x.AddToGroup(It.IsAny<GroupTreeType>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<GroupMember>>()))
                .Throws<PermissionException>();

            var result = await ClassUnderTest.AddToGroup(Domain.Groups.GroupTreeType.None, Guid.NewGuid(),
                Array.Empty<Domain.Groups.GroupMember>());

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetDefaultGroupId_ShouldReturnSuccess_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(x => x.GetDefaultGroupId(It.IsAny<GroupTreeType>()))
                .Returns(Guid.NewGuid());

            var result = await ClassUnderTest.GetDefaultGroupId(Domain.Groups.GroupTreeType.None);

            (result is Success).Should().BeTrue();
        }
    }
}
