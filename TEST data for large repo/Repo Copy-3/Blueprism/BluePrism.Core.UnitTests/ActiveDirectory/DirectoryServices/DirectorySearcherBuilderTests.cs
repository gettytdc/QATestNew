using BluePrism.Common.Security;
using BluePrism.Core.ActiveDirectory;
using BluePrism.Core.ActiveDirectory.DirectoryServices;
using BluePrism.Core.ActiveDirectory.UserQuery;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.DirectoryServices;
using System.Reflection;

namespace BluePrism.Core.UnitTests.ActiveDirectory.DirectoryServices.UserQuery
{
    [TestFixture]
    public class DirectorySearcherBuilderTests
    {
        private Mock<IDirectorySearcher> _directorySearcher;
        private DirectorySearcherBuilder _directorySearcherBuilder;
        private readonly QueryPageOptions _pageOptions = new QueryPageOptions(50, 25);

        [SetUp]
        public void SetUp()
        {
            _directorySearcher = new Mock<IDirectorySearcher>();            
            Func<IDirectorySearcher> directorySearcherFactory = () => _directorySearcher.Object;
            _directorySearcherBuilder = new DirectorySearcherBuilder(directorySearcherFactory);
        }

        [Test]
        public void WithSearchRoot_PathHasValue_ShouldAddSearchRootWithPath()
        {
            const string searchRoot = "CN=admin,OU=system,DC=some,DC=domain,DC=COM";
            _directorySearcher.Setup(x => x.SearchRoot(It.IsAny<DirectoryEntry>()));
            
            _directorySearcherBuilder
                .WithSearchRoot(searchRoot, null); ;

            _directorySearcher
                .Verify(searcher => searcher.SearchRoot(It.Is<DirectoryEntry>(x => x.Path.Equals($"GC://{searchRoot}"))));
        }

        [Test]
        public void WithSearchRoot_CredentialsHasNoValue_ShouldAddSearchRootWithNoCredentialsSpecified()
        {
            const string searchRoot = "CN=admin,OU=system,DC=some,DC=domain,DC=COM";
            _directorySearcher.Setup(x => x.SearchRoot(It.IsAny<DirectoryEntry>()));

            _directorySearcherBuilder
                .WithSearchRoot(searchRoot, null); ;

            _directorySearcher
                .Verify(searcher => searcher.SearchRoot(It.Is<DirectoryEntry>(x => x.Username == null && GetPassword(x) == null)));
        }

        [Test]
        public void WithSearchRoot_NullPath_ShouldThrowException()
        {
            Action withSearchRoot = () => _directorySearcherBuilder.WithSearchRoot(null, null);
            withSearchRoot.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void WithSearchRoot_EmptyPath_ShouldThrowException()
        {
            Action withSearchRoot = () => _directorySearcherBuilder.WithSearchRoot(string.Empty, null);
            withSearchRoot.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void WithSearchRoot_CredentialsHasValue_ShouldAddSearchRootWithPath()
        {
            const string username = "some user";
            const string password = "password1234";
            const string searchRoot = "CN=admin,OU=system,DC=some,DC=domain,DC=COM";

            _directorySearcher.Setup(x => x.SearchRoot(It.IsAny<DirectoryEntry>()));

            _directorySearcherBuilder
                .WithSearchRoot(searchRoot, new DirectorySearcherCredentials(username, password.AsSecureString())); 

            _directorySearcher
                .Verify(searcher => searcher.SearchRoot(It.Is<DirectoryEntry>(x => x.Username == username && GetPassword(x) == password)));
        }

        [Test]
        public void WithUserFilter_ShouldSetLdapFilter()
        {
            const string filterValue = "filter value";
            _directorySearcher.Setup(x => x.Filter(It.IsAny<string>()));
            
            var userFilter = new UserFilter(UserFilterType.UserPrincipalName, filterValue);

            _directorySearcherBuilder.WithUserFilter(userFilter);

            _directorySearcher
                .Verify(searcher => searcher.Filter($"(&(objectCategory=user)(userPrincipalName={filterValue}))"));
        }

        [Test]
        public void WithUserFilter_NullFilter_ShouldThrowException()
        {
            Action withUserFilter = () => _directorySearcherBuilder.WithUserFilter(null);
            withUserFilter.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void WithSortByCn_ShouldSortByCnAscending()
        {
            _directorySearcher.Setup(x => x.Sort(It.IsAny<SortOption>()));                       

            _directorySearcherBuilder.WithSortByCn(); 

            _directorySearcher
                .Verify(searcher => searcher.Sort(It.Is<SortOption>(x => x.Direction == SortDirection.Ascending &&
                                                                         x.PropertyName == "cn")));
        }

        [Test]
        public void WithUserSearchColumns_ShouldAddCorrectColumns()
        {
            _directorySearcher.Setup(x => x.AddPropertyToLoad(It.IsAny<string>()));

            _directorySearcherBuilder.WithUserSearchColumns();

            _directorySearcher.Verify(searcher => searcher.AddPropertyToLoad("distinguishedName"));
            _directorySearcher.Verify(searcher => searcher.AddPropertyToLoad("objectSid"));
            _directorySearcher.Verify(searcher => searcher.AddPropertyToLoad("userPrincipalName"));
        }

        [Test]
        public void WithPaging_ShouldSetListViewWithCorrectAfterCount()
        {            
            _directorySearcher.Setup(x => x.VirtualListView(It.IsAny<DirectoryVirtualListView>()));

            _directorySearcherBuilder.WithPaging(_pageOptions);

            _directorySearcher
                .Verify(searcher => searcher.VirtualListView(It.Is<DirectoryVirtualListView>(x => x.AfterCount == _pageOptions.PageSize - 1)));
        }

        [Test]
        public void WithPaging_NullOptions_ShouldThrowException()
        {            
            Action withPaging = () => _directorySearcherBuilder.WithPaging(null);
            withPaging.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void WithPaging_ShouldSetListViewWithOffsetConvertedToOneBasedIndex()
        {
            _directorySearcher.Setup(x => x.VirtualListView(It.IsAny<DirectoryVirtualListView>()));

            _directorySearcherBuilder.WithPaging(_pageOptions);

            _directorySearcher
                .Verify(searcher => searcher.VirtualListView(It.Is<DirectoryVirtualListView>(x => x.Offset == _pageOptions.StartIndex + 1)));
        }

        [Test]
        public void WithPaging_ShouldSetListViewWithBeforeCountSetToZero()
        {
            _directorySearcher.Setup(x => x.VirtualListView(It.IsAny<DirectoryVirtualListView>()));

            _directorySearcherBuilder.WithPaging(_pageOptions);

            _directorySearcher
                .Verify(searcher => searcher.VirtualListView(It.Is<DirectoryVirtualListView>(x => x.BeforeCount == 0)));
        }

        [Test]
        public void WithBuild_ShouldReturnSearcherThatIsBeingBuilt()
        {           
            var searcher = _directorySearcherBuilder.Build();
            searcher.Should().BeSameAs(_directorySearcher.Object);
        }

        private string GetPassword(DirectoryEntry entry)
        {
            var method = typeof(DirectoryEntry).GetMethod(
                      "GetPassword",
                      BindingFlags.NonPublic | BindingFlags.Instance);

            return (string)method.Invoke(entry, null);
        }
    }
}
