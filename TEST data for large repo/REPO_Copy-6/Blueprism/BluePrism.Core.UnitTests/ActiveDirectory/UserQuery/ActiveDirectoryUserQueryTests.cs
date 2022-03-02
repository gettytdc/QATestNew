using BluePrism.Common.Security;
using BluePrism.Core.ActiveDirectory;
using BluePrism.Core.ActiveDirectory.DirectoryServices;
using BluePrism.Core.ActiveDirectory.UserQuery;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;

namespace BluePrism.Core.UnitTests.ActiveDirectory.UserQuery
{
    [TestFixture]
    public class ActiveDirectoryUserQueryTests
    {
        private const string SearchRoot = "CN=admin,OU=system,DC=some,DC=domain,DC=COM";
        private Mock<IDirectorySearcher> _directorySearcherMock;
        private ActiveDirectoryUserQueryOptions _queryOptions;
        private Mock<IDirectorySearcherBuilder> _directorySearcherBuilderMock;
        private Mock<IMappedUserFinder> _mappedUserFinderMock;
        private ActiveDirectoryUserQuery _activeDirectoryUserQuery;
        private Mock<ISearchResultCollection> _searchResultCollectionMock;
        
        [SetUp]
        public void SetUp()
        {
            _searchResultCollectionMock = new Mock<ISearchResultCollection>();
            _searchResultCollectionMock
                .Setup(searchResults => searchResults.GetEnumerator())
                .Returns(TestUsers().Select(x => CreateSearchResultMock(x)).GetEnumerator());
            _searchResultCollectionMock
                .Setup(SearchResults => SearchResults.Dispose());
            
            _directorySearcherMock = new Mock<IDirectorySearcher>();
            _directorySearcherMock
                .Setup(searcher => searcher.FindAll())
                .Returns(_searchResultCollectionMock.Object);
            _directorySearcherMock
                .Setup(searcher => searcher.Dispose()); 
            
            _queryOptions = new ActiveDirectoryUserQueryOptions(SearchRoot, UserFilterType.Cn, "John*", new QueryPageOptions(50, 25), new DirectorySearcherCredentials("someuser", "password".AsSecureString()));

            _directorySearcherBuilderMock = new Mock<IDirectorySearcherBuilder>();
            _directorySearcherBuilderMock
                .Setup(builder => builder.WithSearchRoot(It.IsAny<string>(), It.IsAny<DirectorySearcherCredentials>()))
                .Returns(_directorySearcherBuilderMock.Object);
            _directorySearcherBuilderMock
                .Setup(builder => builder.WithUserFilter(It.IsAny<UserFilter>()))
                .Returns(_directorySearcherBuilderMock.Object);
            _directorySearcherBuilderMock
                .Setup(builder => builder.WithUserSearchColumns())
                .Returns(_directorySearcherBuilderMock.Object);
            _directorySearcherBuilderMock
                .Setup(builder => builder.WithSortByCn())
                .Returns(_directorySearcherBuilderMock.Object);
            _directorySearcherBuilderMock
                .Setup(builder => builder.WithPaging(It.IsAny<QueryPageOptions>()))
                .Returns(_directorySearcherBuilderMock.Object);
            _directorySearcherBuilderMock
                .Setup(builder => builder.Build())
                .Returns(_directorySearcherMock.Object);

            var alreadyMappedUsers = new HashSet<string>(TestUsers().Where(x => x.AlreadyMapped).Select(x => x.Sid));

            _mappedUserFinderMock = new Mock<IMappedUserFinder>();
            _mappedUserFinderMock
                .Setup(x => x.AlreadyMappedSids(It.IsAny<IEnumerable<ISearchResult>>()))
                .Returns(alreadyMappedUsers);

            _activeDirectoryUserQuery = new ActiveDirectoryUserQuery(() => _directorySearcherBuilderMock.Object, _mappedUserFinderMock.Object);            
        }

        [Test]
        public void Run_ShouldBuildSearcherWithSearchRoot()
        {
            _activeDirectoryUserQuery.Run(_queryOptions);
            _directorySearcherBuilderMock.Verify(builder => builder.WithSearchRoot(_queryOptions.SearchRoot, _queryOptions.Credentials));
        }

        [Test]
        public void Run_ShouldBuildSearcherWithUserFilter()
        {
            _activeDirectoryUserQuery.Run(_queryOptions);
            _directorySearcherBuilderMock.Verify(builder => builder.WithUserFilter(_queryOptions.UserFilter));
        }

        [Test]
        public void Run_ShouldBuildSearcherWithUserSearchColumns()
        {
            _activeDirectoryUserQuery.Run(_queryOptions);
            _directorySearcherBuilderMock.Verify(builder => builder.WithUserSearchColumns());
        }

