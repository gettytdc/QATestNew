using BluePrism.AutomateAppCore;
using BluePrism.Core.ActiveDirectory;
using BluePrism.Core.ActiveDirectory.DirectoryServices;
using BluePrism.Core.ActiveDirectory.UserQuery;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BluePrism.ActiveDirectoryUserSearchService.UnitTests
{
    [TestFixture]
    public class ActiveDirectoryUserSearchServiceTests
    {
        private Mock<IServer> _server;
        private ActiveDirectoryUserSearcher.Services.ActiveDirectoryUserSearchService
            _searchService;
        private DirectorySearcherCredentials _searcherCredentials;

        [SetUp]
        public void SetUp()
        {
            _server = new Mock<IServer>();
            _searchService = 
                new ActiveDirectoryUserSearcher.Services.ActiveDirectoryUserSearchService(_server.Object);
            _searcherCredentials = new DirectorySearcherCredentials("username", new Common.Security.SafeString());
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  \t")]
        public void FindActiveDirectoryUsers_NullEmptyWhiteSpaceSearchRoot_InvalidQuery(object value)
        {
            _server.Setup(x => x.FindActiveDirectoryUsers(
                It.IsAny<string>(),
                UserFilterType.Cn,
                It.IsAny<string>(),
                It.IsAny<QueryPageOptions>(),
                It.IsAny<DirectorySearcherCredentials>()))
            .Returns(PaginatedUserQueryResult.Success(new List<ActiveDirectoryUser>(), 0));

            var result = _searchService.FindActiveDirectoryUsers(
                value as string, UserFilterType.Cn, string.Empty, _searcherCredentials, 0, 0);

            result.Should().NotBeNull();
            result.Status.Should().Be(QueryStatus.InvalidQuery);
        }

        [Test]
        public void FindActiveDirectoryUsers_SearchRootOk_SuccessNoUsers()
        {
            _server.Setup(x => x.FindActiveDirectoryUsers(
                It.IsAny<string>(),
                UserFilterType.Cn,
                It.IsAny<string>(),
                It.IsAny<QueryPageOptions>(),
                It.IsAny<DirectorySearcherCredentials>()))
            .Returns(PaginatedUserQueryResult.Success(new List<ActiveDirectoryUser>(), 0));

            var result = _searchService.FindActiveDirectoryUsers(
                "search_root", UserFilterType.Cn, string.Empty, _searcherCredentials, 0, 1);

            result.Should().NotBeNull();
            result.Status.Should().Be(QueryStatus.Success);
            result.TotalUsers.Should().Be(0);
            result.RequestedPage.Should().NotBeNull();
            result.RequestedPage.ToList().Count.Should().Be(0);
        }

        [Test]
        public void FindActiveDirectoryUsers_SearchRootOk_SuccessOneUsers()
        {
            _server.Setup(x => x.FindActiveDirectoryUsers(
                It.IsAny<string>(),
                UserFilterType.Cn,
                It.IsAny<string>(),
                It.IsAny<QueryPageOptions>(),
                It.IsAny<DirectorySearcherCredentials>()))
            .Returns(PaginatedUserQueryResult.Success(new List<ActiveDirectoryUser>()
                { new ActiveDirectoryUser("prinName", "sid", "disname", true) }, 1));

            var result = _searchService.FindActiveDirectoryUsers(
                "search_root", UserFilterType.Cn, string.Empty, _searcherCredentials, 0, 1);

            result.Should().NotBeNull();
            result.Status.Should().Be(QueryStatus.Success);
            result.TotalUsers.Should().Be(1);
            result.RequestedPage.Should().NotBeNull();

            var usersList = result.RequestedPage.ToList();
            usersList.Count.Should().Be(1);
            usersList[0].UserPrincipalName.Should().Be("prinName");
            usersList[0].Sid.Should().Be("sid");
            usersList[0].DistinguishedName.Should().Be("disname");
        }

        [Test]
        public void GetDistinguishedNameOfCurrentForest_ReturnsValue_ShouldBeOk()
        {
            _server.Setup(x => x.GetDistinguishedNameOfCurrentForest())
            .Returns("ForestName");
            _searchService.GetDistinguishedNameOfCurrentForest().Should().Be("ForestName");
        }
    }
}