        [Test]
        public void Run_ShouldBuildSearcherSortingByCn()
        {
            _activeDirectoryUserQuery.Run(_queryOptions);
            _directorySearcherBuilderMock.Verify(builder => builder.WithSortByCn());
        }

        [Test]
        public void Run_ShouldBuildSearcherWithCorrectPaging()
        {
            _activeDirectoryUserQuery.Run(_queryOptions);
            _directorySearcherBuilderMock.Verify(builder => builder.WithPaging(_queryOptions.PageOptions));
        }

        [Test]
        public void Run_ShouldReturnUsersInQueryResult()
        {
            var queryResult = _activeDirectoryUserQuery.Run(_queryOptions);
            queryResult.RequestedPage.ShouldAllBeEquivalentTo(TestUsers());
        }

        [Test]
        public void Run_ShouldReturnApproximateTotalInQueryResult()
        {
            const int totalUsers = 456;
            _directorySearcherMock.Setup(searcher => searcher.ApproximateTotal).Returns(totalUsers);

            var queryResult = _activeDirectoryUserQuery.Run(_queryOptions);

            queryResult.TotalUsers.Should().Be(totalUsers);
        }

        [Test]
        public void Run_ShouldReturnSuccess()
        {            
            var queryResult = _activeDirectoryUserQuery.Run(_queryOptions);
            queryResult.Status.Should().Be(QueryStatus.Success);
        }


        [Test]
        public void Run_ThrowsInvalidUserNameAndPasswordException_ShouldReturnInvalidCredentials()
        {
            var invalidUserNameAndPasswordException = new DirectoryServicesCOMException("The user name or password is incorrect.");
            SetErrorCode(invalidUserNameAndPasswordException, -2147023570);
            _directorySearcherMock.Setup(searcher => searcher.FindAll()).Throws(invalidUserNameAndPasswordException);

            var queryResult = _activeDirectoryUserQuery.Run(_queryOptions);

            queryResult.Status.Should().Be(QueryStatus.InvalidCredentials);
        }

        [Test]
        public void Run_ThrowsOtherException_ShouldReturnInvalidCredentials()
        {
            var otherException = new DirectoryServicesCOMException("The directory service is unavailable.");
            SetErrorCode(otherException, -2147016689);
           _directorySearcherMock.Setup(searcher => searcher.FindAll()).Throws(otherException);

            var queryResult = _activeDirectoryUserQuery.Run(_queryOptions);

            queryResult.Status.Should().Be(QueryStatus.InvalidQuery);
        }

        [Test]
        public void Run_ShouldDisposeOfSearchResultCollection()
        {
            _activeDirectoryUserQuery.Run(_queryOptions);
            _searchResultCollectionMock.Verify(searchResult => searchResult.Dispose());
        }
               
        [Test]
        public void Run_ShouldDisposeOfDirectorSearcher()
        {
            _activeDirectoryUserQuery.Run(_queryOptions);
            _directorySearcherMock.Verify(searcher => searcher.Dispose());
        }
        
        private IEnumerable<ActiveDirectoryUser> TestUsers()
        {
            yield return new ActiveDirectoryUser("some.person@some.domain.com", "S-1-1-76-18423748-3438888550-264708130-6117", $"CN=some,CN=person,{SearchRoot}",  false);
            yield return new ActiveDirectoryUser("lazy.worker@some.domain.com", "S-1-1-76-18423748-3438888550-264708130-5999", $"CN=lazy,CN=worker,{SearchRoot}",  true);
            yield return new ActiveDirectoryUser("super.boss@some.domain.com", "S-1-1-76-18423748-3438888550-264708130-1987", $"CN=super,CN=boss,{SearchRoot}",  true);
        }


        private ISearchResult CreateSearchResultMock(ActiveDirectoryUser user)
        {
            var searchResultMock = new Mock<ISearchResult>();
            searchResultMock.Setup(searchResult => searchResult.UserPrincipalName).Returns(user.UserPrincipalName);
            searchResultMock.Setup(searchResult => searchResult.Sid).Returns(user.Sid);
            searchResultMock.Setup(searchResult => searchResult.DistinguishedName).Returns(user.DistinguishedName);
            return searchResultMock.Object;
        }

        private void SetErrorCode(Exception entry, int errorCode)
        {
            var method = typeof(Exception).GetMethod(
                      "SetErrorCode",
                      BindingFlags.NonPublic | BindingFlags.Instance,
                      null,
                      new Type[] { typeof(int) },
                      null);

            method.Invoke(entry, new object[] { errorCode });
        }
    }
}
